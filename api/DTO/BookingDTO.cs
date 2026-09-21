namespace api.DTO;

public record BookingDTO(
    DateTime date,
    int durationMinutes,
    List<int> AmenityIds
);