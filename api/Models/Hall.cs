namespace api.Models;

public class Hall
{
    public int Id { get; set; }
    public required string Name { get; set; } = string.Empty;
    public required int Capacity { get; set; }
    public required decimal CostPerHour { get; set; }
    public List<HallAmenity> HallAmenities { get; set; } = new List<HallAmenity>();
}

public class Amenity
{
    public int Id { get; set; }
    public required string Name { get; set; } = string.Empty;
    public required decimal Cost { get; set; }
    public List<HallAmenity> HallAmenities { get; set; } = new();
}

public class HallAmenity
{
    public int Id { get; set; }
    public int HallId { get; set; }
    public int AmenityId { get; set; }
    public Hall Hall { get; set; } = null!;
    public Amenity Amenity { get; set; } = null!;
}
