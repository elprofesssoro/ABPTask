using Microsoft.AspNetCore.Mvc;
using api.Services;
using api.DTO.Reports;
using ErrorOr;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(IReportService _reportService) : ControllerBase
{
    [HttpGet("halls-summary")]
    public async Task<IActionResult> GetHallsSummary([FromQuery] DateRange dateRange)
    {
        ErrorOr<HallSummaryResponse> result = await _reportService.GetHallsSummaryAsync(dateRange);
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