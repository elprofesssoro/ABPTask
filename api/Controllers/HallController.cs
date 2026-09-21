using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;
using api.DTO;
using api.DTO.Responses;
using api.Services;
using ErrorOr;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HallController : ControllerBase
{
    private readonly IHallService _hallService;

    public HallController(IHallService hallService)
    {
        _hallService = hallService;
    }

    [HttpPost("add")]
    public async Task<ActionResult<Hall>> AddHall([FromBody] AddHallDTO hallDto)
    {

        ErrorOr<Hall> result = await _hallService.AddHallAsync(hallDto);

        if (result.IsError)
        {
            var firstError = result.Errors.First();
            return HandleError(firstError);
        }
        return Ok(result.Value.Id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateHall(
        [FromRoute] int id,
        [FromBody] UpdateHallDto hallDto)
    {

        ErrorOr<Success> result = await _hallService.UpdateHallAsync(id, hallDto);

        if (result.IsError)
        {
            var firstError = result.Errors.First();
            return HandleError(firstError);
        }
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteHall([FromRoute] int id)
    {
        ErrorOr<Success> result = await _hallService.DeleteHallAsync(id);
        if (result.IsError)
        {
            var firstError = result.Errors.First();
            return HandleError(firstError);
        }
        return Ok();
    }

    [HttpPost("search")]
    public async Task<ActionResult<SearchHallResponse>> SearchAvailableHalls([FromBody] SearchHallDTO searchDto)
    {
        ErrorOr<SearchHallResponse> result = await _hallService.SearchAvailableHallsAsync(searchDto);

        if (result.IsError)
        {
            var firstError = result.Errors.First();
            return HandleError(firstError);
        }
        return Ok(result.Value);
    }

    [HttpPost("books/{hallId:int}")]
    public async Task<IActionResult> BookHall([FromRoute] int hallId, [FromBody] BookingDTO bookingDto)
    {
        ErrorOr<BookingResponse> result = await _hallService.BookHallAsync(hallId, bookingDto);
        if (result.IsError)
        {
            var firstError = result.Errors.First();
            return HandleError(firstError);
        }

        return Ok(result.Value);
    }

    private ActionResult HandleError(Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation => BadRequest(error.Description),
            ErrorType.Conflict => Conflict(error.Description),
            ErrorType.NotFound => NotFound(error.Description),
            _ => StatusCode(500, "An unexpected error occurred.")
        };
    }
}