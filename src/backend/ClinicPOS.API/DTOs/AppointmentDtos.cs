using System;

namespace ClinicPOS.API.DTOs
{
    public class CreateAppointmentDto
    {
        public Guid BranchId { get; set; }
        public Guid PatientId { get; set; }
        public DateTime StartAt { get; set; }
    }

    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public Guid PatientId { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
