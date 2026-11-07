
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Portal.Services;

namespace Vulindlela.Portal.Controllers;

[Authorize]
public class AccessLogController : Controller
{
    private readonly AccessLogService _accessLogService;
    private readonly IAuthorizationService _authorizationService;

    public AccessLogController(
        AccessLogService accessLogService,
        IAuthorizationService authorizationService)
    {
        _accessLogService = accessLogService;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? citizenId)
    {
        if (User.IsInRole("Citizen"))
        {
            citizenId = User.FindFirstValue("CitizenId");
        }
        else if (!User.IsInRole("Official"))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(citizenId))
        {
            return View(
                new List<Vulindlela.Portal.Models.Auth.AccessLog>());
        }

        var authorization = await _authorizationService.AuthorizeAsync(
            User,
            citizenId,
            "CitizenRecordAccess");

        if (!authorization.Succeeded)
            return Forbid();

        var logs = await _accessLogService
            .GetCitizenAccessLogsAsync(citizenId);

        ViewBag.CitizenId = citizenId;

        return View(logs);
    }
}
