using System;

namespace ClinicPOS.Domain.Interfaces
{
    public interface ITenantEntity
    {
        Guid TenantId { get; set; }
    }
}
