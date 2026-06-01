using Microsoft.EntityFrameworkCore;
using LigaVoleibol.API.Models;

namespace LigaVoleibol.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Match>()
            .HasOne(m => m.HomeTeam)
            .WithMany()
            .HasForeignKey(m => m.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Match>()
            .HasOne(m => m.AwayTeam)
            .WithMany()
            .HasForeignKey(m => m.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Team>().HasData(
            new Team { Id = 1, Name = "Leones de Bogotá", Category = "Masculino", Venue = "Coliseo El Campin", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Team { Id = 2, Name = "Águilas Doradas", Category = "Femenino", Venue = "Palacio de los Deportes", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<Player>().HasData(
            new Player { Id = 1, FirstName = "Carlos", LastName = "Pérez", Position = "Libero", JerseyNumber = 7, TeamId = 1 },
            new Player { Id = 2, FirstName = "Andrés", LastName = "García", Position = "Central", JerseyNumber = 12, TeamId = 1 },
            new Player { Id = 3, FirstName = "María", LastName = "López", Position = "Opuesto", JerseyNumber = 4, TeamId = 2 },
            new Player { Id = 4, FirstName = "Laura", LastName = "Martínez", Position = "Punta", JerseyNumber = 9, TeamId = 2 }
        );

        modelBuilder.Entity<Match>().HasData(
            new Match
            {
                Id = 1, HomeTeamId = 1, AwayTeamId = 2,
                ScheduledAt = new DateTime(2026, 6, 15, 18, 0, 0, DateTimeKind.Utc),
                Venue = "Coliseo El Campin", Status = MatchStatus.Scheduled
            }
        );
    }
}
