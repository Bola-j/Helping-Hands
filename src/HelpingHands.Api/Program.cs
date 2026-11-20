using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using HelpingHands.Api.Auth;
using HelpingHands.Api.Configuration;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features;
using HelpingHands.Api.Features.Catalog;
using HelpingHands.Api.Features.Needs;
using HelpingHands.Api.Features.Reminders;
using HelpingHands.Api.Storage;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var authLimit = builder.Configuration.GetValue("RateLimiting:AuthPermitLimit", 20);
builder.Services.AddProblemDetails();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddOptions<SmtpOptions>().BindConfiguration(SmtpOptions.SectionName);
builder.Services.AddDbContext<HelpingHandsDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("HelpingHands")));
builder.Services.AddHelpingHandsIdentity();
builder.Services.AddScoped<NeedVersionService>();
builder.Services.AddScoped<DailyReminderStateService>();
builder.Services.AddScoped<AntiforgeryFilter>();
builder.Services.AddEvidenceStorage(builder.Configuration);
builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("auth", limiter =>
{
    limiter.PermitLimit = authLimit;
    limiter.Window = TimeSpan.FromMinutes(1);
    limiter.QueueLimit = 0;
}));
var app = builder.Build();
if (args.Contains("--seed-demo"))
{
    await AuthSetup.SeedDemoUsersAsync(app.Services);
    return;
}
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var status = error is DbUpdateConcurrencyException ? 409 : 500;
    await Results.Problem(statusCode: status, title: status == 409 ? "The record was changed" : "An error occurred").ExecuteAsync(context);
}));
app.UseAuthentication();
app.UseMiddleware<ExpectedUserMiddleware>();
app.UseAuthorization();
app.UseRateLimiter();
app.UseAntiforgery();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/ready", async (HelpingHandsDbContext db, CancellationToken ct) => await db.Database.CanConnectAsync(ct) ? Results.Ok(new { status = "ready" }) : Results.Problem(statusCode: 503, title: "Database unavailable"));
if (app.Environment.IsDevelopment()) app.MapOpenApi();
var api = app.MapGroup("/api/v1").AddEndpointFilter<AntiforgeryFilter>();
api.MapAuthEndpoints();
api.MapDemoEndpoints();
api.MapCatalogEndpoints();
app.Run();

public partial class Program;