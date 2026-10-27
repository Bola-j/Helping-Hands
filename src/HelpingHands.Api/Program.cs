var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Process liveness only. No database or other dependencies are checked yet.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
