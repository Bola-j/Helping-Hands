using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Referrals;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task<CapacityAllocationRun> PrepareCapacityAllocationRunAsync()
    {
        var representative = await fixture.LoginAsync("RecipientRepresentative");
        var admin = await fixture.LoginAsync("Administrator");
        var coordinator = await fixture.LoginAsync("Coordinator");
        var beneficiary = await fixture.LoginAsync("Beneficiary");
        var scenario = await CreateCapacityScenarioAsync(representative, admin, coordinator, beneficiary);
        var scope = fixture.Factory.Services.CreateScope();
        return new(representative, admin, coordinator, beneficiary, scope, scenario);
    }

    private async Task<CapacityAllocationScenario> CreateCapacityScenarioAsync(
        HttpClient representative, HttpClient admin, HttpClient coordinator, HttpClient beneficiary)
    {
        var (charity, accepted) = await WorkflowDbFixture.AcceptedNeedAsync(
            representative, admin, coordinator, beneficiary);
        var setup = await WorkflowDbFixture.PrepareLegacyReferralAsync(fixture, coordinator, charity, accepted);
        var recipientId = charity.GetProperty("id").GetGuid();
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var serviceId = await database.CharityServices
            .Where(item => item.CharityId == recipientId && item.ServiceVersionId == setup.ServiceVersionId)
            .Select(item => item.CharityServiceId)
            .SingleAsync();
        return new(accepted.GetProperty("id").GetGuid(), recipientId, setup.ServiceVersionId,
            setup.StandardVersionId, serviceId);
    }

}

internal sealed record CapacityAllocationScenario(
    Guid RequestId, Guid RecipientId, Guid ServiceVersionId, Guid StandardVersionId, Guid CharityServiceId);

internal sealed record CapacityAllocationRun(
    HttpClient Representative, HttpClient Admin, HttpClient Coordinator, HttpClient Beneficiary,
    IServiceScope AllocationScope, CapacityAllocationScenario Scenario);
