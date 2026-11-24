using KaizenTracker.Api.Models;
using KaizenTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace KaizenTracker.Api.Controllers;

[ApiController]
[Route("api/metrics")]
public class MetricsController(IMetricsService metrics) : ControllerBase
{
    // GET api/metrics/lead-time
    [HttpGet("lead-time")]
    public async Task<ActionResult<LeadTimeReport>> LeadTime(CancellationToken ct) =>
        await metrics.GetLeadTimeAsync(ct);
}
