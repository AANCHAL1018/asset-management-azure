using System;

namespace AssetManagementWASM.Models
{
    public class Asset
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string MakeModel { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public DateTime WarrantyExpiryDate { get; set; } = DateTime.Now.AddYears(1);
        public string Condition { get; set; } = "New";
        public string Status { get; set; } = "Available";
        public bool IsSpare { get; set; } = false;
        public string Specifications { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int? EmployeeId { get; set; }
    }
}
