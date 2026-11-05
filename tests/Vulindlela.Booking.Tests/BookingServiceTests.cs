
using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Data;
using Vulindlela.Booking.Api.Models;
using Vulindlela.Booking.Api.Repositories;
using Vulindlela.Booking.Api.Services;
using Xunit;

namespace Vulindlela.Booking.Tests;

public class BookingServiceTests
{
    // Helper: Create a branch and a future appointment slot.
    private static AppointmentSlot CreateFutureSlot(
        SqliteTestFixture fixture,
        bool isBookable = true)
    {
        using var context = fixture.CreateContext();

        var branch = new Branch
        {
            Name = "Test Branch",
            Address = "Test Address",
            BranchCode = Guid.NewGuid().ToString("N")
        };

        context.Branches.Add(branch);
        context.SaveChanges();

        var startTime = DateTime.Now.AddDays(1);

        var slot = new AppointmentSlot
        {
            BranchId = branch.BranchId,
            StartTime = startTime,
            EndTime = startTime.AddMinutes(30),
            IsBookable = isBookable
        };

        context.AppointmentSlots.Add(slot);
        context.SaveChanges();

        return slot;
    }

    // Helper: Create fictional citizen booking details.
    private static BookingRequest CreateRequest(int slotId)
    {
        return new BookingRequest
        {
            SlotId = slotId,
            CitizenId = Guid.NewGuid().ToString("N"),
            CitizenIdNumber = "9001015009087",
            CitizenName = "Test Citizen",
            ServiceType = "ID Application"
        };
    }

    // Helper: Create a BookingService using the supplied context.
    private static BookingService CreateService(
        BookingContext context)
    {
        var repository = new BookingRepository(context);

        return new BookingService(repository);
    }

    // TEST 1: A valid booking should succeed.
    [Fact]
    public async Task ValidBooking_ShouldSucceed()
    {
        using var fixture = new SqliteTestFixture();

        var slot = CreateFutureSlot(fixture);

        await using var context = fixture.CreateContext();

        var service = CreateService(context);

        var result = await service.BookAppointmentAsync(
            CreateRequest(slot.SlotId));

        Assert.True(result.Success);
        Assert.NotNull(result.Appointment);

        await using var verificationContext =
            fixture.CreateContext();

        var savedCount =
            await verificationContext.Appointments.CountAsync();

        Assert.Equal(1, savedCount);
    }

    // TEST 2: A nonexistent slot should be rejected.
    [Fact]
    public async Task NonexistentSlot_ShouldBeRejected()
    {
        using var fixture = new SqliteTestFixture();

        await using var context = fixture.CreateContext();

        var service = CreateService(context);

        var result = await service.BookAppointmentAsync(
            CreateRequest(99999));

        Assert.False(result.Success);

        Assert.Equal(
            BookingError.SlotNotFound,
            result.Error);
    }

    // TEST 3: An unbookable slot should be rejected.
    [Fact]
    public async Task UnbookableSlot_ShouldBeRejected()
    {
        using var fixture = new SqliteTestFixture();

        var slot = CreateFutureSlot(
            fixture,
            isBookable: false);

        await using var context = fixture.CreateContext();

        var service = CreateService(context);

        var result = await service.BookAppointmentAsync(
            CreateRequest(slot.SlotId));

        Assert.False(result.Success);

        Assert.Equal(
            BookingError.SlotUnavailable,
            result.Error);
    }

    // TEST 4: Booking the same slot twice should fail.
    [Fact]
    public async Task DuplicateBooking_ShouldBeRejected()
    {
        using var fixture = new SqliteTestFixture();

        var slot = CreateFutureSlot(fixture);

        await using var firstContext =
            fixture.CreateContext();

        await using var secondContext =
            fixture.CreateContext();

        var firstService = CreateService(firstContext);
        var secondService = CreateService(secondContext);

        var firstResult =
            await firstService.BookAppointmentAsync(
                CreateRequest(slot.SlotId));

        var secondResult =
            await secondService.BookAppointmentAsync(
                CreateRequest(slot.SlotId));

        Assert.True(firstResult.Success);
        Assert.False(secondResult.Success);

        Assert.Equal(
            BookingError.SlotAlreadyBooked,
            secondResult.Error);

        await using var verificationContext =
            fixture.CreateContext();

        var savedCount =
            await verificationContext.Appointments.CountAsync(
                a => a.SlotId == slot.SlotId);

        Assert.Equal(1, savedCount);
    }

    // TEST 5: Ten concurrent booking attempts must result
    // in exactly one successful booking.
    [Fact]
    public async Task ConcurrentBookings_OnlyOneAppointmentPersists()
    {
        using var fixture = new SqliteTestFixture();

        var slot = CreateFutureSlot(fixture);

        const int numberOfCitizens = 10;

        // All booking requests wait for the same signal.
        var startSignal = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var tasks = Enumerable.Range(0, numberOfCitizens)
            .Select(async _ =>
            {
                await startSignal.Task;

                // Each request uses its own DbContext.
                await using var context =
                    fixture.CreateContext();

                var service = CreateService(context);

                return await service.BookAppointmentAsync(
                    CreateRequest(slot.SlotId));
            })
            .ToArray();

        // Release all ten booking attempts.
        startSignal.SetResult(true);

        var results = await Task.WhenAll(tasks);

        // Exactly one request should succeed.
        Assert.Single(results, r => r.Success);

        // All remaining requests should be rejected.
        Assert.Equal(
            numberOfCitizens - 1,
            results.Count(r => !r.Success));

        // Verify the actual database state independently.
        await using var verificationContext =
            fixture.CreateContext();

        var savedCount =
            await verificationContext.Appointments.CountAsync(
                a => a.SlotId == slot.SlotId);

        Assert.Equal(1, savedCount);
    }

    // TEST 6: SQLite must reject duplicate appointments,
    // even when BookingService is bypassed.
    [Fact]
    public async Task Database_ShouldRejectDuplicateSlotBookings()
    {
        using var fixture = new SqliteTestFixture();

        var slot = CreateFutureSlot(fixture);

        // Insert the first appointment directly.
        await using (var firstContext = fixture.CreateContext())
        {
            firstContext.Appointments.Add(new Appointment
            {
                SlotId = slot.SlotId,
                CitizenId = "citizen-001",
                CitizenIdNumber = "9001015009087",
                CitizenName = "First Citizen",
                ServiceType = "ID Application"
            });

            await firstContext.SaveChangesAsync();
        }

        // Attempt to insert a second appointment
        // with the same SlotId.
        await using (var secondContext = fixture.CreateContext())
        {
            secondContext.Appointments.Add(new Appointment
            {
                SlotId = slot.SlotId,
                CitizenId = "citizen-002",
                CitizenIdNumber = "9102025009088",
                CitizenName = "Second Citizen",
                ServiceType = "Passport Application"
            });

            // SQLite must reject the duplicate.
            await Assert.ThrowsAsync<DbUpdateException>(
                async () =>
                    await secondContext.SaveChangesAsync());
        }

        // Verify that only one appointment was persisted.
        await using var verificationContext =
            fixture.CreateContext();

        var savedCount =
            await verificationContext.Appointments.CountAsync(
                a => a.SlotId == slot.SlotId);

        Assert.Equal(1, savedCount);
    }
}
