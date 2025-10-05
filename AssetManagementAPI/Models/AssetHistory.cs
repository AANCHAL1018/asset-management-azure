using System;
using System.ComponentModel.DataAnnotations;

namespace AssetManagementAPI.Models
{
    public class AssetHistory
    {
        public int Id { get; set; }

        [Required]
        public int AssetId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        [CustomValidation(typeof(AssetHistory), nameof(ValidateReturnedDate))]
        public DateTime? ReturnedDate { get; set; }

        [StringLength(300)]
        public string Notes { get; set; } = string.Empty;

        // 🔹 Validation: ReturnedDate >= AssignedDate
        public static ValidationResult? ValidateReturnedDate(DateTime? returnedDate, ValidationContext context)
        {
            var instance = context.ObjectInstance as AssetHistory;
            if (returnedDate.HasValue && instance != null && returnedDate.Value < instance.AssignedDate)
                return new ValidationResult("Returned date cannot be earlier than assigned date.");
            return ValidationResult.Success;
        }
    }
}
