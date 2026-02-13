using ClinicPOS.Domain.Entities;
using ClinicPOS.Domain.Interfaces;
using ClinicPOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ClinicPOS.Tests;

public class DuplicatePhoneTests
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    private ApplicationDbContext GetDbContext(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: _dbName)
            .Options;

        var tenantServiceMock = new Mock<ITenantService>();
        tenantServiceMock.Setup(m => m.GetTenantId()).Returns(tenantId);

        return new ApplicationDbContext(options, tenantServiceMock.Object);
    }

    [Fact]
    public async Task DuplicatePhoneCheck_Should_Be_Scoped_To_Tenant()
    {
        // Arrange
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();
        var phoneNumber = "1234567890";

        // Seed tenant 2 with the same phone number
        var db2 = GetDbContext(tenant2);
        db2.Patients.Add(new Patient { FirstName = "Other", LastName = "Clinic", PhoneNumber = phoneNumber, TenantId = tenant2 });
        await db2.SaveChangesAsync();

        // Act - Check in Tenant 1 context
        var db1 = GetDbContext(tenant1);
        // Tenant 1 doesn't have it yet, so it should be allowed (AnyAsync should return false)
        var existsInT1 = await db1.Patients.AnyAsync(p => p.PhoneNumber == phoneNumber);

        // Assert
        Assert.False(existsInT1);

        // Seed Tenant 1
        db1.Patients.Add(new Patient { FirstName = "Me", LastName = "Clinic", PhoneNumber = phoneNumber, TenantId = tenant1 });
        await db1.SaveChangesAsync();

        // Now it should exist in T1
        var existsInT1Now = await db1.Patients.AnyAsync(p => p.PhoneNumber == phoneNumber);
        Assert.True(existsInT1Now);
    }
}
