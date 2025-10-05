using System;

namespace AssetManagementWASM.Models
{
    public class AssetHistory
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.Now;
        public DateTime? ReturnedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
