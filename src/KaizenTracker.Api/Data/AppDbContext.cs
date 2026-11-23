using KaizenTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KaizenTracker.Api.Data;

// The DbContext is EF Core's unit of work: it tracks loaded entities and turns
// changes into SQL on SaveChangesAsync(). Registered as Scoped (one per request).
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Proposal> Proposals => Set<Proposal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Proposal>(e =>
        {
            e.Property(p => p.Title).HasMaxLength(200);
            e.Property(p => p.Area).HasMaxLength(100);

            // Store the enum as text ("Approved"), not an int, so reordering
            // the enum can never silently change existing rows.
            e.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

            // The list endpoint filters by status; the metrics query (step 4) groups by area.
            e.HasIndex(p => p.Status);
            e.HasIndex(p => p.Area);
        });
    }
}
