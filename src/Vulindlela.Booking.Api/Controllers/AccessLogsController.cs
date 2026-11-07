
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Booking.Api.Services;

namespace Vulindlela.Booking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/accesslogs")]
public class AccessLogsController : ControllerBase
{
    private readonly BookingAccessLogService _accessLogService;

    public AccessLogsController(BookingAccessLogService accessLogService)
    {
        _accessLogService = accessLogService;
    }

    [HttpGet("citizen/{citizenId}")]
    public async Task<IActionResult> GetCitizenAccessLogs(string citizenId)
    {
        if (string.IsNullOrWhiteSpace(citizenId))
        {
            return BadRequest("A citizen ID is required.");
        }

        if (!User.IsInRole("Official"))
        {
            if (!User.IsInRole("Citizen"))
            {
                return Forbid();
            }

            var signedInCitizenId = User.FindFirstValue("CitizenId");

            if (string.IsNullOrWhiteSpace(signedInCitizenId) ||
                !string.Equals(
                    signedInCitizenId,
                    citizenId,
                    StringComparison.Ordinal))
            {
                return Forbid();
            }
        }

        var logs = await _accessLogService
            .GetLogsForCitizenAsync(citizenId);

        return Ok(logs);
    }
}
