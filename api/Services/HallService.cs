using ErrorOr;
using api.Models;
using api.Data;
using api.DTO;
using api.DTO.Responses;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class HallService(AppDbContext _context, IDiscountService _discountService) : IHallService
{
    public async Task<ErrorOr<Hall>> AddHallAsync(AddHallDTO hallDto)
    {
        List<int> existingAmenityIds = await _context.Amenities
        .Where(a => hallDto.AmenityIds.Contains(a.Id))
        .Select(a => a.Id)
        .ToListAsync();

        List<int> missingIds = hallDto.AmenityIds.Except(existingAmenityIds).ToList();
        if (missingIds.Any())
            return Error.Validation("Amenities.Invalid", $"Amenities not found: {string.Join(", ", missingIds)}");

        bool hallExists = await _context.Halls.AnyAsync(h => h.Name == hallDto.Name);
        if (hallExists)
            return Error.Conflict(code: "Hall.AlreadyExists", description: $"A hall with the name '{hallDto.Name}' already exists.");

        Hall hall = new Hall
        {
            Name = hallDto.Name,
            Capacity = hallDto.Capacity,
            CostPerHour = hallDto.CostPerHour,
            HallAmenities = existingAmenityIds.Select(aid => new HallAmenity
            {
                AmenityId = aid
            }).ToList()
        };

        _context.Halls.Add(hall);
        await _context.SaveChangesAsync();

        return hall;
    }

    public async Task<ErrorOr<Success>> UpdateHallAsync(int hallId, UpdateHallDto hallDto)
    {
        Hall? hall = await _context.Halls
            .Include(h => h.HallAmenities)
            .FirstOrDefaultAsync(h => h.Id == hallId);

        if (hall is null)
            return Error.NotFound("Hall.NotFound", $"A hall with ID '{hallId}' was not found.");

        if (hallDto.Name is not null) hall.Name = hallDto.Name;
        if (hallDto.Capacity.HasValue) hall.Capacity = hallDto.Capacity.Value;
        if (hallDto.Cost.HasValue) hall.CostPerHour = hallDto.Cost.Value;

        if (hallDto.AmenityIds is not null)
        {
            var amenityResult = await SyncHallAmenitiesAsync(hall, hallDto.AmenityIds);
            if (amenityResult.IsError)
            {
                return amenityResult.Errors;
            }
        }

        await _context.SaveChangesAsync();
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> DeleteHallAsync(int hallId)
    {
        bool hallExists = await _context.Halls.AnyAsync(h => h.Id == hallId);
        if (!hallExists)
            return Error.NotFound("Hall.NotFound", $"A hall with ID '{hallId}' was not found.");

        bool hasActiveBookings = await _context.Bookings
            .AnyAsync(b => b.HallId == hallId);

        if (hasActiveBookings)
            return Error.Conflict("Hall.HasActiveBookings", "Cannot delete a hall with upcoming bookings.");

        int rowsAffected = await _context.Halls
         .Where(h => h.Id == hallId)
         .ExecuteDeleteAsync();

        if (rowsAffected == 0)
            return Error.NotFound("Hall.NotFound", $"A hall with ID '{hallId}' was not found.");

        return Result.Success;
    }

    public async Task<ErrorOr<SearchHallResponse>> SearchAvailableHallsAsync(SearchHallDTO searchDto)
    {
        DateTime startTime = searchDto.StartTime.UtcDateTime;
        DateTime endTime = searchDto.EndTime.UtcDateTime;

        if (startTime >= endTime)
        {
            return Error.Validation(
                "Search.InvalidTimeRange",
                "The start time must precede the end time.");
        }

        if (startTime < DateTime.UtcNow)
        {
            return Error.Validation(
                "Search.PastTime",
                "Cannot search for hall availability in the past.");
        }

        if (searchDto.Capacity <= 0)
        {
            return Error.Validation(
                "Search.InvalidCapacity",
                "Capacity must be greater than zero.");
        }

        List<HallDto> availableHalls = await _context.Halls
            .AsNoTracking()
            .Where(h => h.Capacity >= searchDto.Capacity)
            .Where(h => !_context.Bookings.Any(b =>
                b.HallId == h.Id &&
                b.StartTime < endTime &&
                b.EndTime > startTime))
            .Select(h => new HallDto(
                h.Id,
                h.Name,
                h.Capacity,
                h.CostPerHour,
                h.HallAmenities.Select(ha => new AmenityDto(ha.AmenityId, ha.Amenity.Name, ha.Amenity.Cost)).ToList()
            ))
            .ToListAsync();

        return new SearchHallResponse(availableHalls);
    }

    public async Task<ErrorOr<BookingResponse>> BookHallAsync(int hallId, BookingDTO bookingDTO)
    {
        DateTime startTime = bookingDTO.date.UtcDateTime;
        DateTime endTime = startTime.AddMinutes(bookingDTO.durationMinutes);

        //Validate booking details
        if (bookingDTO.durationMinutes <= 0)
            return Error.Validation("Booking.InvalidDuration", "Duration must be greater than zero minutes.");

        if (startTime < DateTime.UtcNow)
            return Error.Validation("Booking.PastDate", "Cannot book a hall for a past date or time.");

        // Calculate the start and end times for the booking
        Hall? hall = await _context.Halls.FirstOrDefaultAsync(h => h.Id == hallId);

        if (hall is null)
            return Error.NotFound("Hall.NotFound", $"A hall with ID '{hallId}' was not found.");

        // Check whether the hall is available for the requested time slot
        bool hasConflict = await _context.Bookings.AnyAsync(b =>
            b.HallId == hallId &&
            b.StartTime < endTime &&
            b.EndTime > startTime);

        if (hasConflict)
            return Error.Conflict("Booking.Conflict", "The hall is already booked for the selected time slot.");

        // Validate and calculate amenities
        decimal totalAmenitiesCost = 0m;
        List<BookingAmenity> bookingAmenities = new List<BookingAmenity>();

        if (bookingDTO.AmenityIds is { Count: > 0 })
        {
            List<int> distinctTargetIds = bookingDTO.AmenityIds.Distinct().ToList();

            var availableAmenities = await _context.HallAmenities
                .Where(ha => ha.HallId == hallId && distinctTargetIds.Contains(ha.AmenityId))
                .Select(ha => new
                {
                    ha.AmenityId,
                    ha.Amenity.Cost
                })
                .ToListAsync();

            if (availableAmenities.Count != distinctTargetIds.Count)
            {
                var foundIds = availableAmenities.Select(a => a.AmenityId).ToHashSet();
                var missingIds = distinctTargetIds.Where(id => !foundIds.Contains(id));

                return Error.Validation(
                    "Booking.InvalidAmenities",
                    $"The following amenities are not available for this hall: {string.Join(", ", missingIds)}"
                );
            }

            totalAmenitiesCost = availableAmenities.Sum(a => a.Cost);

            bookingAmenities = availableAmenities
                .Select(a => new BookingAmenity
                {
                    AmenityId = a.AmenityId,
                    PriceAtBooking = a.Cost
                })
                .ToList();
        }

        decimal durationHours = (decimal)bookingDTO.durationMinutes / 60m;
        decimal hallBaseCost = Math.Round(hall.CostPerHour * durationHours, 2);
        decimal hallFinalCost = _discountService.CalculateDiscountPrice(hall.CostPerHour, startTime, endTime);
        decimal totalCost = hallFinalCost + totalAmenitiesCost;

        Booking booking = new Booking
        {
            HallId = hallId,
            BookingDate = DateTime.UtcNow,
            StartTime = startTime,
            EndTime = endTime,
            HallBaseCost = hallBaseCost,
            HallDiscountedCost = hallFinalCost,
            AmenitiesCost = totalAmenitiesCost,
            TotalCost = totalCost,
            BookedAmenities = bookingAmenities
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return new BookingResponse(totalCost);
    }

    private async Task<ErrorOr<Success>> SyncHallAmenitiesAsync(Hall hall, List<int> targetAmenityIds)
    {
        var distinctTargetIds = targetAmenityIds.Distinct().ToList();

        var existingIds = await _context.Amenities
            .Where(a => distinctTargetIds.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync();

        // Validate that all target IDs exist
        if (existingIds.Count != distinctTargetIds.Count)
        {
            IEnumerable<int> missingIds = distinctTargetIds.Except(existingIds);
            return Error.Validation(
                "Amenities.Invalid",
                $"Amenities not found: {string.Join(", ", missingIds)}"
            );
        }

        // Remove any HallAmenity links that are not in the target list and add new ones 
        var currentIds = hall.HallAmenities.Select(ha => ha.AmenityId).ToHashSet();
        var targetSet = distinctTargetIds.ToHashSet();
        hall.HallAmenities.RemoveAll(ha => !targetSet.Contains(ha.AmenityId));

        var newLinks = targetSet
            .Where(id => !currentIds.Contains(id))
            .Select(id => new HallAmenity { HallId = hall.Id, AmenityId = id });

        hall.HallAmenities.AddRange(newLinks);

        return Result.Success;
    }
}