using TriviaWhip.Shared.Models;

namespace TriviaWhip.Client.Services;

public class SettingsService
{
    private readonly ProfileService _profileService;

    public Settings Current => _profileService.CurrentSettings;
    public event Action? Changed;

    public SettingsService(ProfileService profileService)
    {
        _profileService = profileService;
        _profileService.Changed += () => Changed?.Invoke();
    }

    public async Task InitializeAsync()
    {
        await _profileService.InitializeAsync();
    }

    public async Task SaveAsync()
    {
        await _profileService.SaveAsync();
    }
}
