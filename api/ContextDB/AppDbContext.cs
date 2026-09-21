using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{

    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<HallAmenity> HallAmenities => Set<HallAmenity>();
    public DbSet<BookingAmenity> BookingAmenities => Set<BookingAmenity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HallAmenity>()
            .HasKey(ha => new { ha.HallId, ha.AmenityId });

        modelBuilder.Entity<HallAmenity>()
            .HasOne(ha => ha.Hall)
            .WithMany(h => h.HallAmenities)
            .HasForeignKey(ha => ha.HallId);

        modelBuilder.Entity<HallAmenity>()
            .HasOne(ha => ha.Amenity)
            .WithMany(a => a.HallAmenities)
            .HasForeignKey(ha => ha.AmenityId);
    }
}