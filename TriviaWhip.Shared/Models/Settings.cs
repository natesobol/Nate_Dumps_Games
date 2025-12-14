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
}
