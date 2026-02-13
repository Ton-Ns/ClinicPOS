using ClinicPOS.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace ClinicPOS.Domain.Entities
{
    public class Branch : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
    }
}
