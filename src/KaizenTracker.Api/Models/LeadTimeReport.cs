namespace KaizenTracker.Api.Models;

// Response shapes for GET api/metrics/lead-time. Lead time = submission to completion.
public record AreaLeadTime(string Area, int CompletedCount, double AverageLeadTimeDays);

public record LeadTimeReport(int CompletedCount, double? AverageLeadTimeDays, List<AreaLeadTime> ByArea);
