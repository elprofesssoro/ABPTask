using api.Options;
using Microsoft.Extensions.Options;

namespace api.Services;

public class DiscountService(IOptions<PriceRules> options) : IDiscountService
{
    private readonly List<TimeFrameOption> _timeFrames = options.Value.TimeFrames;

    public decimal CalculateDiscountPrice(decimal costPerHour, DateTime startTime, DateTime endTime)
    {
        decimal durationHours = (decimal)(endTime - startTime).TotalHours;
        decimal baseCost = costPerHour * durationHours;

        decimal multiplier = FindMatchingFrame(startTime, endTime)?.Multiplier ?? 1.0m;

        return Math.Round(baseCost * multiplier, 2);
    }

    private TimeFrameOption? FindMatchingFrame(DateTime startTime, DateTime endTime)
    {
        TimeOnly start = TimeOnly.FromDateTime(startTime);
        TimeOnly end = TimeOnly.FromDateTime(endTime);

        return _timeFrames.FirstOrDefault(band => start >= band.Start && end <= band.End);
    }
}