namespace TriviaWhip.Shared.Models;

public class Session
{
    public List<Question> Questions { get; set; } = new();
    public int CurrentIndex { get; set; }
    public bool TimedMode { get; set; }
    public bool PracticeMode { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime TargetEndTime { get; set; }
    public DateTime CurrentQuestionStart { get; set; }
    public int Lives { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public int Streak { get; set; }
    public int MaxStreak { get; set; }
    public bool IsComplete { get; set; }
    public int CoinsEarned { get; set; }
    public List<int> AnswerTimesMs { get; set; } = new();
    public Dictionary<string, int> CategoryCorrectCounts { get; set; } = new();
    public Dictionary<string, int> CategoryWrongCounts { get; set; } = new();
    public Dictionary<string, int> SubcategoryCorrectCounts { get; set; } = new();
    public Dictionary<string, int> SubcategoryWrongCounts { get; set; } = new();
    public Dictionary<string, int> BuffUsageCounts { get; set; } = new();
}
