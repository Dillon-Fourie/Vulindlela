
using Microsoft.EntityFrameworkCore;
using Vulindlela.Portal.Models.Auth;

namespace Vulindlela.Portal.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User accounts
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(user => user.Id);

            entity.HasIndex(user => user.Username)
                .IsUnique();

            entity.Property(user => user.Username)
                .IsRequired();

            entity.Property(user => user.PasswordHash)
                .IsRequired();

            entity.Property(user => user.Role)
                .IsRequired();

            entity.Property(user => user.CitizenId)
                .IsRequired();
        });

        // Citizen record access audit log
        modelBuilder.Entity<AccessLog>(entity =>
        {
            entity.HasKey(log => log.Id);

            entity.Property(log => log.CitizenId)
                .IsRequired();

            entity.Property(log => log.AccessedBy)
                .IsRequired();

            entity.Property(log => log.AccessedByRole)
                .IsRequired();

            entity.Property(log => log.AccessedAt)
                .IsRequired();

            entity.Property(log => log.Reason)
                .IsRequired();

            entity.Property(log => log.ResourceType)
                .IsRequired();

            entity.Property(log => log.ResourceId)
                .IsRequired();

            entity.HasIndex(log => log.CitizenId);

            entity.HasIndex(log => log.AccessedAt);
        });
    }
}
