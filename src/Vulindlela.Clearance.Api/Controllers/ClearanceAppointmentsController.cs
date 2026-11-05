
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Clearance.Api.Data;
using Vulindlela.Clearance.Api.DTOs;
using Vulindlela.Clearance.Api.Models;

namespace Vulindlela.Clearance.Api.Controllers;

[ApiController]
[Route("api/clearance/appointments")]
public class ClearanceAppointmentsController : ControllerBase
{
    private readonly ClearanceContext _context;

    public ClearanceAppointmentsController(ClearanceContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterAppointment(
        [FromBody] RegisterAppointmentRequest request)
    {
        var existing = await _context.ClearanceAppointments
            .FindAsync(request.AppointmentId);

        if (existing != null)
        {
            // Idempotent registration: repeated notifications
            // do not create duplicate records.
            return Ok(new
            {
                existing.AppointmentId,
                Message = "Appointment already registered."
            });
        }

        var appointment = new ClearanceAppointment
        {
            AppointmentId = request.AppointmentId,
            CitizenId = request.CitizenId.Trim(),
            ServiceType = request.ServiceType.Trim(),
            RegisteredAt = DateTime.UtcNow
        };

        _context.ClearanceAppointments.Add(appointment);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Handle simultaneous notifications for the same ID.
            return Conflict(new
            {
                Message = "Appointment registration conflict."
            });
        }

        return CreatedAtAction(
            nameof(GetAppointment),
            new { appointmentId = appointment.AppointmentId },
            new
            {
                appointment.AppointmentId,
                appointment.CitizenId,
                appointment.ServiceType,
                appointment.RegisteredAt
            });
    }

    [HttpGet("{appointmentId:int}/registration")]
    public async Task<IActionResult> GetAppointment(int appointmentId)
    {
        var appointment = await _context.ClearanceAppointments
            .FindAsync(appointmentId);

        if (appointment == null)
        {
            return NotFound(new
            {
                Message = "Appointment is not registered with Clearance."
            });
        }

        return Ok(appointment);
    }
}
