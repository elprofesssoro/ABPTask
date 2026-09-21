namespace api.DTO;

public record CreateAmenityDto(
    string Name,
    decimal Cost
);

public record UpdateHallDto(
    string? Name,
    int? Capacity,
    decimal? Cost,
    List<int>? AmenityIds,
    List<CreateAmenityDto>? NewAmenities
);