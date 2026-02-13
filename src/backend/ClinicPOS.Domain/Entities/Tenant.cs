using System.Collections.Generic;

namespace ClinicPOS.Domain.Entities
{
    public class Tenant : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Patient> Patients { get; set; } = new List<Patient>();
    }
}
