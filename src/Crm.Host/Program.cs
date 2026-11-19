var app = WebApplication.CreateBuilder(args).Build();

var api = app.MapGroup("/api/v1");
api.MapGet("/health", () => new HealthResponse("ok"));

app.Run();

internal sealed record HealthResponse(string Status);
