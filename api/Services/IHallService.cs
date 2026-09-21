using api.Models;
using ErrorOr;
using api.DTO;
using api.DTO.Responses;

namespace api.Services;

public interface IHallService
{
    Task<ErrorOr<Hall>> AddHallAsync(AddHallDTO hallDto);
    Task<ErrorOr<Success>> UpdateHallAsync(int hallId, UpdateHallDto hallDto);
    Task<ErrorOr<SearchHallResponse>> SearchAvailableHallsAsync(SearchHallDTO searchDto);
    Task<ErrorOr<BookingResponse>> BookHallAsync(int hallId, BookingDTO bookingDTO);
    Task<ErrorOr<Success>> DeleteHallAsync(int hallId);
}