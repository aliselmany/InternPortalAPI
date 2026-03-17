using Microsoft.EntityFrameworkCore;
using InternPortal.Domain.Entities;

namespace InternPortal.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<InternPortal.Domain.Entities.Application> Applications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
     
        modelBuilder.Entity<InternPortal.Domain.Entities.Application>()
            .HasOne(a => a.User)
            .WithOne(u => u.Application) 
            .HasForeignKey<InternPortal.Domain.Entities.Application>(a => a.UserId);

        modelBuilder.Entity<User>().Property(u => u.Role).HasConversion<string>();

        var appEntity = modelBuilder.Entity<InternPortal.Domain.Entities.Application>();
        appEntity.Property(e => e.Status).HasConversion<string>();
        appEntity.Property(e => e.StudentGrade).HasConversion<string>();
        appEntity.Property(e => e.SelectedDepartment).HasConversion<string>();
        appEntity.Property(e => e.InternshipType).HasConversion<string>();
    }
}