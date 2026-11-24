using KaizenTracker.Api.Data;
using KaizenTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KaizenTracker.Api.Services;

public interface IMetricsService
{
    Task<LeadTimeReport> GetLeadTimeAsync(CancellationToken ct = default);
}

public class MetricsService(AppDbContext db) : IMetricsService
{
    private const double SecondsPerDay = 86_400;

    public async Task<LeadTimeReport> GetLeadTimeAsync(CancellationToken ct = default)
    {
        // One SQL query: GROUP BY Area, returning a count and a total per area.
        // EF.Functions.DateDiffSecond translates to SQL Server's DATEDIFF(second, ...),
        // so the date math runs in the database. It is SQL Server-specific and can't run in memory.
        var perArea = await db.Proposals
            .AsNoTracking()
            .Where(p => p.Status == ProposalStatus.Completed && p.CompletedAt != null)
            .GroupBy(p => p.Area)
            .Select(g => new
            {
                Area = g.Key,
                Count = g.Count(),
                TotalSeconds = g.Sum(p => (long)EF.Functions.DateDiffSecond(p.SubmittedAt, p.CompletedAt!.Value)),
            })
            .OrderBy(r => r.Area)
            .ToListAsync(ct);

        // From here on it's ordinary LINQ over a small in-memory list.
        var byArea = perArea
            .Select(r => new AreaLeadTime(r.Area, r.Count, ToDays(r.TotalSeconds / (double)r.Count)))
            .ToList();

        // Overall average is weighted by count (total time / total proposals),
        // not an average of the per-area averages.
        var totalCount = perArea.Sum(r => r.Count);
        double? overall = totalCount == 0 ? null : ToDays(perArea.Sum(r => r.TotalSeconds) / (double)totalCount);

        return new LeadTimeReport(totalCount, overall, byArea);
    }

    private static double ToDays(double seconds) => Math.Round(seconds / SecondsPerDay, 1);
}
