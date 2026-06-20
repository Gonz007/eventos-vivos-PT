using EventBooking.API.Middleware;
using EventBooking.Application;
using EventBooking.Infrastructure;
using EventBooking.Infrastructure.Persistence;
using EventBooking.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ── Resolve DB path to repository root ──────────────────────────────────────
// Regardless of the working directory when `dotnet run` is invoked,
// the DB file always lands at <repo-root>/eventbooking.db
var repoRoot    = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var dbPath      = Path.Combine(repoRoot, "eventbooking.db");
var connString  = $"Data Source={dbPath}";

// Override whatever is in appsettings so tests/CI never need to touch it
builder.Configuration["ConnectionStrings:DefaultConnection"] = connString;
// ─────────────────────────────────────────────────────────────────────────────

// --- Services ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "EventBooking API",
        Version     = "v1",
        Description = "Sistema de reservas EventosVivos — SQLite · eventbooking.db en raíz del repositorio"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// --- Middleware pipeline ---
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventBooking API v1"));
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// --- Apply migrations & seed on startup ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (app.Environment.IsEnvironment("Testing"))
    {
        // Integration tests inject their own SQLite in-memory context
        db.Database.EnsureCreated();
    }
    else
    {
        // Creates eventbooking.db at repo root and applies all pending migrations
        await db.Database.MigrateAsync();
        app.Logger.LogInformation("Database ready at: {DbPath}", dbPath);
    }

    await ApplicationDbContextSeed.SeedAsync(db);
}

app.Run();

// Exposed for integration tests
public partial class Program { }
