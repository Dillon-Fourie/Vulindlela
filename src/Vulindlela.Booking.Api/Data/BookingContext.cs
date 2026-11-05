
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure primary keys
        modelBuilder.Entity<Branch>()
            .HasKey(b => b.BranchId);

        modelBuilder.Entity<AppointmentSlot>()
            .HasKey(s => s.SlotId);

        modelBuilder.Entity<Appointment>()
            .HasKey(a => a.AppointmentId);

        // A unique code must be assigned to each branch.
        modelBuilder.Entity<Branch>()
            .HasIndex(b => b.BranchCode)
            .IsUnique();

        // There cannot be two slots starting at the same time for a branch.
        modelBuilder.Entity<AppointmentSlot>()
            .HasIndex(s => new { s.BranchId, s.StartTime })
            .IsUnique();

        // CRITICAL: There can only be one appointment per slot.
        modelBuilder.Entity<Appointment>()
            .HasIndex(a => a.SlotId)
            .IsUnique();

        // There can be many appointment slots per branch
        modelBuilder.Entity<AppointmentSlot>()
            .HasOne(s => s.Branch)
            .WithMany(b => b.Slots)
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Each appointment needs to belong to a slot
        // Each slot can only have one appointment
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Slot)
            .WithOne(s => s.Appointment)
            .HasForeignKey<Appointment>(a => a.SlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
