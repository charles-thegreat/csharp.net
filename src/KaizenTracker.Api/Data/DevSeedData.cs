using KaizenTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KaizenTracker.Api.Data;

// Sample proposals for local development, with submission/completion dates spread over
// the past few months so the lead-time metrics have something meaningful to show.
// Wired up in Program.cs (Development only) via UseSeeding/UseAsyncSeeding, which EF Core
// runs after migrations: on `dotnet ef database update` or Database.Migrate().
// Rows are inserted directly, so they bypass the ProposalWorkflow rules.
public static class DevSeedData
{
    public static void Seed(DbContext context)
    {
        var proposals = context.Set<Proposal>();
        if (proposals.Any()) return;

        proposals.AddRange(Build(DateTimeOffset.UtcNow));
        context.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, CancellationToken ct)
    {
        var proposals = context.Set<Proposal>();
        if (await proposals.AnyAsync(ct)) return;

        proposals.AddRange(Build(DateTimeOffset.UtcNow));
        await context.SaveChangesAsync(ct);
    }

    private static IEnumerable<Proposal> Build(DateTimeOffset now)
    {
        // (title, area, status, submitted N days ago, completed N days after submission)
        (string Title, string Area, ProposalStatus Status, int DaysAgo, int? LeadDays)[] rows =
        [
            ("Shadow board for torque wrenches", "Assembly", ProposalStatus.Completed, 90, 12),
            ("Relocate fastener bins to point of use", "Assembly", ProposalStatus.Completed, 75, 8),
            ("Andon cord at station 14", "Assembly", ProposalStatus.Completed, 40, 21),
            ("Standard work sheet for door install", "Assembly", ProposalStatus.UnderReview, 6, null),
            ("Poka-yoke jig for bracket orientation", "Body Weld", ProposalStatus.Completed, 120, 30),
            ("Tip-dresser schedule by weld count", "Body Weld", ProposalStatus.Completed, 60, 18),
            ("Spatter shield on robot cell 3", "Body Weld", ProposalStatus.Approved, 20, null),
            ("Booth filter change visual indicator", "Paint", ProposalStatus.Completed, 100, 5),
            ("Color-change sequence to cut purge waste", "Paint", ProposalStatus.Completed, 50, 9),
            ("Second inspection light at final sand", "Paint", ProposalStatus.Rejected, 30, null),
            ("Kanban loop for door seals", "Logistics", ProposalStatus.Completed, 80, 25),
            ("Milk-run route change for line 2", "Logistics", ProposalStatus.Submitted, 2, null),
        ];

        foreach (var r in rows)
        {
            var submitted = now.AddDays(-r.DaysAgo);
            yield return new Proposal
            {
                Title = r.Title,
                Description = $"Sample proposal for the {r.Area} area.",
                Area = r.Area,
                Status = r.Status,
                SubmittedAt = submitted,
                CompletedAt = r.LeadDays is int lead ? submitted.AddDays(lead) : null,
            };
        }
    }
}
