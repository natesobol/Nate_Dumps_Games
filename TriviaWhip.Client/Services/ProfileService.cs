using System.Text.Json;
using Microsoft.JSInterop;
using TriviaWhip.Shared.Models;

namespace TriviaWhip.Client.Services;

public class ProfileService
{
    private const string StorageKey = "triviawhip-profile";
    private readonly IJSRuntime _jsRuntime;

    public Profile Current { get; private set; } = new();
    public event Action? Changed;

    public ProfileService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        Current.Milestones = BuildMilestones();
    }

    public async Task InitializeAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var restored = JsonSerializer.Deserialize<Profile>(json);
                if (restored is not null)
                {
                    Current = restored;
                    if (!Current.Milestones.Any())
                    {
                        Current.Milestones = BuildMilestones();
                    }
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load profile from local storage: {ex.Message}");
        }

        Current.Milestones = BuildMilestones();
        Current.OwnedSchemes.Add(0);
        await SaveAsync();
    }

    public async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(Current);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to persist profile: {ex.Message}");
        }

        Changed?.Invoke();
    }

    private static List<int> BuildMilestones()
    {
        return new() { 5, 15, 30, 50, 80, 120, 170, 230, 300 };
    }
}
