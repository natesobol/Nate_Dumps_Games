using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using Supabase.Gotrue;
using TriviaWhip.Shared.Models;
using GotrueSession = Supabase.Gotrue.Session;

namespace TriviaWhip.Client.Services;

public class AuthService
{
    private readonly Supabase.Client _supabase;
    private bool _initialized;

    public GotrueSession? CurrentSession { get; private set; }
    public ProfileRow? Profile { get; private set; }
    public event Action? AuthStateChanged;

    public bool IsAuthenticated => CurrentSession?.User != null || _supabase.Auth.CurrentUser != null;
    public string? CurrentEmail => CurrentSession?.User?.Email ?? _supabase.Auth.CurrentUser?.Email;

    public AuthService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task InitializeAsync()
    {
        await EnsureClientReadyAsync();
        CurrentSession ??= _supabase.Auth.CurrentSession;

        if (CurrentSession?.User != null)
        {
            await LoadProfileAsync();
            AuthStateChanged?.Invoke();
        }
    }

    public async Task<AuthResult> SignInAsync(string usernameOrEmail, string password)
    {
        await EnsureClientReadyAsync();

        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Failure("Please enter your username and password.");
        }

        var email = await ResolveEmailAsync(usernameOrEmail);
        if (string.IsNullOrWhiteSpace(email))
        {
            return AuthResult.Failure("We couldn't find that account. Try creating one.");
        }

        var session = await _supabase.Auth.SignIn(email, password);
        if (session?.User == null)
        {
            return AuthResult.Failure("Incorrect username or password.");
        }

        CurrentSession = session;
        await LoadProfileAsync();
        AuthStateChanged?.Invoke();
        return AuthResult.Success();
    }

    public async Task<AuthResult> SignUpAsync(string email, string username, string password)
    {
        await EnsureClientReadyAsync();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Failure("Please fill in email, username, and password.");
        }

        var signUpOptions = new SignUpOptions
        {
            Data = new Dictionary<string, object>
            {
                ["username"] = username
            }
        };

        var session = await _supabase.Auth.SignUp(email, password, signUpOptions);
        if (session?.User == null)
        {
            return AuthResult.Failure("We couldn't create your account yet. Try again.");
        }

        CurrentSession = session;
        await CreateProfileAsync(email, username);
        await LoadProfileAsync();
        AuthStateChanged?.Invoke();
        return AuthResult.Success();
    }

    public async Task<AuthResult> UpdatePasswordAsync(string newPassword)
    {
        await EnsureClientReadyAsync();

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return AuthResult.Failure("Enter a password before saving.");
        }

        if (CurrentSession == null)
        {
            return AuthResult.Failure("Log in before updating your password.");
        }

        var result = await _supabase.Auth.Update(new UserAttributes
        {
            Password = newPassword
        });

        return result == null
            ? AuthResult.Failure("Password change was not saved.")
            : AuthResult.Success();
    }

    public async Task SignOutAsync()
    {
        if (!_initialized)
        {
            return;
        }

        await _supabase.Auth.SignOut();
        CurrentSession = null;
        Profile = null;
        AuthStateChanged?.Invoke();
    }

    private async Task<string?> ResolveEmailAsync(string usernameOrEmail)
    {
        if (usernameOrEmail.Contains('@'))
        {
            return usernameOrEmail;
        }

        var response = await _supabase.From<ProfileRow>()
            .Where(row => row.Name == usernameOrEmail)
            .Get();

        var match = response.Models.FirstOrDefault();
        return match?.Email;
    }

    private async Task CreateProfileAsync(string email, string username)
    {
        var user = CurrentSession?.User ?? _supabase.Auth.CurrentUser;
        if (user?.Id == null)
        {
            return;
        }

        Guid.TryParse(user.Id, out var id);

        var profileRow = new ProfileRow
        {
            Id = id,
            Email = email,
            Name = username
        };

        await _supabase.From<ProfileRow>().Upsert(profileRow);
    }

    private async Task LoadProfileAsync()
    {
        var user = CurrentSession?.User ?? _supabase.Auth.CurrentUser;
        if (user?.Id == null)
        {
            Profile = null;
            return;
        }

        Guid.TryParse(user.Id, out var id);
        var response = await _supabase.From<ProfileRow>()
            .Where(row => row.Id == id)
            .Get();

        Profile = response.Models.FirstOrDefault();
    }

    private async Task EnsureClientReadyAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _supabase.InitializeAsync();
        _initialized = true;
    }
}

public record AuthResult(bool Succeeded, string? Error)
{
    public static AuthResult Success() => new(true, null);
    public static AuthResult Failure(string message) => new(false, message);
}
