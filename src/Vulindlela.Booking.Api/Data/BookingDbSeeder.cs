
using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Models;

namespace Vulindlela.Booking.Api.Data;

public class BookingDbSeeder
{
    private readonly BookingContext _dbContext;

    public BookingDbSeeder(BookingContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync()
    {
        await _dbContext.Database.EnsureCreatedAsync();
        await EnsureAccessLogTableAsync();

        // Create the three branches if they do not exist.
        if (!await _dbContext.Branches.AnyAsync())
        {
            var branches = new List<Branch>
            {
                new Branch
                {
                    Name = "Cape Town Civic Services",
                    Address = "Cape Town, Western Cape",
                    BranchCode = "CPT001"
                },
                new Branch
                {
                    Name = "Johannesburg Civic Services",
                    Address = "Johannesburg, Gauteng",
                    BranchCode = "JHB001"
                },
                new Branch
                {
                    Name = "Durban Civic Services",
                    Address = "Durban, KwaZulu-Natal",
                    BranchCode = "DBN001"
                }
            };

            await _dbContext.Branches.AddRangeAsync(branches);
            await _dbContext.SaveChangesAsync();
        }

        var branchesFromDb = await _dbContext.Branches.ToListAsync();

        // Start tomorrow to guarantee future appointment dates.
        var firstDate = DateTime.Today.AddDays(1);

        foreach (var branch in branchesFromDb)
        {
            int weekdaysCreated = 0;
            var date = firstDate;

            // Generate 14 future weekdays, excluding weekends.
            while (weekdaysCreated < 14)
            {
                if (date.DayOfWeek == DayOfWeek.Saturday ||
                    date.DayOfWeek == DayOfWeek.Sunday)
                {
                    date = date.AddDays(1);
                    continue;
                }

                // Create 30-minute slots from 09:00 to 16:00.
                for (int hour = 9; hour < 16; hour++)
                {
                    for (int minute = 0; minute < 60; minute += 30)
                    {
                        var startTime = date.AddHours(hour)
                            .AddMinutes(minute);

                        var endTime = startTime.AddMinutes(30);

                        bool exists = await _dbContext.AppointmentSlots
                            .AnyAsync(s =>
                                s.BranchId == branch.BranchId &&
                                s.StartTime == startTime);

                        if (exists)
                        {
                            continue;
                        }

                        _dbContext.AppointmentSlots.Add(
                            new AppointmentSlot
                            {
                                BranchId = branch.BranchId,
                                StartTime = startTime,
                                EndTime = endTime,
                                IsBookable = true
                            });
                    }
                }

                weekdaysCreated++;
                date = date.AddDays(1);
            }
        }

        await _dbContext.SaveChangesAsync();
    }


private async Task EnsureAccessLogTableAsync()
{
    await _dbContext.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS "BookingAccessLogs" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_BookingAccessLogs" PRIMARY KEY AUTOINCREMENT,
            "CitizenId" TEXT NOT NULL,
            "AccessedBy" TEXT NOT NULL,
            "AccessedByRole" TEXT NOT NULL,
            "AccessedAt" TEXT NOT NULL,
            "Reason" TEXT NOT NULL,
            "ResourceType" TEXT NOT NULL,
            "ResourceId" TEXT NOT NULL
        );
        """);

    await _dbContext.Database.ExecuteSqlRawAsync(
        """
        CREATE INDEX IF NOT EXISTS "IX_BookingAccessLogs_CitizenId"
        ON "BookingAccessLogs" ("CitizenId");
        """);

    await _dbContext.Database.ExecuteSqlRawAsync(
        """
        CREATE INDEX IF NOT EXISTS "IX_BookingAccessLogs_AccessedAt"
        ON "BookingAccessLogs" ("AccessedAt");
        """);
}


}
