using System;
using ClinicPOS.Domain.Enums;
using ClinicPOS.Domain.Interfaces;

namespace ClinicPOS.Domain.Entities
{
    public class User : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Simplified for now
        public UserRole Role { get; set; }

        public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();

    }
}
