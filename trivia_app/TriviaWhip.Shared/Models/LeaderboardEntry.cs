namespace TriviaWhip.Shared.Models;

public record LeaderboardEntry(string Player, int Score, DateTimeOffset When);
