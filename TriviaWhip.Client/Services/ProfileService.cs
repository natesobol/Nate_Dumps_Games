using System.Linq;
using Supabase;
using TriviaWhip.Shared.Models;

namespace TriviaWhip.Client.Services;

public class ProfileService
{
    private readonly Supabase.Client _supabase;
    private bool _initialized;

    public Profile Current { get; private set; } = new();
    public Settings CurrentSettings { get; private set; } = new();
    public event Action? Changed;

    public ProfileService(Supabase.Client supabase)
    {
        _supabase = supabase;
        Current.Milestones = BuildMilestones();
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            Changed?.Invoke();
            return;
        }

        var user = _supabase.Auth.CurrentUser ?? throw new InvalidOperationException("Not authenticated.");
        Guid.TryParse(user.Id, out var userId);

        var profileResponse = await _supabase.From<TriviaProfileRow>()
            .Where(row => row.ProfileId == userId)
            .Get();

        var row = profileResponse.Models.FirstOrDefault();
        if (row == null)
        {
            var defaultSettings = BuildDefaultSettings();
            row = new TriviaProfileRow
            {
                ProfileId = userId,
                TotalCoins = Current.Coins,
                Lives = (short)Current.Lives,
                Settings = defaultSettings,
                CategoriesSelected = defaultSettings.CategoriesSelected,
                SubCategoriesSelected = defaultSettings.SubCategoriesSelected,
                Achievements = Current.Achievements
            };
            await _supabase.From<TriviaProfileRow>().Insert(row);
        }

        MapFromRow(row);
        _initialized = true;
        Changed?.Invoke();
    }

    public async Task SaveAsync()
    {
        var user = _supabase.Auth.CurrentUser;
        if (user == null)
        {
            return;
        }

        RecalculateRatios();

        Guid.TryParse(user.Id, out var userId);
        var settingsPayload = BuildSettingsEnvelope();

        var row = new TriviaProfileRow
        {
            ProfileId = userId,
            TotalCoins = Current.Coins,

            BuffCoinMultiplier = ConvertToStoredMultiplier(Current.Buffs.CoinMultiplier),
            BuffCorrectMultiplier = ConvertToStoredMultiplier(Current.Buffs.CorrectMultiplier),
            BuffSkipDiscount = ConvertToStoredMultiplier(Current.Buffs.SkipCostMultiplier),
            ExtraLife = Current.Buffs.ExtraLife,
            IdolLevel = (short)Current.IdolLevel,
            Streak = Current.Streak,
            Lives = (short)Current.Lives,
            TotalCorrect = Current.CorrectCount,
            Level = Current.Level,
            MilestoneProgress = Current.MilestoneProgress,
            BestScore = Current.HighestGameScore,
            DevMode = false,
            LastEmail = CurrentSettings.LastEmailAddress,
            Achievements = Current.Achievements,
            RatioScores = Current.RatioScores,
            SubcategoryCorrectCounts = Current.SubcategoryCorrectCounts,
            SubcategoryWrongCounts = Current.SubcategoryWrongCounts,
            CategoryCorrectCounts = Current.CategoryCorrectCounts,
            CategoryWrongCounts = Current.CategoryWrongCounts,
            CategoriesSelected = settingsPayload.CategoriesSelected,
            SubCategoriesSelected = settingsPayload.SubCategoriesSelected,
            Settings = settingsPayload,
            TotalQuestionsAnswered = Current.TotalQuestionsAnswered,
            TotalSessionsPlayed = Current.TotalSessionsPlayed,
            HighestGameScore = Current.HighestGameScore,
            AverageGameScore = Current.AverageGameScore,
            MaxStreak = Current.MaxStreak,
            AverageAnswerTimeMs = Current.AverageAnswerTimeMs,
            FastestAnswerTimeMs = Current.FastestAnswerTimeMs,
            SlowestAnswerTimeMs = Current.SlowestAnswerTimeMs,
            BuffUsageCounts = Current.BuffUsageCounts,
            FirstPlayedAt = Current.FirstPlayedAt,
            LastPlayedAt = Current.LastPlayedAt,
            CoinsSpentGame = Current.CoinsSpentGame,
            CoinsSpentStore = Current.CoinsSpentStore
        };

        await _supabase.From<TriviaProfileRow>().Upsert(row);
        Changed?.Invoke();
    }

    private void RecalculateRatios()
    {
        var ratios = new Dictionary<string, double>();

        static void ApplyRatios(Dictionary<string, int> correct, Dictionary<string, int> wrong, Dictionary<string, double> target)
        {
            foreach (var kvp in correct)
            {
                var totalWrong = wrong.TryGetValue(kvp.Key, out var incorrect) ? incorrect : 0;
                var total = kvp.Value + totalWrong;
                if (total > 0)
                {
                    target[kvp.Key] = Math.Round((double)kvp.Value / total, 4);
                }
            }

            foreach (var kvp in wrong.Where(pair => !correct.ContainsKey(pair.Key)))
            {
                var total = kvp.Value;
                if (total > 0)
                {
                    target[kvp.Key] = 0;
                }
            }
        }

        ApplyRatios(Current.CategoryCorrectCounts, Current.CategoryWrongCounts, ratios);
        ApplyRatios(Current.SubcategoryCorrectCounts, Current.SubcategoryWrongCounts, ratios);

        Current.RatioScores = ratios;
    }

    private void MapFromRow(TriviaProfileRow row)
    {
        Current.Coins = row.TotalCoins;
        Current.Streak = row.Streak;
        Current.Lives = row.Lives;
        Current.Buffs = new Buff
        {
            CoinMultiplier = ConvertFromStoredMultiplier(row.BuffCoinMultiplier),
            CorrectMultiplier = ConvertFromStoredMultiplier(row.BuffCorrectMultiplier),
            SkipCostMultiplier = ConvertFromStoredMultiplier(row.BuffSkipDiscount),
            ExtraLife = row.ExtraLife
        };
        Current.IdolLevel = row.IdolLevel;
        Current.CorrectCount = row.TotalCorrect;
        Current.Level = row.Level;
        Current.Milestones = BuildMilestones();
        Current.Achievements = row.Achievements ?? new();
        Current.BestStreak = row.MaxStreak > 0 ? row.MaxStreak : row.BestScore;
        Current.MaxStreak = row.MaxStreak > 0 ? row.MaxStreak : row.BestScore;
        Current.MilestoneProgress = row.MilestoneProgress;
        Current.TotalQuestionsAnswered = row.TotalQuestionsAnswered;
        Current.TotalSessionsPlayed = row.TotalSessionsPlayed;
        Current.HighestGameScore = row.HighestGameScore > 0 ? row.HighestGameScore : row.BestScore;
        Current.AverageGameScore = row.AverageGameScore;
        Current.AverageAnswerTimeMs = row.AverageAnswerTimeMs;
        Current.FastestAnswerTimeMs = row.FastestAnswerTimeMs;
        Current.SlowestAnswerTimeMs = row.SlowestAnswerTimeMs;
        Current.FirstPlayedAt = row.FirstPlayedAt;
        Current.LastPlayedAt = row.LastPlayedAt;
        Current.RatioScores = row.RatioScores ?? new();
        Current.CategoryCorrectCounts = row.CategoryCorrectCounts ?? new();
        Current.CategoryWrongCounts = row.CategoryWrongCounts ?? new();
        Current.SubcategoryCorrectCounts = row.SubcategoryCorrectCounts ?? new();
        Current.SubcategoryWrongCounts = row.SubcategoryWrongCounts ?? new();
        Current.BuffUsageCounts = row.BuffUsageCounts ?? new();
        Current.CoinsSpentGame = row.CoinsSpentGame;
        Current.CoinsSpentStore = row.CoinsSpentStore;

        var settings = row.Settings ?? BuildDefaultSettings();
        EnsureSettingsDefaults(settings);
        CurrentSettings = settings;

        Current.OwnedAvatars = settings.OwnedAvatars;
        Current.OwnedSchemes = settings.OwnedSchemes;
        Current.OwnedBuffs = settings.OwnedBuffs;
        Current.HasSeenLivesTutorial = settings.HasSeenLivesTutorial;
        Current.HasSeenQuestionMarkTutorial = settings.HasSeenQuestionMarkTutorial;
        Current.HasSeenCategoryTutorial = settings.HasSeenCategoryTutorial;
        Current.IncorrectCount = settings.IncorrectCount;

        CurrentSettings.CategoriesSelected = settings.CategoriesSelected;
        CurrentSettings.SubCategoriesSelected = settings.SubCategoriesSelected;
    }

    private Settings BuildSettingsEnvelope()
    {
        EnsureSettingsDefaults(CurrentSettings);
        CurrentSettings.OwnedAvatars = Current.OwnedAvatars;
        CurrentSettings.OwnedSchemes = Current.OwnedSchemes;
        CurrentSettings.OwnedBuffs = Current.OwnedBuffs;
        CurrentSettings.HasSeenLivesTutorial = Current.HasSeenLivesTutorial;
        CurrentSettings.HasSeenQuestionMarkTutorial = Current.HasSeenQuestionMarkTutorial;
        CurrentSettings.HasSeenCategoryTutorial = Current.HasSeenCategoryTutorial;
        CurrentSettings.IncorrectCount = Current.IncorrectCount;
        return CurrentSettings;
    }

    private static void EnsureSettingsDefaults(Settings settings)
    {
        settings.CategoriesSelected ??= new();
        settings.SubCategoriesSelected ??= new();
        settings.OwnedAvatars ??= new() { 0 };
        settings.OwnedSchemes ??= new() { 0 };
        settings.OwnedBuffs ??= new();
    }

    private static Settings BuildDefaultSettings()
    {
        return new Settings
        {
            DarkTheme = false,
            TimerMode = false,
            AchievementMode = true,
            QuestionCount = 10,
            EnableQuestionButtons = true,
            AlphabeticalCategories = false,
            ViewCategoriesOnStart = false,
            OwnedSchemes = new() { 0 },
            OwnedAvatars = new() { 0 },
            OwnedBuffs = new(),
            CategoriesSelected = new(),
            SubCategoriesSelected = new(),
            LastEmailAddress = string.Empty
        };
    }

    private static List<int> BuildMilestones()
    {
        return new() { 5, 15, 30, 50, 80, 120, 170, 230, 300 };
    }

    private static double ConvertFromStoredMultiplier(int stored)
    {
        if (stored <= 1)
        {
            return 1d;
        }

        return stored / 100d;
    }

    private static int ConvertToStoredMultiplier(double multiplier)
    {
        return (int)Math.Round(multiplier * 100);
    }
}
