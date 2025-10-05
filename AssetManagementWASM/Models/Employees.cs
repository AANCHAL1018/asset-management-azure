using System;
using System.Collections.Generic;

namespace AssetManagementWASM.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
