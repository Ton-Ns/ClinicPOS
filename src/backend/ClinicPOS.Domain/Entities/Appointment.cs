using ClinicPOS.Domain.Interfaces;
using System;

namespace ClinicPOS.Domain.Entities
{
    public class Appointment : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public Guid BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public DateTime StartAt { get; set; }
    }
}
