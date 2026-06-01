using LigaVoleibol.API.Models;

namespace LigaVoleibol.API.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Teams.Any()) return;

        var leones = new Team { Name = "Leones de Bogotá", Category = "Masculino", Venue = "Coliseo El Campin", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        var aguilas = new Team { Name = "Águilas Doradas", Category = "Femenino", Venue = "Palacio de los Deportes", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        db.Teams.AddRange(leones, aguilas);
        db.SaveChanges();

        db.Players.AddRange(
            new Player { FirstName = "Carlos", LastName = "Pérez", Position = "Libero", JerseyNumber = 7, TeamId = leones.Id },
            new Player { FirstName = "Andrés", LastName = "García", Position = "Central", JerseyNumber = 12, TeamId = leones.Id },
            new Player { FirstName = "María", LastName = "López", Position = "Opuesto", JerseyNumber = 4, TeamId = aguilas.Id },
            new Player { FirstName = "Laura", LastName = "Martínez", Position = "Punta", JerseyNumber = 9, TeamId = aguilas.Id }
        );

        db.Matches.Add(new Match
        {
            HomeTeamId = leones.Id, AwayTeamId = aguilas.Id,
            ScheduledAt = new DateTime(2026, 6, 15, 18, 0, 0, DateTimeKind.Utc),
            Venue = "Coliseo El Campin", Status = MatchStatus.Scheduled
        });

        db.SaveChanges();
    }
}
