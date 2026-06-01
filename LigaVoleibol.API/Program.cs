using Microsoft.EntityFrameworkCore;
using LigaVoleibol.API.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var isTestEnvironment = builder.Environment.IsEnvironment("Testing");
var testDbName = isTestEnvironment ? "TestDb_" + Guid.NewGuid() : null;
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (isTestEnvironment)
        options.UseInMemoryDatabase(testDbName!);
    else
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.ProviderName?.Contains("InMemory") == true)
    {
        db.Database.EnsureCreated();
        // No seed in tests — each test manages its own data
    }
    else
    {
        var retries = 5;
        while (retries > 0)
        {
            try
            {
                db.Database.Migrate();
                break;
            }
            catch (Exception ex) when (retries > 1)
            {
                retries--;
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("DB migration failed, retrying ({retries} left): {msg}", retries, ex.Message);
                Thread.Sleep(3000);
            }
        }
        DbSeeder.Seed(db);
    }
}

app.Run();

public partial class Program { }
