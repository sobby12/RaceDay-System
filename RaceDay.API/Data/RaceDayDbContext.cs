
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
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
        public DbSet<EventEnrollment> EventEnrollments => Set<EventEnrollment>();
        public DbSet<Result> Result => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Roles
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            // Users
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne<Role>()
                .WithMany()
                .HasForeignKey(u => u.RoleId);

            // Events
            modelBuilder.Entity<Event>()
                .Property(e => e.EventDate)
                .HasColumnType("date");

            modelBuilder.Entity<Event>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.OrganizerId);

            // Categories
            modelBuilder.Entity<Category>()
                .HasIndex(c => new { c.EventId, c.CategoryName })
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasAlternateKey(c => new { c.EventId, c.CategoryId });

            modelBuilder.Entity<Category>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(c => c.EventId);

            // Event enrollments
            modelBuilder.Entity<EventEnrollment>()
                .HasIndex(e => new { e.EventId, e.UserId })
                .IsUnique();

            modelBuilder.Entity<EventEnrollment>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(e => e.EventId);

            modelBuilder.Entity<EventEnrollment>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<EventEnrollment>()
                .HasOne<Category>()
                .WithMany()
                .HasForeignKey(e => new { e.EventId, e.CategoryId })
                .HasPrincipalKey(c => new { c.EventId, c.CategoryId });

            modelBuilder.Entity<EventEnrollment>()
                .Property(e => e.EnrollmentDate)
                .HasDefaultValueSql("SYSDATETIME()");

            modelBuilder.Entity<EventEnrollment>()
                .Property(e => e.Status)
                .HasDefaultValue("Registered");


            // Results
            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrollmentId)
                .IsUnique();

            modelBuilder.Entity<Result>()
                .HasOne<EventEnrollment>()
                .WithMany()
                .HasForeignKey(r => r.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Result>()
                .Property(r => r.ResultStatus)
                .HasDefaultValue("Official");

        }
    }
}
