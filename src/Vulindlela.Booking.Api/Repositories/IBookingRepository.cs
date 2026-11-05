
using Vulindlela.Booking.Api.Models;

namespace Vulindlela.Booking.Api.Repositories;

public interface IBookingRepository
{
    // Retrieve all branches.
    Task<List<Branch>> GetBranchesAsync();

    // Retrieve slots for a specific branch.
    Task<List<AppointmentSlot>> GetSlotsByBranchAsync(
        int branchId);

    // Retrieve a specific appointment slot.
    Task<AppointmentSlot?> GetSlotByIdAsync(int slotId);

    // Check whether a slot already has an appointment.
    Task<bool> IsSlotTakenAsync(int slotId);

    // Save a new appointment.
    Task<Appointment> AddAppointmentAsync(
        Appointment appointment);

    // Retrieve an appointment by its ID.
    Task<Appointment?> GetAppointmentByIdAsync(
        int appointmentId);
}
