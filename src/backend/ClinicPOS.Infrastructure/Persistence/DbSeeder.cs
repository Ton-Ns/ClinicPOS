using System;
using System.Linq;
using System.Threading.Tasks;
using ClinicPOS.Domain.Entities;
using ClinicPOS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicPOS.Infrastructure.Persistence
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Check if any tenant exists
            if (await context.Tenants.AnyAsync()) return;

            // Create Tenant
            // Use static GUID for deterministic testing
            var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var tenant = new Tenant { Id = tenantId, Name = "Main Clinic" };
            context.Tenants.Add(tenant);

            await context.SaveChangesAsync();

            // Create Branches
            var branch1Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var branch2Id = Guid.Parse("22222222-2222-2222-2222-333333333333");

            var branch1 = new Branch { Id = branch1Id, Name = "Branch A", TenantId = tenantId };
            var branch2 = new Branch { Id = branch2Id, Name = "Branch B", TenantId = tenantId };

            context.Branches.AddRange(branch1, branch2);
            await context.SaveChangesAsync();

            // Create Users
            var adminId = Guid.Parse("33333333-3333-3333-1111-111111111111");
            var userId = Guid.Parse("33333333-3333-3333-2222-222222222222");
            var viewerId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            var admin = new User
            {
                Id = adminId,
                Username = "admin",
                Role = UserRole.Admin,
                TenantId = tenantId,
                UserBranches = new List<UserBranch>
                {
                    new UserBranch { BranchId = branch1Id },
                    new UserBranch { BranchId = branch2Id }
                }
            };
            var user = new User
            {
                Id = userId,
                Username = "user",
                Role = UserRole.User,
                TenantId = tenantId,
                UserBranches = new List<UserBranch>
                {
                    new UserBranch { BranchId = branch1Id }
                }
            };
            var viewer = new User
            {
                Id = viewerId,
                Username = "viewer",
                Role = UserRole.Viewer,
                TenantId = tenantId,
                UserBranches = new List<UserBranch>
                {
                    new UserBranch { BranchId = branch2Id }
                }
            };

            context.Users.AddRange(admin, user, viewer);
            await context.SaveChangesAsync();

            // Create some Sample Patients for smoke test
            var patient1 = new Patient
            {
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "1234567890",
                TenantId = tenantId,
                PrimaryBranchId = branch1Id
            };
            var patient2 = new Patient
            {
                FirstName = "Jane",
                LastName = "Smith",
                PhoneNumber = "0987654321",
                TenantId = tenantId,
                PrimaryBranchId = branch2Id
            };
            context.Patients.AddRange(patient1, patient2);
            await context.SaveChangesAsync();
        }
    }
}
