using System.Net;
using System.Text.Json;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task CharityCapacityUpdates_ReplacesAvailableBalanceAndAppendsAuditHistory()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var (_, service1, _, version1) = await PublishedCatalogAsync(db);
        var (_, service2, _, version2) = await PublishedCatalogAsync(db);

        var repClient = await CreateLoggedInCharityRepresentativeAsync("Rep Capacity");
        var charity = await WorkflowDbFixture.RecipientAsync(repClient);
        var recipientId = charity.GetProperty("id").GetGuid();

        using var selectResponse = await PutWithAntiforgeryAsync(repClient, $"/api/v1/recipients/{recipientId}/services", new
        {
            Services = new[]
            {
                new { ServiceVersionId = service1.Id, StandardVersionId = version1.Id },
                new { ServiceVersionId = service2.Id, StandardVersionId = version2.Id }
            }
        });
        var selectBody = await WorkflowDbFixture.JsonAsync(selectResponse, HttpStatusCode.OK);
        var selected = selectBody.GetProperty("selectedServices").EnumerateArray().ToList();
        var cs1Id = selected.Single(x => x.GetProperty("serviceVersionId").GetGuid() == service1.Id).GetProperty("charityServiceId").GetGuid();
        var cs2Id = selected.Single(x => x.GetProperty("serviceVersionId").GetGuid() == service2.Id).GetProperty("charityServiceId").GetGuid();

        // 1. Initial direct available-from-now capacity update
        using var firstCapPut = await PutWithAntiforgeryAsync(repClient, $"/api/v1/recipients/{recipientId}/capacity", new
        {
            Capacities = new[]
            {
                new { CharityServiceId = cs1Id, Available = 12.5m },
                new { CharityServiceId = cs2Id, Available = 5m }
            }
        });
        var firstCapBody = await WorkflowDbFixture.JsonAsync(firstCapPut, HttpStatusCode.OK);
        var capsAfterFirst = firstCapBody.GetProperty("capacities").EnumerateArray().ToList();
        Assert.Equal(2, capsAfterFirst.Count);
        var cap1Id = capsAfterFirst.Single(x => x.GetProperty("charityServiceId").GetGuid() == cs1Id).GetProperty("capacityId").GetGuid();

        // 2. Subsequent update directly replaces available balance without double-subtracting earlier commitments
        using var secondCapPut = await PutWithAntiforgeryAsync(repClient, $"/api/v1/recipients/{recipientId}/capacity", new
        {
            Capacities = new[]
            {
                new { CharityServiceId = cs1Id, Available = 7.25m },
                new { CharityServiceId = cs2Id, Available = 0m }
            }
        });
        var secondCapBody = await WorkflowDbFixture.JsonAsync(secondCapPut, HttpStatusCode.OK);
        var capsAfterSecond = secondCapBody.GetProperty("capacities").EnumerateArray().ToList();
        Assert.Equal(7.25m, capsAfterSecond.Single(x => x.GetProperty("charityServiceId").GetGuid() == cs1Id).GetProperty("available").GetDecimal());
        Assert.Equal(0m, capsAfterSecond.Single(x => x.GetProperty("charityServiceId").GetGuid() == cs2Id).GetProperty("available").GetDecimal());

        // 3. Verify GET returns current Cairo day capacities and append-only CapacityUpdate rows exist
        using var getCapResponse = await repClient.GetAsync($"/api/v1/recipients/{recipientId}/capacity");
        var getCapBody = await WorkflowDbFixture.JsonAsync(getCapResponse, HttpStatusCode.OK);
        Assert.Equal(2, getCapBody.GetProperty("capacities").GetArrayLength());

        var updatesInDb = await db.CapacityUpdates.AsNoTracking()
            .Where(u => u.CapacityId == cap1Id)
            .OrderBy(u => u.UpdatedAt)
            .ToListAsync();
        Assert.Equal(2, updatesInDb.Count);
        Assert.Equal(12.5m, updatesInDb[0].Available);
        Assert.Equal(7.25m, updatesInDb[1].Available);
    }

    [Fact]
    public async Task CharityCapacityUpdates_RejectsNegativeQuantitiesAndDisabledOrForeignServices()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var (_, service1, _, version1) = await PublishedCatalogAsync(db);
        var (_, service2, _, version2) = await PublishedCatalogAsync(db);

        var repClient = await CreateLoggedInCharityRepresentativeAsync("Rep Cap Validation");
        var charity = await WorkflowDbFixture.RecipientAsync(repClient);
        var recipientId = charity.GetProperty("id").GetGuid();

        // Enable both services then disable service2
        using var initialSelect = await PutWithAntiforgeryAsync(repClient, $"/api/v1/recipients/{recipientId}/services", new
        {
            Services = new[]
            {
                new { ServiceVersionId = service1.Id, StandardVersionId = version1.Id },
                new { ServiceVersionId = service2.Id, StandardVersionId = version2.Id }
            }
        });
        var selectBody = await WorkflowDbFixture.JsonAsync(initialSelect, HttpStatusCode.OK);
        var selected = selectBody.GetProperty("selectedServices").EnumerateArray().ToList();
        var cs1Id = selected.Single(x => x.GetProperty("serviceVersionId").GetGuid() == service1.Id).GetProperty("charityServiceId").GetGuid();
        var cs2Id = selected.Single(x => x.GetProperty("serviceVersionId").GetGuid() == service2.Id).GetProperty("charityServiceId").GetGuid();

        using var disableService2 = await PutWithAntiforgeryAsync(repClient, $"/api/v1/recipients/{recipientId}/services", new
        {
            Services = new[]
            {
                new { ServiceVersionId = service1.Id, StandardVersionId = version1.Id }
            }
        });
        Assert.Equal(HttpStatusCode.OK, disableService2.StatusCode);

        // Negative available quantity -> 400 BadRequest
        using var negativeResponse = await PutWithAntiforgeryAsync(repClient, $"/api/v1/recipients/{recipientId}/capacity", new
        {
            Capacities = new[] { new { CharityServiceId = cs1Id, Available = -1m } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, negativeResponse.StatusCode);

        // Disabled service -> 400 BadRequest
        using var disabledResponse = await PutWithAntiforgeryAsync(repClient, $"/api/v1/recipients/{recipientId}/capacity", new
        {
            Capacities = new[] { new { CharityServiceId = cs2Id, Available = 10m } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, disabledResponse.StatusCode);
    }

    [Fact]
    public async Task CharityCapacityUpdates_EnforcesRepresentativeAuthorization()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var (_, service1, _, version1) = await PublishedCatalogAsync(db);

        var ownerClient = await CreateLoggedInCharityRepresentativeAsync("Rep Cap Owner");
        var charityA = await WorkflowDbFixture.RecipientAsync(ownerClient);
        var charityAId = charityA.GetProperty("id").GetGuid();

        using var selectRes = await PutWithAntiforgeryAsync(ownerClient, $"/api/v1/recipients/{charityAId}/services", new
        {
            Services = new[] { new { ServiceVersionId = service1.Id, StandardVersionId = version1.Id } }
        });
        var selectBody = await WorkflowDbFixture.JsonAsync(selectRes, HttpStatusCode.OK);
        var cs1Id = selectBody.GetProperty("selectedServices").EnumerateArray().First().GetProperty("charityServiceId").GetGuid();

        // Other charity representative gets 404 NotFound
        var otherRepClient = await CreateLoggedInCharityRepresentativeAsync("Rep Cap Other");
        await WorkflowDbFixture.RecipientAsync(otherRepClient);
        using var otherPut = await PutWithAntiforgeryAsync(otherRepClient, $"/api/v1/recipients/{charityAId}/capacity", new
        {
            Capacities = new[] { new { CharityServiceId = cs1Id, Available = 5m } }
        });
        Assert.Equal(HttpStatusCode.NotFound, otherPut.StatusCode);

        // Beneficiary gets 403 Forbidden
        var beneficiaryClient = fixture.Client();
        var benEmail = $"ben-{Guid.NewGuid():N}@example.test";
        await WorkflowDbFixture.RegisterBeneficiaryAsync(beneficiaryClient, "Fictional Beneficiary", benEmail, fixture.Password);
        await WorkflowDbFixture.LoginRegisteredAsync(beneficiaryClient, benEmail, fixture.Password);

        using var benPut = await PutWithAntiforgeryAsync(beneficiaryClient, $"/api/v1/recipients/{charityAId}/capacity", new
        {
            Capacities = new[] { new { CharityServiceId = cs1Id, Available = 5m } }
        });
        Assert.Equal(HttpStatusCode.Forbidden, benPut.StatusCode);
    }
}