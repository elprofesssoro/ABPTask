namespace api.DTO;

public record SearchHallDTO(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int Capacity
);