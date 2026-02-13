using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicPOS.API.DTOs
{
    public class CreatePatientDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public Guid BranchId { get; set; }
    }

    public class PatientDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Guid PrimaryBranchId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
