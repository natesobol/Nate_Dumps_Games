using System;
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
            Console.Error.WriteLine("No questions available after applying filters.");
            CurrentSession = new Session { Questions = new List<Question>(), IsComplete = true };
            StateChanged?.Invoke();
            return;
        }

        Console.WriteLine($"Starting new session with {shuffled.Count} questions. Timer mode: {settings.TimerMode}");
        CurrentSession = new Session
        {
            Questions = shuffled,
            CurrentIndex = 0,
            TimedMode = settings.TimerMode,
            Lives = Math.Max(profile.Lives, 1),
            StartTime = DateTime.UtcNow,
            TargetEndTime = DateTime.UtcNow.AddSeconds(settings.TimerMode ? 20 : 300),
            CurrentQuestionStart = DateTime.UtcNow
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
        var elapsedMs = (int)Math.Max(0, (DateTime.UtcNow - CurrentSession.CurrentQuestionStart).TotalMilliseconds);
        RecordAnswerTime(elapsedMs);

        if (correct)
        {
            CurrentSession.CorrectCount++;
            CurrentSession.Streak++;
            CurrentSession.MaxStreak = Math.Max(CurrentSession.MaxStreak, CurrentSession.Streak);
            _profileService.Current.CorrectCount++;
            _profileService.Current.Streak++;
            _profileService.Current.BestStreak = Math.Max(_profileService.Current.BestStreak, _profileService.Current.Streak);
            _profileService.Current.MaxStreak = Math.Max(_profileService.Current.MaxStreak, _profileService.Current.Streak);
            var coins = (int)(10 * _profileService.Current.Buffs.CoinMultiplier);
            CurrentSession.CoinsEarned += coins;
            _profileService.Current.Coins += coins;
            RecordCategoryResult(true);
        }
        else
        {
            CurrentSession.IncorrectCount++;
            _profileService.Current.IncorrectCount++;
            _profileService.Current.Streak = 0;
            CurrentSession.Streak = 0;
            if (CurrentSession.Lives > 0)
            {
                CurrentSession.Lives -= 1;
            }
            RecordCategoryResult(false);
        }

        _profileService.Current.TotalQuestionsAnswered++;
        UpdateAnswerAverages(elapsedMs);

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
        RecordBuffUsage("skip");
        StateChanged?.Invoke();
    }

    public async Task NextQuestionAsync()
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
            FinalizeSessionStats();
            await _profileService.SaveAsync();
            StateChanged?.Invoke();
            return;
        }

        CurrentSession.CurrentIndex++;
        CurrentSession.CurrentQuestionStart = DateTime.UtcNow;
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

    private void RecordAnswerTime(int elapsedMs)
    {
        if (CurrentSession is null)
        {
            return;
        }

        CurrentSession.AnswerTimesMs.Add(elapsedMs);
        CurrentSession.CurrentQuestionStart = DateTime.UtcNow;
    }

    private void UpdateAnswerAverages(int elapsedMs)
    {
        var totalAnswers = _profileService.Current.TotalQuestionsAnswered;
        if (totalAnswers <= 0)
        {
            return;
        }

        if (!_profileService.Current.AverageAnswerTimeMs.HasValue)
        {
            _profileService.Current.AverageAnswerTimeMs = elapsedMs;
        }
        else
        {
            var previousAverage = _profileService.Current.AverageAnswerTimeMs.Value;
            _profileService.Current.AverageAnswerTimeMs = (int)Math.Round(((double)previousAverage * (totalAnswers - 1) + elapsedMs) / totalAnswers);
        }

        if (!_profileService.Current.FastestAnswerTimeMs.HasValue || elapsedMs < _profileService.Current.FastestAnswerTimeMs)
        {
            _profileService.Current.FastestAnswerTimeMs = elapsedMs;
        }

        if (!_profileService.Current.SlowestAnswerTimeMs.HasValue || elapsedMs > _profileService.Current.SlowestAnswerTimeMs)
        {
            _profileService.Current.SlowestAnswerTimeMs = elapsedMs;
        }
    }

    private void RecordCategoryResult(bool correct)
    {
        if (CurrentQuestion is null || CurrentSession is null)
        {
            return;
        }

        var (categorySlug, subSlug) = CategoryCatalog.NormalizeQuestionCategory(CurrentQuestion);
        UpdateCountDictionary(correct ? CurrentSession.CategoryCorrectCounts : CurrentSession.CategoryWrongCounts, categorySlug);
        UpdateCountDictionary(correct ? CurrentSession.SubcategoryCorrectCounts : CurrentSession.SubcategoryWrongCounts, subSlug);
        UpdateCountDictionary(correct ? _profileService.Current.CategoryCorrectCounts : _profileService.Current.CategoryWrongCounts, categorySlug);
        UpdateCountDictionary(correct ? _profileService.Current.SubcategoryCorrectCounts : _profileService.Current.SubcategoryWrongCounts, subSlug);
    }

    private static void UpdateCountDictionary(IDictionary<string, int> map, string key, int increment = 1)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        map.TryGetValue(key, out var current);
        map[key] = current + increment;
    }

    private void RecordBuffUsage(string buffKey)
    {
        if (CurrentSession is not null)
        {
            UpdateCountDictionary(CurrentSession.BuffUsageCounts, buffKey);
        }
        UpdateCountDictionary(_profileService.Current.BuffUsageCounts, buffKey);
    }

    private void FinalizeSessionStats()
    {
        if (CurrentSession is null)
        {
            return;
        }

        _profileService.Current.TotalSessionsPlayed++;
        _profileService.Current.FirstPlayedAt ??= CurrentSession.StartTime;
        _profileService.Current.LastPlayedAt = DateTime.UtcNow;

        var score = CurrentSession.CorrectCount;
        if (score > _profileService.Current.HighestGameScore)
        {
            _profileService.Current.HighestGameScore = score;
        }

        if (_profileService.Current.TotalSessionsPlayed > 0)
        {
            var sessionCount = _profileService.Current.TotalSessionsPlayed;
            _profileService.Current.AverageGameScore = ((_profileService.Current.AverageGameScore * (sessionCount - 1)) + score) / sessionCount;
        }

        _profileService.Current.MaxStreak = Math.Max(_profileService.Current.MaxStreak, CurrentSession.MaxStreak);
        _profileService.Current.BestStreak = _profileService.Current.MaxStreak;
        _profileService.Current.MilestoneProgress = _profileService.Current.CorrectCount;

        MergeDictionaries(_profileService.Current.BuffUsageCounts, CurrentSession.BuffUsageCounts);
    }

    private static void MergeDictionaries(IDictionary<string, int> target, IDictionary<string, int> source)
    {
        foreach (var kvp in source)
        {
            UpdateCountDictionary(target, kvp.Key, kvp.Value);
        }
    }
}
