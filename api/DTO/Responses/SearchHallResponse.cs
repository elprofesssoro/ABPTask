using api.Models;
namespace api.DTO.Responses;

public record AmenityDto(int Id, string Name, decimal Cost);
public record HallDto(int Id, string Name, int Capacity, decimal CostPerHour,
                      List<AmenityDto> Amenities);

public record SearchHallResponse(
    List<HallDto> Halls
);
                     