namespace api.DTO;

public record AmenityDTO(string Name, decimal Cost);
public record AddHallDTO(string Name, int Capacity, decimal CostPerHour, List<int> AmenityIds);