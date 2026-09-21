namespace api.DTO;

public record SearchHallDTO(
    DateTime StartTime,
    DateTime EndTime,
    int Capacity
);