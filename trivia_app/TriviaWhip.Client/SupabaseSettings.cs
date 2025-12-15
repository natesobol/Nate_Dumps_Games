namespace TriviaWhip.Client;

public class SupabaseSettings
{
    public string Url { get; set; } = string.Empty;
    public string AnonKey { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Url) &&
        !Url.Contains("YOUR-SUPABASE-URL", StringComparison.OrdinalIgnoreCase) &&
        !string.IsNullOrWhiteSpace(AnonKey) &&
        !AnonKey.Contains("YOUR_PUBLISHABLE_KEY", StringComparison.OrdinalIgnoreCase);
}
