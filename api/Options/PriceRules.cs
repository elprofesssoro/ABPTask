namespace api.Options;

public class PriceRules
{
    public List<TimeFrameOption> TimeFrames { get; set; } = new();
}

public class TimeFrameOption
{
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
    public decimal Multiplier { get; set; } = 1.0m;
}