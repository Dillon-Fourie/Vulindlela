
using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Models;

namespace Vulindlela.Booking.Api.Data;

public class BookingContext : DbContext
{
    public BookingContext(
        DbContextOptions<BookingContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<AppointmentSlot> AppointmentSlots
        => Set<AppointmentSlot>();

    public DbSet<Appointment> Appointments
        => Set<Appointment>();

    public DbSet<BookingAccessLog> BookingAccessLogs
        => Set<BookingAccessLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Branch>()
            .HasKey(b => b.BranchId);

        modelBuilder.Entity<AppointmentSlot>()
            .HasKey(s => s.SlotId);

        modelBuilder.Entity<Appointment>()
            .HasKey(a => a.AppointmentId);

        modelBuilder.Entity<BookingAccessLog>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<Branch>()
            .HasIndex(b => b.BranchCode)
            .IsUnique();

        modelBuilder.Entity<AppointmentSlot>()
            .HasIndex(s => new { s.BranchId, s.StartTime })
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => a.SlotId)
            .IsUnique();

        modelBuilder.Entity<AppointmentSlot>()
            .HasOne(s => s.Branch)
            .WithMany(b => b.Slots)
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Slot)
            .WithOne(s => s.Appointment)
            .HasForeignKey<Appointment>(a => a.SlotId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BookingAccessLog>()
            .Property(a => a.CitizenId)
            .IsRequired();

        modelBuilder.Entity<BookingAccessLog>()
            .Property(a => a.AccessedBy)
            .IsRequired();

        modelBuilder.Entity<BookingAccessLog>()
            .Property(a => a.AccessedByRole)
            .IsRequired();

        modelBuilder.Entity<BookingAccessLog>()
            .Property(a => a.Reason)
            .IsRequired();

        modelBuilder.Entity<BookingAccessLog>()
            .Property(a => a.ResourceType)
            .IsRequired();

        modelBuilder.Entity<BookingAccessLog>()
            .Property(a => a.ResourceId)
            .IsRequired();

        modelBuilder.Entity<BookingAccessLog>()
            .HasIndex(a => a.CitizenId);

        modelBuilder.Entity<BookingAccessLog>()
            .HasIndex(a => a.AccessedAt);
    }
}
