using System;

namespace ClinicPOS.Domain.Interfaces
{
    public interface ITenantService
    {
        Guid? GetTenantId();
    }
}
