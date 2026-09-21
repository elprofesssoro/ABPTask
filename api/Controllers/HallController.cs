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
public class HallsController : ControllerBase
{
    private readonly IHallService _hallService;

    public HallsController(IHallService hallService)
    {
        _hallService = hallService;
    }

    [HttpPost("")]
    public async Task<ActionResult<GetHallResponse>> AddHall([FromBody] AddHallDTO hallDto)
    {

        ErrorOr<Hall> result = await _hallService.AddHallAsync(hallDto);
        if (result.IsError)
            return HandleError(result.Errors.First());

        GetHallResponse response = new GetHallResponse(result.Value.Id, result.Value.Name);
        return CreatedAtAction("GetHall", new { id = result.Value.Id }, response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetHallResponse>> GetHall([FromRoute] int id)
    {
        ErrorOr<Hall> hall = await _hallService.GetHallByIdAsync(id);
        if (hall.IsError)
            return HandleError(hall.Errors.First());

        return Ok(new GetHallResponse(hall.Value.Id, hall.Value.Name));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateHall(
        [FromRoute] int id,
        [FromBody] UpdateHallDto hallDto)
    {

        ErrorOr<Success> result = await _hallService.UpdateHallAsync(id, hallDto);
        if (result.IsError)
            return HandleError(result.Errors.First());

        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteHall([FromRoute] int id)
    {
        ErrorOr<Success> result = await _hallService.DeleteHallAsync(id);
        if (result.IsError)
            return HandleError(result.Errors.First());

        return NoContent();
    }

    [HttpPost("search")]
    public async Task<ActionResult<SearchHallResponse>> SearchAvailableHalls([FromBody] SearchHallDTO searchDto)
    {
        ErrorOr<SearchHallResponse> result = await _hallService.SearchAvailableHallsAsync(searchDto);
        if (result.IsError)
            return HandleError(result.Errors.First());

        return Ok(result.Value);
    }

    [HttpPost("{hallId:int}/bookings")]
    public async Task<IActionResult> BookHall([FromRoute] int hallId, [FromBody] BookingDTO bookingDto)
    {
        ErrorOr<BookingResponse> result = await _hallService.BookHallAsync(hallId, bookingDto);
        if (result.IsError)
            return HandleError(result.Errors.First());

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