using ErrorOr;
using api.Data;
using api.DTO.Reports;
using Microsoft.EntityFrameworkCore;
namespace api.Services;

public class ReportService(AppDbContext _context) : IReportService
{
    public async Task<ErrorOr<HallSummaryResponse>> GetHallsSummaryAsync(DateRange filter)
    {
        // Default to current month if dates are omitted
        DateTime fromUtc = filter.FromDate?.UtcDateTime
                ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        DateTime toUtc = filter.ToDate?.UtcDateTime
            ?? DateTime.UtcNow;

        if (fromUtc > toUtc)
            return Error.Validation("Report.InvalidDateRange", "FromDate cannot be later than ToDate.");

        // Query all halls so even halls with 0 bookings are represented
        var hallDb = await _context.Halls
            .AsNoTracking()
            .Select(h => new
            {
                h.Id,
                h.Name,
                Bookings = _context.Bookings
                    .Where(b => b.HallId == h.Id && b.StartTime >= fromUtc && b.EndTime <= toUtc)
                    .Select(b => new { b.TotalCost })
                    .ToList()
            }).ToListAsync();

        List<HallPerformanceDTO> hallDtos = hallDb.Select(h =>
        {
            int count = h.Bookings.Count;
            decimal revenue = h.Bookings.Sum(b => b.TotalCost);
            decimal avg = count > 0 ? Math.Round(revenue / count, 2) : 0m;

            return new HallPerformanceDTO(
                HallId: h.Id,
                HallName: h.Name,
                BookingsCount: count,
                TotalRevenue: Math.Round(revenue, 2),
                AverageBookingValue: avg
            );
        }).OrderByDescending(h => h.TotalRevenue).ToList();

        int overallBookings = hallDtos.Sum(h => h.BookingsCount);
        decimal overallRevenue = hallDtos.Sum(h => h.TotalRevenue);

        return new HallSummaryResponse(
            FromDate: fromUtc,
            ToDate: toUtc,
            TotalBookings: overallBookings,
            TotalRevenue: Math.Round(overallRevenue, 2),
            Halls: hallDtos
        );
    }
}