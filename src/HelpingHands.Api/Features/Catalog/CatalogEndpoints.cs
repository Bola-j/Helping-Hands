namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this RouteGroupBuilder api)
    {
        var catalog = api.MapGroup("/catalog").RequireAuthorization();
        catalog.MapGet("/current", CurrentCatalog).RequireAuthorization(policy => policy.RequireRole("Beneficiary", "RecipientRepresentative", "Coordinator", "Administrator"));
        var staff = catalog.MapGroup("").RequireAuthorization(policy => policy.RequireRole("Coordinator", "Administrator"));
        staff.MapGet("/categories", Categories);
        staff.MapGet("/categories/{id:guid}", GetCategory);
        staff.MapGet("/service-versions/{id:guid}", GetServiceVersion);
        staff.MapGet("/standards/{id:guid}", GetStandard);
        staff.MapGet("/standard-versions/{id:guid}", GetStandardVersion);
        staff.MapGet("/components/{id:guid}", GetComponent);
        staff.MapGet("/categories/{id:guid}/versions", ServiceVersions);
        staff.MapGet("/service-versions/{id:guid}/standards", Standards);
        staff.MapGet("/standards/{id:guid}/versions", StandardVersions);
        staff.MapPost("/categories", CreateCategory);
        staff.MapPut("/categories/{id:guid}", UpdateCategory);
        staff.MapPost("/categories/{id:guid}/versions", CreateServiceVersion);
        staff.MapPut("/service-versions/{id:guid}", UpdateServiceVersion);
        staff.MapPost("/service-versions/{id:guid}/publish", PublishServiceVersion);
        staff.MapPost("/service-versions/{id:guid}/standards", CreateStandard);
        staff.MapPut("/standards/{id:guid}", UpdateStandard);
        staff.MapPost("/standards/{id:guid}/versions", CreateStandardVersion);
        staff.MapPut("/standard-versions/{id:guid}", UpdateStandardVersion);
        staff.MapPost("/standard-versions/{id:guid}/publish", PublishStandardVersion);
        staff.MapPost("/standard-versions/{id:guid}/components", CreateComponent);
        staff.MapPut("/components/{id:guid}", UpdateComponent);
        staff.MapDelete("/components/{id:guid}", DeleteComponent);
    }
}
