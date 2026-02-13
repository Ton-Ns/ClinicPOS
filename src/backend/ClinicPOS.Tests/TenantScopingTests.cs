using ClinicPOS.Domain.Entities;
using ClinicPOS.Domain.Interfaces;
using ClinicPOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ClinicPOS.Tests;

public class TenantScopingTests
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
    public async Task TenantScoping_Should_Filter_Data()
    {
        // Arrange
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();

        var db1 = GetDbContext(tenant1);
        db1.Patients.Add(new Patient { FirstName = "T1", LastName = "Patient", TenantId = tenant1 });
        await db1.SaveChangesAsync();

        var db2 = GetDbContext(tenant2);
        db2.Patients.Add(new Patient { FirstName = "T2", LastName = "Patient", TenantId = tenant2 });
        await db2.SaveChangesAsync();

        // Act - Switch to Tenant 1 context
        var patients = await db1.Patients.ToListAsync();

        // Assert
        Assert.Single(patients);
        Assert.Equal("T1", patients[0].FirstName);
    }
}
