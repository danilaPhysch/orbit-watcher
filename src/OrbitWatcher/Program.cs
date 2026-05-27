using OrbitWatcher.Celestrak;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCelestrakOmm(builder.Configuration);

var app = builder.Build();

await app.RunAsync();