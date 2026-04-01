using Microsoft.EntityFrameworkCore;
using InternPortal.Domain.Entities;

namespace InternPortal.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRoleMapping> UserRoleMappings { get; set; }
    public DbSet<InternPortal.Domain.Entities.Application> Applications { get; set; }

    
    public DbSet<UserSocialAccount> UserSocialAccounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<UserRoleMapping>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRoleMapping>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRoleMapping>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Mentor)
            .WithMany(m => m.Interns)
            .HasForeignKey(u => u.MentorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InternPortal.Domain.Entities.Application>()
            .HasOne(a => a.User)
            .WithOne(u => u.Application)
            .HasForeignKey<InternPortal.Domain.Entities.Application>(a => a.UserId);

        modelBuilder.Entity<UserSocialAccount>()
            .HasOne(s => s.User)
            .WithMany(u => u.SocialAccounts)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade); 

        var appEntity = modelBuilder.Entity<InternPortal.Domain.Entities.Application>();
        appEntity.Property(e => e.Status).HasConversion<string>();
        appEntity.Property(e => e.StudentGrade).HasConversion<string>();
        appEntity.Property(e => e.Department).HasConversion<string>();
        appEntity.Property(e => e.InternshipType).HasConversion<string>();
    }
}