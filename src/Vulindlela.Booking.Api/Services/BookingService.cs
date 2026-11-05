
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Clients;
using Vulindlela.Booking.Api.Models;
using Vulindlela.Booking.Api.Repositories;

namespace Vulindlela.Booking.Api.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;
    private readonly IClearanceNotificationClient _clearanceClient;

    public BookingService(
        IBookingRepository repository,
        IClearanceNotificationClient clearanceClient)
    {
        _repository = repository;
        _clearanceClient = clearanceClient;
    }

    public async Task<List<Branch>> GetBranchesAsync()
    {
        return await _repository.GetBranchesAsync();
    }

    public async Task<List<AppointmentSlot>> GetAvailableSlotsAsync(
        int branchId)
    {
        var slots = await _repository
            .GetSlotsByBranchAsync(branchId);

        var available = new List<AppointmentSlot>();

        foreach (var slot in slots)
        {
            if (!slot.IsBookable ||
                slot.StartTime <= DateTime.Now)
            {
                continue;
            }

            if (!await _repository.IsSlotTakenAsync(slot.SlotId))
            {
                available.Add(slot);
            }
        }

        return available;
    }

    public async Task<Appointment?> GetAppointmentAsync(
        int appointmentId)
    {
        return await _repository
            .GetAppointmentByIdAsync(appointmentId);
    }

    public async Task<BookingResult> BookAppointmentAsync(
        BookingRequest request)
    {
        // Rule 1: Required citizen information.
        if (request is null ||
            request.SlotId <= 0 ||
            string.IsNullOrWhiteSpace(request.CitizenId) ||
            string.IsNullOrWhiteSpace(request.CitizenIdNumber) ||
            string.IsNullOrWhiteSpace(request.CitizenName) ||
            string.IsNullOrWhiteSpace(request.ServiceType))
        {
            return new BookingResult
            {
                Success = false,
                Error = BookingError.InvalidRequest,
                Message = "Valid citizen and booking details are required."
            };
        }

        // Rule 2: The requested slot must exist.
        var slot = await _repository
            .GetSlotByIdAsync(request.SlotId);

        if (slot is null)
        {
            return new BookingResult
            {
                Success = false,
                Error = BookingError.SlotNotFound,
                Message = "The requested appointment slot does not exist."
            };
        }

        // Rule 3: The slot must be bookable and in the future.
        if (!slot.IsBookable ||
            slot.StartTime <= DateTime.Now)
        {
            return new BookingResult
            {
                Success = false,
                Error = BookingError.SlotUnavailable,
                Message = "This appointment slot is not available."
            };
        }

        // Rule 4: Check whether the slot is already occupied.
        if (await _repository.IsSlotTakenAsync(request.SlotId))
        {
            return new BookingResult
            {
                Success = false,
                Error = BookingError.SlotAlreadyBooked,
                Message = "This appointment slot has already been booked."
            };
        }

        var appointment = new Appointment
        {
            SlotId = request.SlotId,
            CitizenId = request.CitizenId.Trim(),
            CitizenIdNumber = request.CitizenIdNumber.Trim(),
            CitizenName = request.CitizenName.Trim(),
            ServiceType = request.ServiceType.Trim(),
            BookingDate = DateTime.UtcNow
        };

        Appointment savedAppointment;

        try
        {
            // The database unique constraint is the final
            // protection against concurrent double bookings.
            savedAppointment = await _repository
                .AddAppointmentAsync(appointment);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException sqliteEx &&
                  sqliteEx.SqliteErrorCode == 19 &&
                  sqliteEx.SqliteExtendedErrorCode == 2067)
        {
            // SQLite error 2067 means a UNIQUE constraint failed.
            // Verify that the conflict was on this appointment slot.
            if (await _repository.IsSlotTakenAsync(request.SlotId))
            {
                return new BookingResult
                {
                    Success = false,
                    Error = BookingError.SlotAlreadyBooked,
                    Message = "This slot was booked by another citizen."
                };
            }

            // Another unique constraint failed unexpectedly.
            throw;
        }

        // The appointment has been committed to Booking's database.
        // Clearance is a separate service, so notification failures
        // must never undo the booking.
        try
        {
            await _clearanceClient.NotifyAppointmentCreatedAsync(
                savedAppointment.AppointmentId,
                savedAppointment.CitizenId,
                savedAppointment.ServiceType);
        }
        catch (Exception ex)
        {
            // The HTTP client already handles expected network failures.
            // This also protects the completed booking against an
            // unexpected notification error.
            System.Diagnostics.Debug.WriteLine(
                $"Clearance notification failed: {ex.Message}");
        }

        return new BookingResult
        {
            Success = true,
            Error = BookingError.None,
            Message = "Appointment booked successfully.",
            Appointment = savedAppointment
        };
    }

    
    public async Task<List<Appointment>> GetAppointmentsByCitizenIdAsync(
        string citizenId)
    {
        if (string.IsNullOrWhiteSpace(citizenId))
        {
            return new List<Appointment>();
        }

        return await _repository.GetAppointmentsByCitizenIdAsync(
            citizenId.Trim());
    }

}
