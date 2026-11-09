using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests;

/// <summary>
/// Spins up the real RaceDay API in-memory for integration testing, but
/// swaps the real SQL Server database for an EF Core in-memory database
/// so tests never touch a real SQL Server instance. Each factory instance
/// gets its own isolated database, keyed by a random name.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public string DbName { get; } = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<RaceDayContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<RaceDayContext>(options =>
            {
                options.UseInMemoryDatabase(DbName);
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
            db.Database.EnsureCreated();

            // The in-memory provider also needs the two roles seeded,
            // since HasData only applies automatically to relational
            // providers running real migrations.
            if (!db.Roles.Any())
            {
                db.Roles.AddRange(
                    new Role { Id = 1, Name = "Organiser" },
                    new Role { Id = 2, Name = "Participant" });
                db.SaveChanges();
            }
        });
    }
}
