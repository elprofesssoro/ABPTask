namespace api.Models;

public class Booking
{
    public int Id { get; set; }
    public required DateTime BookingDate { get; set; }
    public required DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Duration => (int)(EndTime - StartTime).TotalMinutes;
    public int HallId { get; set; }
    public Hall Hall { get; set; } = null!;
    public List<BookingAmenity> BookedAmenities { get; set; } = new List<BookingAmenity>();

    public decimal HallBaseCost { get; set; }
    public decimal HallDiscountedCost { get; set; }
    public decimal AmenitiesCost { get; set; }
    public decimal TotalCost { get; set; }
}

public class BookingAmenity
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int AmenityId { get; set; }
    public decimal PriceAtBooking { get; set; }
    public Booking Booking { get; set; } = null!;
    public Amenity Amenity { get; set; } = null!;
}