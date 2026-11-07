
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Portal.Models;
using Vulindlela.Portal.Services;

namespace Vulindlela.Portal.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly AppointmentPortalService _appointmentService;
    private readonly AccessLogService _accessLogService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        AppointmentPortalService appointmentService,
        AccessLogService accessLogService,
        IAuthorizationService authorizationService,
        ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _accessLogService = accessLogService;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid appointment ID.");

        try
        {
            var details = await _appointmentService
                .GetAppointmentDetailsAsync(id);

            if (details == null)
                return NotFound("Appointment not found.");

            var citizenId = details.Appointment.CitizenId;

            var authorization = await _authorizationService.AuthorizeAsync(
                User,
                citizenId,
                "CitizenRecordAccess");

            if (!authorization.Succeeded)
                return Forbid();

            await RecordAccessAsync(
                citizenId,
                "Viewed appointment details",
                "Appointment",
                id.ToString());

            return View(details);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex,
                "Unable to retrieve appointment {AppointmentId}", id);

            return StatusCode(503,
                "Booking information is temporarily unavailable. " +
                "Please try again later.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex,
                "Booking API timed out for appointment {AppointmentId}", id);

            return StatusCode(503,
                "Booking information is temporarily unavailable. " +
                "Please try again later.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> MyAppointments(string? citizenId)
    {
        if (string.IsNullOrWhiteSpace(citizenId))
            citizenId = User.FindFirstValue("CitizenId");

        ViewBag.CitizenId = citizenId;

        if (string.IsNullOrWhiteSpace(citizenId))
            return View(new List<AppointmentDetailsViewModel>());

        var authorization = await _authorizationService.AuthorizeAsync(
            User,
            citizenId,
            "CitizenRecordAccess");

        if (!authorization.Succeeded)
            return Forbid();

        try
        {
            var appointments = await _appointmentService
                .GetCitizenAppointmentsAsync(citizenId);

            await RecordAccessAsync(
                citizenId,
                "Viewed citizen appointment list",
                "AppointmentList",
                citizenId);

            return View(appointments);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex,
                "Unable to retrieve appointments for citizen {CitizenId}",
                citizenId);

            ViewBag.ErrorMessage =
                "Booking information is temporarily unavailable. " +
                "Please try again later.";
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex,
                "Booking API timed out for citizen {CitizenId}",
                citizenId);

            ViewBag.ErrorMessage =
                "Booking information is temporarily unavailable. " +
                "Please try again later.";
        }

        return View(new List<AppointmentDetailsViewModel>());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateBookingViewModel
        {
            CitizenId = User.FindFirstValue("CitizenId") ?? ""
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookingViewModel model)
    {
        if (User.IsInRole("Citizen"))
        {
            var signedInCitizenId = User.FindFirstValue("CitizenId");

            if (string.IsNullOrWhiteSpace(signedInCitizenId))
                return Forbid();

            model.CitizenId = signedInCitizenId;

            ModelState.Remove(nameof(model.CitizenId));
            TryValidateModel(model);
        }
        else if (!User.IsInRole("Official"))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
            return View(model);

        var result = await _appointmentService.CreateBookingAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        if (!result.AppointmentId.HasValue)
        {
            ModelState.AddModelError(
                string.Empty,
                "Booking succeeded, but the confirmation ID was not returned. " +
                "Please check My Appointments before trying again.");

            return View(model);
        }

        return RedirectToAction(
            nameof(Details),
            new { id = result.AppointmentId.Value });
    }

    private async Task RecordAccessAsync(
        string citizenId,
        string reason,
        string resourceType,
        string resourceId)
    {
        var username = User.Identity?.Name ?? "Unknown";

        var role = User.IsInRole("Official")
            ? "Official"
            : "Citizen";

        await _accessLogService.RecordAccessAsync(
            citizenId,
            username,
            role,
            reason,
            resourceType,
            resourceId);
    }
}
