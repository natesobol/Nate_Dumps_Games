using TriviaWhip.Shared.Models;

namespace TriviaWhip.Client.Services;

public class GameEngine
{
    private readonly QuestionService _questionService;
    private readonly SettingsService _settingsService;
    private readonly ProfileService _profileService;
    private readonly AchievementService _achievementService;

    public Session? CurrentSession { get; private set; }
    public Question? CurrentQuestion => CurrentSession is null || !CurrentSession.Questions.Any()
        ? null
        : CurrentSession.Questions[CurrentSession.CurrentIndex];
    public HashSet<int> HiddenChoices { get; } = new();

    public event Action? StateChanged;

    public GameEngine(QuestionService questionService, SettingsService settingsService, ProfileService profileService, AchievementService achievementService)
    {
        _questionService = questionService;
        _settingsService = settingsService;
        _profileService = profileService;
        _achievementService = achievementService;
    }

    public async Task StartGameAsync()
    {
        var settings = _settingsService.Current;
        var profile = _profileService.Current;
        var filtered = await _questionService.GetFilteredAsync(settings);
        var shuffled = QuestionService.Shuffle(filtered).Take(Math.Max(1, settings.QuestionCount)).ToList();
        if (!shuffled.Any())
        {
            CurrentSession = new Session { Questions = new List<Question>(), IsComplete = true };
            StateChanged?.Invoke();
            return;
        }

        CurrentSession = new Session
        {
            Questions = shuffled,
            CurrentIndex = 0,
            TimedMode = settings.TimerMode,
            Lives = Math.Max(profile.Lives, 1),
            StartTime = DateTime.UtcNow,
            TargetEndTime = DateTime.UtcNow.AddSeconds(settings.TimerMode ? 20 : 300)
        };
        HiddenChoices.Clear();
        StateChanged?.Invoke();
    }

    public bool SubmitAnswer(int choiceIndex)
    {
        if (CurrentSession is null || CurrentQuestion is null || CurrentSession.IsComplete)
        {
            return false;
        }

        var correct = choiceIndex == CurrentQuestion.AnswerIndex;
        if (correct)
        {
            CurrentSession.CorrectCount++;
            _profileService.Current.CorrectCount++;
            _profileService.Current.Streak++;
            _profileService.Current.BestStreak = Math.Max(_profileService.Current.BestStreak, _profileService.Current.Streak);
            var coins = (int)(10 * _profileService.Current.Buffs.CoinMultiplier);
            CurrentSession.CoinsEarned += coins;
            _profileService.Current.Coins += coins;
        }
        else
        {
            CurrentSession.IncorrectCount++;
            _profileService.Current.IncorrectCount++;
            _profileService.Current.Streak = 0;
            if (CurrentSession.Lives > 0)
            {
                CurrentSession.Lives -= 1;
            }
        }

        _achievementService.CheckAchievements(_profileService.Current);
        HiddenChoices.Clear();
        StateChanged?.Invoke();
        return correct;
    }

    public void UseSkip()
    {
        if (CurrentQuestion is null || HiddenChoices.Any())
        {
            return;
        }

        var rng = new Random();
        var wrongIndexes = Enumerable.Range(0, CurrentQuestion.Choices.Count)
            .Where(i => i != CurrentQuestion.AnswerIndex)
            .OrderBy(_ => rng.Next())
            .Take(2);
        foreach (var index in wrongIndexes)
        {
            HiddenChoices.Add(index);
        }
        StateChanged?.Invoke();
    }

    public void NextQuestion()
    {
        if (CurrentSession is null)
        {
            return;
        }

        if (CurrentSession.CurrentIndex + 1 >= CurrentSession.Questions.Count || CurrentSession.Lives <= 0)
        {
            CurrentSession.IsComplete = true;
            _profileService.Current.Level = CalculateLevel(_profileService.Current.CorrectCount);
            _profileService.Current.Lives = Math.Max(CurrentSession.Lives, 1);
            _profileService.SaveAsync();
            StateChanged?.Invoke();
            return;
        }

        CurrentSession.CurrentIndex++;
        HiddenChoices.Clear();
        StateChanged?.Invoke();
    }

    private static int CalculateLevel(int correct)
    {
        if (correct < 5) return 1;
        if (correct < 15) return 2;
        if (correct < 30) return 3;
        if (correct < 50) return 4;
        if (correct < 80) return 5;
        if (correct < 120) return 6;
        if (correct < 170) return 7;
        if (correct < 230) return 8;
        if (correct < 300) return 9;
        return 10;
    }
}
