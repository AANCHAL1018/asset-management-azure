using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AssetManagementAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required, StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Designation { get; set; } = string.Empty;

        [Required]
        [RegularExpression("Active|Inactive", ErrorMessage = "Status must be either Active or Inactive")]
        public string Status { get; set; } = "Active";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public List<Asset> Assets { get; set; } = new List<Asset>();
    }
}
