namespace TriviaWhip.Shared.Models;

public class Question
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public List<string> Choices { get; set; } = new();
    public int AnswerIndex { get; set; }
}
