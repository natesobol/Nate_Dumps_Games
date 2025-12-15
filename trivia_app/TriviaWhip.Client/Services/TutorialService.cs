using TriviaWhip.Shared.Models;

namespace TriviaWhip.Client.Services;

public class TutorialService
{
    private readonly ProfileService _profileService;

    public TutorialService(ProfileService profileService)
    {
        _profileService = profileService;
    }

    public bool ShouldShowLivesTutorial() => !_profileService.Current.HasSeenLivesTutorial;
    public bool ShouldShowQuestionMarkTutorial() => !_profileService.Current.HasSeenQuestionMarkTutorial;
    public bool ShouldShowCategoryTutorial() => !_profileService.Current.HasSeenCategoryTutorial;

    public void DismissLivesTutorial() => _profileService.Current.HasSeenLivesTutorial = true;
    public void DismissQuestionMarkTutorial() => _profileService.Current.HasSeenQuestionMarkTutorial = true;
    public void DismissCategoryTutorial() => _profileService.Current.HasSeenCategoryTutorial = true;
}
