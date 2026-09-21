using ErrorOr;
using api.DTO.Reports;

namespace api.Services;

public interface IReportService
{
    Task<ErrorOr<HallSummaryResponse>> GetHallsSummaryAsync(DateRange dateRangeFilter);
}