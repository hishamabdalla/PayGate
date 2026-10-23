// Grows with the public API: each phase adds calls here so the packages are exercised under
// strict consumer settings, not just referenced.
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "PayGate consumer check");

await app.RunAsync();
