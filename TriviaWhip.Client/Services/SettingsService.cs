using System.Text.Json;
using Microsoft.JSInterop;
using TriviaWhip.Shared.Models;

namespace TriviaWhip.Client.Services;

public class SettingsService
{
    private const string StorageKey = "triviawhip-settings";
    private readonly IJSRuntime _jsRuntime;

    public Settings Current { get; private set; } = new();
    public event Action? Changed;

    public SettingsService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var restored = JsonSerializer.Deserialize<Settings>(json);
                if (restored is not null)
                {
                    Current = restored;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load settings from local storage: {ex.Message}");
        }

        Current = new Settings
        {
            DarkTheme = false,
            TimerMode = false,
            AchievementMode = true,
            QuestionCount = 10,
            EnableQuestionButtons = true,
            AlphabeticalCategories = false,
            ViewCategoriesOnStart = false
        };
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
            Console.Error.WriteLine($"Failed to persist settings: {ex.Message}");
        }

        Changed?.Invoke();
    }
}
