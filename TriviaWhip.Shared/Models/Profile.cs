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
    public int MaxStreak { get; set; }
    public bool HasSeenLivesTutorial { get; set; }
    public bool HasSeenQuestionMarkTutorial { get; set; }
    public bool HasSeenCategoryTutorial { get; set; }
    public int MilestoneProgress { get; set; }
    public int TotalQuestionsAnswered { get; set; }
    public int TotalSessionsPlayed { get; set; }
    public int HighestGameScore { get; set; }
    public double AverageGameScore { get; set; }
    public int? AverageAnswerTimeMs { get; set; }
    public int? FastestAnswerTimeMs { get; set; }
    public int? SlowestAnswerTimeMs { get; set; }
    public DateTime? FirstPlayedAt { get; set; }
    public DateTime? LastPlayedAt { get; set; }
    public int CoinsSpentStore { get; set; }
    public int CoinsSpentGame { get; set; }
    public Dictionary<string, int> CategoryCorrectCounts { get; set; } = new();
    public Dictionary<string, int> CategoryWrongCounts { get; set; } = new();
    public Dictionary<string, int> SubcategoryCorrectCounts { get; set; } = new();
    public Dictionary<string, int> SubcategoryWrongCounts { get; set; } = new();
    public Dictionary<string, int> BuffUsageCounts { get; set; } = new();
    public Dictionary<string, double> RatioScores { get; set; } = new();
}
