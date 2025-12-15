namespace TriviaWhip.Shared.Models;

public class Buff
{
    public double CoinMultiplier { get; set; } = 1.0;
    public double CorrectMultiplier { get; set; } = 1.0;
    public double SkipCostMultiplier { get; set; } = 1.0;
    public bool ExtraLife { get; set; }
    public int Id { get; set; }
}
