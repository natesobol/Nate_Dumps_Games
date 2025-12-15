using System;
using System.Collections.Generic;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TriviaWhip.Shared.Models;

[Table("profiles")]
public class ProfileRow : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("country")]
    public string? Country { get; set; }

    [Column("payment_provider")]
    public string? PaymentProvider { get; set; }

    [Column("payment_reference")]
    public string? PaymentReference { get; set; }
}

[Table("trivia_profiles")]
public class TriviaProfileRow : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("profile_id")]
    public Guid ProfileId { get; set; }

    [Column("total_coins")]
    public int TotalCoins { get; set; }

    [Column("coins_spent_store")]
    public int CoinsSpentStore { get; set; }

    [Column("coins_spent_game")]
    public int CoinsSpentGame { get; set; }

    [Column("buff_coin_multiplier")]
    public int BuffCoinMultiplier { get; set; }

    [Column("buff_correct_multiplier")]
    public int BuffCorrectMultiplier { get; set; }

    [Column("buff_skip_discount")]
    public int BuffSkipDiscount { get; set; }

    [Column("extra_life")]
    public bool ExtraLife { get; set; }

    [Column("idol_level")]
    public short IdolLevel { get; set; }

    [Column("streak")]
    public int Streak { get; set; }

    [Column("lives")]
    public short Lives { get; set; }

    [Column("total_correct")]
    public int TotalCorrect { get; set; }

    [Column("level")]
    public int Level { get; set; }

    [Column("milestone_progress")]
    public int MilestoneProgress { get; set; }

    [Column("best_score")]
    public int BestScore { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("dev_mode")]
    public bool DevMode { get; set; }

    [Column("last_email")]
    public string? LastEmail { get; set; }

    [Column("achievements")]
    public Dictionary<string, bool>? Achievements { get; set; }

    [Column("ratio_scores")]
    public Dictionary<string, double>? RatioScores { get; set; }

    [Column("subcat_correct_counts")]
    public Dictionary<string, int>? SubcategoryCorrectCounts { get; set; }

    [Column("subcat_wrong_counts")]
    public Dictionary<string, int>? SubcategoryWrongCounts { get; set; }

    [Column("category_correct_counts")]
    public Dictionary<string, int>? CategoryCorrectCounts { get; set; }

    [Column("category_wrong_counts")]
    public Dictionary<string, int>? CategoryWrongCounts { get; set; }

    [Column("selected_categories")]
    public HashSet<string>? CategoriesSelected { get; set; }

    [Column("selected_subcategories")]
    public HashSet<string>? SubCategoriesSelected { get; set; }

    [Column("settings")]
    public Settings? Settings { get; set; }

    [Column("total_questions_answered")]
    public int TotalQuestionsAnswered { get; set; }

    [Column("total_sessions_played")]
    public int TotalSessionsPlayed { get; set; }

    [Column("highest_game_score")]
    public int HighestGameScore { get; set; }

    [Column("average_game_score")]
    public double AverageGameScore { get; set; }

    [Column("max_streak")]
    public int MaxStreak { get; set; }

    [Column("average_answer_time_ms")]
    public int? AverageAnswerTimeMs { get; set; }

    [Column("fastest_answer_time_ms")]
    public int? FastestAnswerTimeMs { get; set; }

    [Column("slowest_answer_time_ms")]
    public int? SlowestAnswerTimeMs { get; set; }

    [Column("buff_usage_counts")]
    public Dictionary<string, int>? BuffUsageCounts { get; set; }

    [Column("first_played_at")]
    public DateTime? FirstPlayedAt { get; set; }

    [Column("last_played_at")]
    public DateTime? LastPlayedAt { get; set; }
}
