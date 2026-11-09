using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayContext : DbContext
{
    public RaceDayContext(DbContextOptions<RaceDayContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(r => r.Name).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasOne(e => e.Organiser)
                  .WithMany(u => u.OrganisedEvents)
                  .HasForeignKey(e => e.OrganiserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<EventCategory>(entity =>
        {
            entity.HasIndex(ec => new { ec.EventId, ec.CategoryId }).IsUnique();

            entity.HasOne(ec => ec.Event)
                  .WithMany(e => e.EventCategories)
                  .HasForeignKey(ec => ec.EventId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ec => ec.Category)
                  .WithMany(c => c.EventCategories)
                  .HasForeignKey(ec => ec.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.HasIndex(en => new { en.ParticipantId, en.EventCategoryId }).IsUnique();

            entity.HasOne(en => en.Participant)
                  .WithMany(u => u.Enrolments)
                  .HasForeignKey(en => en.ParticipantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(en => en.EventCategory)
                  .WithMany(ec => ec.Enrolments)
                  .HasForeignKey(en => en.EventCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasIndex(r => r.EnrolmentId).IsUnique();

            entity.HasOne(r => r.Enrolment)
                  .WithOne(en => en.Result)
                  .HasForeignKey<Result>(r => r.EnrolmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed the two fixed roles. Registration only ever assigns one of
        // these two - there is no endpoint to create new roles, so a user
        // can never register as anything other than Organiser or Participant.
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Organiser" },
            new Role { Id = 2, Name = "Participant" }
        );
    }
}
