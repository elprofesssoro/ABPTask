namespace api.DTO;

public record BookingDTO(
    DateTimeOffset date,
    int durationMinutes,
    List<int> AmenityIds
);