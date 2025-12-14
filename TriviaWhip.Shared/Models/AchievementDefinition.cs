namespace TriviaWhip.Shared.Models;

public class AchievementDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Func<Profile, bool> Condition { get; set; } = _ => false;
}
