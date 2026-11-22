using System.Net;
using System.Net.Http.Json;
using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class EligibleCharityMatchingTests
{
    [Fact]
    [Trait("Category", "PostgresIntegration")]
    public async Task MatchingApi_ReturnsOnlyFreshApprovedExactVersionCharity()
    {
        var connection = Environment.GetEnvironmentVariable("P1_WORKFLOW_POSTGRES")
            ?? throw new InvalidOperationException("P1_WORKFLOW_POSTGRES is required for the matching API regression.");
        if (!(new NpgsqlConnectionStringBuilder(connection).Database ?? "").StartsWith("helping_hands_workflow", StringComparison.Ordinal))
            throw new InvalidOperationException("Use an isolated helping_hands_workflow database for this test.");

        await using var factory = CreateMatchingFactory(connection);
        var (category, service, standard, charity) = await SeedMatchingCandidateAsync(factory);
        var path = $"/api/v1/matching/charities?serviceCategoryId={category}&serviceVersionId={service}&standardVersionId={standard}";
        using var coordinator = factory.CreateClient();
        coordinator.DefaultRequestHeaders.Add(TestRoleHeader, "Coordinator");
        var result = await coordinator.GetFromJsonAsync<MatchResponseDto>(path);
        Assert.Equal(charity, Assert.Single(result!.Candidates).RecipientId);
        Assert.Equal(1, result.MatchCount);

        using var representative = factory.CreateClient();
        representative.DefaultRequestHeaders.Add(TestRoleHeader, "RecipientRepresentative");
        Assert.Equal(HttpStatusCode.Forbidden, (await representative.GetAsync(path)).StatusCode);
        var differentVersion = $"/api/v1/matching/charities?serviceCategoryId={category}&serviceVersionId={service}&standardVersionId={Guid.NewGuid()}";
        Assert.Empty((await coordinator.GetFromJsonAsync<MatchResponseDto>(differentVersion))!.Candidates);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        (await db.Recipients.SingleAsync(item => item.Id == charity)).Active = false;
        await db.SaveChangesAsync();
        Assert.Empty((await coordinator.GetFromJsonAsync<MatchResponseDto>(path))!.Candidates);
    }

    private static WebApplicationFactory<Program> CreateMatchingFactory(string connection) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            foreach (var descriptor in services.Where(item => item.ServiceType == typeof(DbContextOptions<HelpingHandsDbContext>) ||
                item.ServiceType == typeof(HelpingHandsDbContext)).ToList())
                services.Remove(descriptor);
            services.AddDbContext<HelpingHandsDbContext>(options => options.UseNpgsql(connection));
            foreach (var descriptor in services.Where(item => item.ServiceType == typeof(TimeProvider)).ToList())
                services.Remove(descriptor);
            services.AddSingleton<TimeProvider>(new FixedTimeProvider(new DateTimeOffset(2026, 11, 20, 7, 0, 0, TimeSpan.Zero)));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, HeaderRoleAuthHandler>("Test", _ => { });
        }));

    private static async Task<(Guid Category, Guid Service, Guid Standard, Guid Charity)> SeedMatchingCandidateAsync(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        await db.Database.MigrateAsync();
        var category = new ServiceCategory { Code = Guid.NewGuid().ToString("N") };
        var service = new ServiceVersion { CategoryId = category.Id, VersionNumber = 1, Name = "Fictional food" };
        var unit = new UnitStandard { ServiceVersionId = service.Id, Name = "Fictional parcel" };
        var standard = new StandardVersion { UnitStandardId = unit.Id, VersionNumber = 1, EffectiveAt = new DateTimeOffset(2026, 11, 17, 0, 0, 0, TimeSpan.Zero) };
        db.AddRange(category, service, unit, standard, new StandardComponent { StandardVersionId = standard.Id, Name = "Fictional food", Quantity = 1, Unit = "parcel" });
        await db.SaveChangesAsync();
        service.IsPublished = true;
        await db.SaveChangesAsync();
        standard.IsPublished = true;
        await db.SaveChangesAsync();
        var charity = new Recipient { Reference = "REC-" + Guid.NewGuid().ToString("N")[..20], DisplayName = "Fictional charity", ApprovalStatus = "Approved", Active = true };
        var selection = new CharityService { CharityId = charity.Id, ServiceVersionId = service.Id, StandardVersionId = standard.Id };
        db.AddRange(charity, new Charity { RecipientId = charity.Id }, selection);
        db.DailyCapacities.Add(new DailyCapacity { CharityServiceId = selection.CharityServiceId, CairoDate = new DateOnly(2026, 11, 20), Available = 3m, UpdatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
        return (category.Id, service.Id, standard.Id, charity.Id);
    }
}
