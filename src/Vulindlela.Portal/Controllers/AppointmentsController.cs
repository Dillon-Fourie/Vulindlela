
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Portal.Services;
using Vulindlela.Portal.Models;

namespace Vulindlela.Portal.Controllers;

public class AppointmentsController : Controller
{
    private readonly AppointmentPortalService _appointmentService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        AppointmentPortalService appointmentService,
        ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid appointment ID.");
        }

        try
        {
            var appointment = await _appointmentService
                .GetAppointmentDetailsAsync(id);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            return View(appointment);
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
            ViewBag.CitizenId = citizenId;

            if (string.IsNullOrWhiteSpace(citizenId))
            {
                return View(new List<
                    Vulindlela.Portal.Models.AppointmentDetailsViewModel>());
            }

            try
            {
                var appointments = await _appointmentService
                    .GetCitizenAppointmentsAsync(citizenId);

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

            return View(new List<
                Vulindlela.Portal.Models.AppointmentDetailsViewModel>());
        }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateBookingViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _appointmentService.CreateBookingAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message
            );

            return View(model);
        }

        if (!result.AppointmentId.HasValue)
        {
            ModelState.AddModelError(
                string.Empty,
                "Booking succeeded, but the confirmation ID was not returned. " +
                "Please check My Appointments before trying again."
            );

            return View(model);
        }

        return RedirectToAction(
            nameof(Details),
            new { id = result.AppointmentId.Value }
        );
    }


}
