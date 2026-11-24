using KaizenTracker.Api.Data;
using KaizenTracker.Api.Models;
using KaizenTracker.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace KaizenTracker.Api.Tests;

// Service tests against a real (SQLite, in-memory) database instead of mocks.
// xUnit creates a new instance of this class for every test, so the constructor is
// per-test setup and Dispose() is per-test teardown: each test gets a fresh, empty database.
public sealed class ProposalServiceTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);

    // An in-memory SQLite database lives only as long as its connection, so keep it open.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly FakeTimeProvider _clock = new(Now);

    public ProposalServiceTests()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

        // EnsureCreated builds the schema from the model. The migrations are SQL Server-specific,
        // so they aren't used here.
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    [Fact]
    public async Task Completing_an_approved_proposal_records_the_completion_time()
    {
        var id = await AddProposalAsync(ProposalStatus.Approved, submittedAt: Now.AddDays(-10));

        await using var db = new AppDbContext(_options);
        var service = new ProposalService(db, _clock);
        var result = await service.ChangeStatusAsync(id, ProposalStatus.Completed);

        Assert.Equal(StatusChangeOutcome.Changed, result.Outcome);
        var saved = await ReadProposalAsync(id); // fresh context, so this reads the database
        Assert.Equal(ProposalStatus.Completed, saved.Status);
        Assert.Equal(Now, saved.CompletedAt);
    }

    [Fact]
    public async Task Completing_an_unapproved_proposal_is_rejected_and_nothing_is_saved()
    {
        var id = await AddProposalAsync(ProposalStatus.Submitted, submittedAt: Now.AddDays(-3));

        await using var db = new AppDbContext(_options);
        var service = new ProposalService(db, _clock);
        var result = await service.ChangeStatusAsync(id, ProposalStatus.Completed);

        Assert.Equal(StatusChangeOutcome.InvalidTransition, result.Outcome);
        Assert.Contains("Allowed: UnderReview, Rejected", result.Error);
        var saved = await ReadProposalAsync(id);
        Assert.Equal(ProposalStatus.Submitted, saved.Status);
        Assert.Null(saved.CompletedAt);
    }

    [Fact]
    public async Task Changing_the_status_of_a_missing_proposal_returns_not_found()
    {
        await using var db = new AppDbContext(_options);
        var service = new ProposalService(db, _clock);

        var result = await service.ChangeStatusAsync(999, ProposalStatus.UnderReview);

        Assert.Equal(StatusChangeOutcome.NotFound, result.Outcome);
    }

    private async Task<int> AddProposalAsync(ProposalStatus status, DateTimeOffset submittedAt)
    {
        await using var db = new AppDbContext(_options);
        var proposal = new Proposal
        {
            Title = "Test proposal",
            Description = "For tests",
            Area = "Assembly",
            Status = status,
            SubmittedAt = submittedAt,
        };
        db.Proposals.Add(proposal);
        await db.SaveChangesAsync();
        return proposal.Id;
    }

    private async Task<Proposal> ReadProposalAsync(int id)
    {
        await using var db = new AppDbContext(_options);
        return await db.Proposals.AsNoTracking().SingleAsync(p => p.Id == id);
    }
}
