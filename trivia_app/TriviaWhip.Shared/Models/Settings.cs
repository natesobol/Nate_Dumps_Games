namespace TriviaWhip.Shared.Models;

public class Settings
{
    public bool DarkTheme { get; set; }
    public bool TimerMode { get; set; }
    public bool AchievementMode { get; set; }
    public int QuestionCount { get; set; } = 10;
    public bool AdsRemoved { get; set; }
    public int SchemeColor { get; set; } = 0;
    public int Avatar { get; set; } = 0;
    public bool Mute { get; set; }
    public bool EnableQuestionButtons { get; set; }
    public int PickerChoice { get; set; } = 0;
    public bool ViewCategoriesOnStart { get; set; }
    public bool AlphabeticalCategories { get; set; }
    public HashSet<string> CategoriesSelected { get; set; } = new();
    public HashSet<string> SubCategoriesSelected { get; set; } = new();
    public string LastEmailAddress { get; set; } = string.Empty;

    // Persist profile-owned assets and tutorial flags alongside settings to align with Supabase storage.
    public HashSet<int> OwnedAvatars { get; set; } = new() { 0 };
    public HashSet<int> OwnedSchemes { get; set; } = new() { 0 };
    public HashSet<int> OwnedBuffs { get; set; } = new();
    public bool HasSeenLivesTutorial { get; set; }
    public bool HasSeenQuestionMarkTutorial { get; set; }
    public bool HasSeenCategoryTutorial { get; set; }
    public int IncorrectCount { get; set; }
}
