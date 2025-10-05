using System;
using System.ComponentModel.DataAnnotations;

namespace AssetManagementAPI.Models
{
    public class Asset
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [StringLength(100)]
        public string MakeModel { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [CustomValidation(typeof(Asset), nameof(ValidatePurchaseDate))]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        [CustomValidation(typeof(Asset), nameof(ValidateWarrantyExpiry))]
        public DateTime WarrantyExpiryDate { get; set; }

        [Required]
        [RegularExpression("New|Good|Needs Repair|Damaged", ErrorMessage = "Condition must be New, Good, Needs Repair, or Damaged")]
        public string Condition { get; set; } = "New";

        [Required]
        [RegularExpression("Available|Assigned|Under Repair|Retired", ErrorMessage = "Status must be Available, Assigned, Under Repair, or Retired")]
        public string Status { get; set; } = "Available";

        public bool IsSpare { get; set; } = false;

        [StringLength(500)]
        public string Specifications { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        // 🔹 Custom Validations
        public static ValidationResult? ValidatePurchaseDate(DateTime date, ValidationContext context)
        {
            if (date > DateTime.Now)
                return new ValidationResult("Purchase date cannot be in the future.");
            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateWarrantyExpiry(DateTime expiryDate, ValidationContext context)
        {
            var instance = context.ObjectInstance as Asset;
            if (instance != null && expiryDate < instance.PurchaseDate)
                return new ValidationResult("Warranty expiry date cannot be before purchase date.");
            return ValidationResult.Success;
        }
    }
}
