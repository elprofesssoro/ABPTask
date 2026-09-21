using api.Data;
using api.Models;
using api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddScoped<IHallService, HallService>();

WebApplication app = builder.Build();
app.MapControllers();

using (IServiceScope scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Halls.Any())
    {
        db.Halls.AddRange(
            new Hall { Name = "Зал А", Capacity = 50, CostPerHour = 2000 },
            new Hall { Name = "Зал B", Capacity = 100, CostPerHour = 3500 },
            new Hall { Name = "Зал C", Capacity = 30, CostPerHour = 1500 });
    }

    if (!db.Amenities.Any())
    {
        db.Amenities.AddRange(
            new Amenity { Name = "Проєктор", Cost = 500 },
            new Amenity { Name = "Wi-Fi", Cost = 300 },
            new Amenity { Name = "Звук", Cost = 700 });
    }

    db.SaveChanges();
}

app.Run();
