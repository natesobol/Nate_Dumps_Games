namespace TriviaWhip.Shared.Models;

public class Session
{
    public List<Question> Questions { get; set; } = new();
    public int CurrentIndex { get; set; }
    public bool TimedMode { get; set; }
    public bool PracticeMode { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime TargetEndTime { get; set; }
    public int Lives { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public int Streak { get; set; }
    public bool IsComplete { get; set; }
    public int CoinsEarned { get; set; }
}
