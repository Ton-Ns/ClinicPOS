using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicPOS.Domain.Entities;
using ClinicPOS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace ClinicPOS.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ITenantService _tenantService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService tenantService)
            : base(options)
        {
            _tenantService = tenantService;
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserBranch> UserBranches { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.TenantId, a.BranchId, a.PatientId, a.StartAt })
                .IsUnique();

            modelBuilder.Entity<UserBranch>()
                .HasKey(ub => new { ub.UserId, ub.BranchId });

            modelBuilder.Entity<UserBranch>()
                .HasOne(ub => ub.User)
                .WithMany(u => u.UserBranches)
                .HasForeignKey(ub => ub.UserId);

            modelBuilder.Entity<UserBranch>()
                .HasOne(ub => ub.Branch)
                .WithMany()
                .HasForeignKey(ub => ub.BranchId);

            // Apply Global Query Filter for Multi-tenancy
            Expression<Func<ITenantEntity, bool>> filterExpr = entity =>
                !_tenantService.GetTenantId().HasValue || entity.TenantId == _tenantService.GetTenantId().GetValueOrDefault();

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var body = ReplacingExpressionVisitor.Replace(filterExpr.Parameters[0], parameter, filterExpr.Body);
                    var lambdaExpression = Expression.Lambda(body, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambdaExpression);
                }
            }

            // Explicit Configurations
            modelBuilder.Entity<Patient>()
                .HasIndex(p => new { p.TenantId, p.PhoneNumber })
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();
        }

        public override int SaveChanges()
        {
            UpdateTenantIds();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTenantIds();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTenantIds()
        {
            var tenantId = _tenantService.GetTenantId();
            if (!tenantId.HasValue) return;

            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.TenantId = tenantId.Value;
                }
            }
        }
    }
}
