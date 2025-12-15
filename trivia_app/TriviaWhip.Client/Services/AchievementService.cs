using TriviaWhip.Shared.Models;

namespace TriviaWhip.Client.Services;

public class AchievementService
{
    private readonly List<AchievementDefinition> _definitions = new();

    public AchievementService()
    {
        _definitions.AddRange(new[]
        {
            new AchievementDefinition { Id = "correct_5", Description = "Answer 5 questions correctly", Condition = p => p.CorrectCount >= 5 },
            new AchievementDefinition { Id = "correct_20", Description = "Answer 20 questions correctly", Condition = p => p.CorrectCount >= 20 },
            new AchievementDefinition { Id = "correct_100", Description = "Answer 100 questions correctly", Condition = p => p.CorrectCount >= 100 },
            new AchievementDefinition { Id = "streak_5", Description = "Earn a 5 answer streak", Condition = p => p.BestStreak >= 5 },
            new AchievementDefinition { Id = "streak_10", Description = "Earn a 10 answer streak", Condition = p => p.BestStreak >= 10 },
            new AchievementDefinition { Id = "coins_500", Description = "Collect 500 coins", Condition = p => p.Coins >= 500 }
        });
    }

    public IReadOnlyList<string> CheckAchievements(Profile profile)
    {
        var unlocked = new List<string>();
        foreach (var definition in _definitions)
        {
            var already = profile.Achievements.TryGetValue(definition.Id, out var done) && done;
            if (!already && definition.Condition(profile))
            {
                profile.Achievements[definition.Id] = true;
                unlocked.Add(definition.Id);
            }
        }
        return unlocked;
    }
}
