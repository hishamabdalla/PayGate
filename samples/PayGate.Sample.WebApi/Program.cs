var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "PayGate sample");

await app.RunAsync();
