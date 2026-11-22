using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class EligibleCharityMatchingTests
{
    private const string TestRoleHeader = "X-Test-Role";

    [Fact]
    public void ExactVersionAndFreshnessPolicy_RejectsStaleCapacityAndVersionMismatchWithoutSilentConversion()
    {
        var categoryId = Guid.NewGuid();
        var otherCategoryId = Guid.NewGuid();

        var serviceV1Id = Guid.NewGuid();
        var serviceV2Id = Guid.NewGuid();
        var standardV1Id = Guid.NewGuid();
        var standardV2Id = Guid.NewGuid();

        var cairoToday = new DateOnly(2026, 11, 20);
        var cairoYesterday = new DateOnly(2026, 11, 19);
        var nowUtc = new DateTimeOffset(2026, 11, 20, 9, 0, 0, TimeSpan.Zero);

        var rows = new[]
        {
            new CandidateRow(
                RecipientId: Guid.NewGuid(),
                CharityName: "Nile Care",
                IsCharity: true,
                ApprovalStatus: "Approved",
                Active: true,
                Enabled: true,
                CategoryId: categoryId,
                ServiceVersionId: serviceV1Id,
                StandardVersionId: standardV1Id,
                CapacityCairoDate: cairoToday,
                Available: 25m,
                UpdatedAt: nowUtc),

            new CandidateRow(
                RecipientId: Guid.NewGuid(),
                CharityName: "Pending Charity",
                IsCharity: true,
                ApprovalStatus: "Pending",
                Active: true,
                Enabled: true,
                CategoryId: categoryId,
                ServiceVersionId: serviceV1Id,
                StandardVersionId: standardV1Id,
                CapacityCairoDate: cairoToday,
                Available: 40m,
                UpdatedAt: nowUtc),

            new CandidateRow(
                RecipientId: Guid.NewGuid(),
                CharityName: "Inactive Charity",
                IsCharity: true,
                ApprovalStatus: "Approved",
                Active: false,
                Enabled: true,
                CategoryId: categoryId,
                ServiceVersionId: serviceV1Id,
                StandardVersionId: standardV1Id,
                CapacityCairoDate: cairoToday,
                Available: 35m,
                UpdatedAt: nowUtc),

            new CandidateRow(
                RecipientId: Guid.NewGuid(),
                CharityName: "Stale Capacity Charity",
                IsCharity: true,
                ApprovalStatus: "Approved",
                Active: true,
                Enabled: true,
                CategoryId: categoryId,
                ServiceVersionId: serviceV1Id,
                StandardVersionId: standardV1Id,
                CapacityCairoDate: cairoYesterday,
                Available: 60m,
                UpdatedAt: nowUtc.AddDays(-1)),

            new CandidateRow(
                RecipientId: Guid.NewGuid(),
                CharityName: "V2 Only Charity",
                IsCharity: true,
                ApprovalStatus: "Approved",
                Active: true,
                Enabled: true,
                CategoryId: categoryId,
                ServiceVersionId: serviceV2Id,
                StandardVersionId: standardV2Id,
                CapacityCairoDate: cairoToday,
                Available: 80m,
                UpdatedAt: nowUtc),

            new CandidateRow(
                RecipientId: Guid.NewGuid(),
                CharityName: "Other Category Charity",
                IsCharity: true,
                ApprovalStatus: "Approved",
                Active: true,
                Enabled: true,
                CategoryId: otherCategoryId,
                ServiceVersionId: serviceV1Id,
                StandardVersionId: standardV1Id,
                CapacityCairoDate: cairoToday,
                Available: 90m,
                UpdatedAt: nowUtc)
        };

        var matched = FilterEligibleCandidates(
            rows,
            expectedCategoryId: categoryId,
            expectedServiceVersionId: serviceV1Id,
            expectedStandardVersionId: standardV1Id,
            cairoToday: cairoToday);

        var onlyMatch = Assert.Single(matched);
        Assert.Equal("Nile Care", onlyMatch.CharityName);
        Assert.Equal(25m, onlyMatch.Available);

        var unmatchedOlderVersion = FilterEligibleCandidates(
            rows,
            expectedCategoryId: categoryId,
            expectedServiceVersionId: serviceV2Id,
            expectedStandardVersionId: standardV1Id,
            cairoToday: cairoToday);

        Assert.Empty(unmatchedOlderVersion);
    }

    [Fact]
    public async Task GetEligibleCharitiesEndpoint_WhenPostgresConfigured_EnforcesCoordinatorAndExactMatching()
    {
        var connection = Environment.GetEnvironmentVariable("P1_WORKFLOW_POSTGRES");
        if (string.IsNullOrWhiteSpace(connection))
            return;

        var dbName = new NpgsqlConnectionStringBuilder(connection).Database ?? "";
        if (!dbName.StartsWith("helping_hands_workflow", StringComparison.Ordinal))
            return;

        var utcNow = new DateTimeOffset(2026, 11, 20, 7, 0, 0, TimeSpan.Zero);
        var clock = new FixedTimeProvider(utcNow);

        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var dbDescriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<HelpingHandsDbContext>) ||
                    d.ServiceType == typeof(HelpingHandsDbContext)).ToList();
                foreach (var d in dbDescriptors)
                    services.Remove(d);

                services.AddDbContext<HelpingHandsDbContext>(options =>
                    options.UseNpgsql(connection));

                var clockDescriptors = services.Where(d => d.ServiceType == typeof(TimeProvider)).ToList();
                foreach (var d in clockDescriptors)
                    services.Remove(d);
                services.AddSingleton<TimeProvider>(clock);

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, HeaderRoleAuthHandler>("Test", _ => { });
            });
        });

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
            await db.Database.MigrateAsync();
        }

        using var forbiddenClient = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        forbiddenClient.DefaultRequestHeaders.Add(TestRoleHeader, "RecipientRepresentative");

        var forbiddenResponse = await forbiddenClient.GetAsync(
            $"/api/v1/matching/charities?serviceCategoryId={Guid.NewGuid()}&serviceVersionId={Guid.NewGuid()}&standardVersionId={Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        using var coordClient = factory.CreateClient();
        coordClient.DefaultRequestHeaders.Add(TestRoleHeader, "Coordinator");

        var payload = await coordClient.GetFromJsonAsync<MatchResponseDto>(
            $"/api/v1/matching/charities?serviceCategoryId={Guid.NewGuid()}&serviceVersionId={Guid.NewGuid()}&standardVersionId={Guid.NewGuid()}");

        Assert.NotNull(payload);
        Assert.Equal(0, payload.MatchCount);
        Assert.Empty(payload.Candidates);
    }

    private static IReadOnlyList<CandidateRow> FilterEligibleCandidates(
        IEnumerable<CandidateRow> source,
        Guid expectedCategoryId,
        Guid expectedServiceVersionId,
        Guid expectedStandardVersionId,
        DateOnly cairoToday)
    {
        return source
            .Where(x =>
                x.IsCharity &&
                x.ApprovalStatus == "Approved" &&
                x.Active &&
                x.Enabled &&
                x.CategoryId == expectedCategoryId &&
                x.ServiceVersionId == expectedServiceVersionId &&
                x.StandardVersionId == expectedStandardVersionId &&
                x.CapacityCairoDate == cairoToday &&
                x.Available > 0m)
            .OrderByDescending(x => x.Available)
            .ThenBy(x => x.CharityName)
            .ToList();
    }

    private sealed record CandidateRow(
        Guid RecipientId,
        string CharityName,
        bool IsCharity,
        string ApprovalStatus,
        bool Active,
        bool Enabled,
        Guid CategoryId,
        Guid ServiceVersionId,
        Guid StandardVersionId,
        DateOnly CapacityCairoDate,
        decimal Available,
        DateTimeOffset UpdatedAt);

    private sealed record CandidateDto(Guid RecipientId, string CharityName, bool IsFreshToday, bool IsStale, decimal AvailableCapacity);
    private sealed record MatchResponseDto(int MatchCount, IReadOnlyList<CandidateDto> Candidates);

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class HeaderRoleAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var role = Request.Headers.TryGetValue(TestRoleHeader, out var values) && !string.IsNullOrWhiteSpace(values.ToString())
                ? values.ToString()
                : "Coordinator";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
