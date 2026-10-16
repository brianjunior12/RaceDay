using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().ToTable("Role");
        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<Event>().ToTable("Event");
        modelBuilder.Entity<Category>().ToTable("Category");
        modelBuilder.Entity<Enrolment>().ToTable("Enrolment");
        modelBuilder.Entity<Result>().ToTable("Results");
        modelBuilder.Entity<Enrolment>().ToTable("Enrolment");
        modelBuilder.Entity<Result>().ToTable("Results");

        modelBuilder.Entity<Result>().HasKey(r => r.ResultsID);
            

        modelBuilder.Entity<User>()
            .Property(u => u.PasswordHash)
            .HasColumnName("password_hash");

        modelBuilder.Entity<Event>()
            .Property(e => e.OrganiserID)
            .HasColumnName("organiserID");

        modelBuilder.Entity<Event>()
            .Property(e => e.Distance)
            .HasPrecision(5, 2);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.RoleName)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleID);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(u => u.Events)
            .HasForeignKey(e => e.OrganiserID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventID);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Participant)
            .WithMany(u => u.Enrolments)
            .HasForeignKey(e => e.ParticipantID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Event)
            .WithMany(e => e.Enrolments)
            .HasForeignKey(e => e.EventID);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(e => e.CategoryID);

        modelBuilder.Entity<Result>()

            .HasOne(r => r.Enrolment)
            .WithOne(e => e.Result)
            .HasForeignKey<Result>(r => r.EnrolmentID);
            
    }
}