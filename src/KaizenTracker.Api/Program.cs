using System.Text.Json.Serialization;
using KaizenTracker.Api.Data;
using KaizenTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register services with the DI container.

// In Development the connection string comes from user-secrets (dotnet user-secrets list);
// in a deployed environment it would come from an env var: ConnectionStrings__Default.
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString)); // Scoped lifetime

builder.Services.AddSingleton(TimeProvider.System);                // stateless, safe to share
builder.Services.AddScoped<IProposalService, ProposalService>();   // Scoped because it uses AppDbContext

builder.Services
    .AddControllers()
    // Send and accept enums as "Approved" rather than 2.
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi(); // OpenAPI document at /openapi/v1.json

var app = builder.Build();

// Configure the HTTP request pipeline (middleware runs in this order).
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// UseHttpsRedirection removed: plain HTTP locally under WSL.
app.UseAuthorization();
app.MapControllers();

app.Run();
