using System;

namespace ClinicPOS.Domain.Events
{
    public record AppointmentCreated(Guid AppointmentId, Guid TenantId, Guid BranchId, DateTime StartAt);
}
