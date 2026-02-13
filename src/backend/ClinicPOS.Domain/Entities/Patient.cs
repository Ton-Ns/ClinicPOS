using System;
using ClinicPOS.Domain.Interfaces;

namespace ClinicPOS.Domain.Entities
{
    public class Patient : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public Guid PrimaryBranchId { get; set; }
        // While Branch is not strictly required for navigation based on requirements (1 Patient -> 1 Tenant), 
        // it serves as the "Home Branch".
        public Branch PrimaryBranch { get; set; } = null!;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
