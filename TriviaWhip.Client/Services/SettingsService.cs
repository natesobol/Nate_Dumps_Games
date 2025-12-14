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
        var json = JsonSerializer.Serialize(Current);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        Changed?.Invoke();
    }
}
