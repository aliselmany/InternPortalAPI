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
    public DbSet<KanbanTask> KanbanTasks { get; set; }
    public DbSet<TaskComment> TaskComments { get; set; }
    public DbSet<KanbanComment> KanbanComments { get; set; }


    public DbSet<InternTransferRequest> InternTransferRequests { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted))
        {
            if (entry.CurrentValues.Properties.Any(p => p.Name == "IsDeleted"))
            {
                entry.State = EntityState.Modified;
                entry.CurrentValues["IsDeleted"] = true;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InternPortal.Domain.Entities.Application>().HasQueryFilter(a => !a.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<KanbanTask>().HasQueryFilter(k => !k.IsDeleted);
        modelBuilder.Entity<UserSocialAccount>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<TaskComment>().HasQueryFilter(tc => !tc.IsDeleted);

        modelBuilder.Entity<UserRoleMapping>().ToTable("UserRoleMappings");
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
            .HasForeignKey<InternPortal.Domain.Entities.Application>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserSocialAccount>()
            .HasOne(s => s.User)
            .WithMany(u => u.SocialAccounts)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

    
        modelBuilder.Entity<InternTransferRequest>()
            .HasOne(r => r.Intern)
            .WithMany()
            .HasForeignKey(r => r.InternId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InternTransferRequest>()
            .HasOne(r => r.FromStaff)
            .WithMany()
            .HasForeignKey(r => r.FromStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InternTransferRequest>()
            .HasOne(r => r.ToStaff)
            .WithMany()
            .HasForeignKey(r => r.ToStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        var appEntity = modelBuilder.Entity<InternPortal.Domain.Entities.Application>();

        appEntity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        appEntity.Property(e => e.Grade).HasConversion<string>().HasMaxLength(30);
        appEntity.Property(e => e.Department).HasConversion<string>().HasMaxLength(50);
        appEntity.Property(e => e.InternshipType).HasConversion<string>().HasMaxLength(30);
        appEntity.Property(e => e.SchoolName).IsRequired().HasMaxLength(150);
        appEntity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(15);
        appEntity.Property(e => e.Reference).HasMaxLength(100);
        appEntity.Property(e => e.ReferenceGsm).HasMaxLength(15);
        appEntity.Property(e => e.ReferenceClosenessStatus).HasMaxLength(50);
        appEntity.Property(e => e.Description).HasMaxLength(1000);

        modelBuilder.Entity<KanbanTask>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(k => k.InternId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<KanbanTask>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(k => k.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskComment>()
            .HasOne(tc => tc.Task)
            .WithMany()
            .HasForeignKey(tc => tc.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskComment>()
            .HasOne(tc => tc.User)
            .WithMany()
            .HasForeignKey(tc => tc.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}