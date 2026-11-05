
using Microsoft.EntityFrameworkCore;
using Vulindlela.Clearance.Api.Models;

namespace Vulindlela.Clearance.Api.Data;

public class ClearanceContext : DbContext
{
    public ClearanceContext(
        DbContextOptions<ClearanceContext> options)
        : base(options)
    {
    }

    public DbSet<ClearanceSubmission> ClearanceSubmissions
        => Set<ClearanceSubmission>();

    public DbSet<SubmittedDocument> SubmittedDocuments
        => Set<SubmittedDocument>();

    public DbSet<MissingDocument> MissingDocuments
        => Set<MissingDocument>();

    public DbSet<ClearanceAppointment> ClearanceAppointments
    => Set<ClearanceAppointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Each submission has a unique primary key.
        modelBuilder.Entity<ClearanceSubmission>()
            .HasKey(s => s.ClearanceSubmissionId);

        modelBuilder.Entity<ClearanceSubmission>()
            .Property(s => s.Status)
            .HasConversion<string>();

        modelBuilder.Entity<ClearanceSubmission>()
            .Property(s => s.CitizenId)
            .IsRequired();

        modelBuilder.Entity<ClearanceSubmission>()
            .Property(s => s.ServiceType)
            .IsRequired();

        // One appointment has one clearance submission.
        modelBuilder.Entity<ClearanceSubmission>()
            .HasIndex(s => s.AppointmentId)
            .IsUnique();

        // Submission can contain multiple documents.
        modelBuilder.Entity<SubmittedDocument>()
            .HasKey(d => d.SubmittedDocumentId);

        modelBuilder.Entity<SubmittedDocument>()
            .HasOne(d => d.ClearanceSubmission)
            .WithMany(s => s.Documents)
            .HasForeignKey(d => d.ClearanceSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Submissions can have multiple missing documents.
        modelBuilder.Entity<MissingDocument>()
            .HasKey(d => d.MissingDocumentId);

        modelBuilder.Entity<MissingDocument>()
            .HasOne(d => d.ClearanceSubmission)
            .WithMany(s => s.MissingDocuments)
            .HasForeignKey(d => d.ClearanceSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ClearanceAppointment>()
            .HasKey(a => a.AppointmentId);
    }
}
