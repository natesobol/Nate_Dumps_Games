namespace TriviaWhip.Shared.Models;

public class Profile
{
    public int Coins { get; set; }
    public int Streak { get; set; }
    public int Lives { get; set; } = 5;
    public Buff Buffs { get; set; } = new();
    public int IdolLevel { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public Dictionary<string, bool> Achievements { get; set; } = new();
    public int Level { get; set; } = 1;
    public List<int> Milestones { get; set; } = new();
    public HashSet<int> OwnedAvatars { get; set; } = new() { 0 };
    public HashSet<int> OwnedSchemes { get; set; } = new() { 0 };
    public HashSet<int> OwnedBuffs { get; set; } = new();
    public int BestStreak { get; set; }
    public bool HasSeenLivesTutorial { get; set; }
    public bool HasSeenQuestionMarkTutorial { get; set; }
    public bool HasSeenCategoryTutorial { get; set; }
}
