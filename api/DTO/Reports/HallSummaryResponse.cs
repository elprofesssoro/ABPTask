namespace api.DTO.Reports;

public record DateRange(
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate
);

public record HallPerformanceDTO(
    int HallId,
    string HallName,
    int BookingsCount,
    decimal TotalRevenue,
    decimal AverageBookingValue
);

public record HallSummaryResponse(
    DateTimeOffset FromDate,
    DateTimeOffset ToDate,
    int TotalBookings,
    decimal TotalRevenue,
    List<HallPerformanceDTO> Halls
);