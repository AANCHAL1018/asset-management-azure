using AssetManagementAPI.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagementAPI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedSampleDataAsync(AppDbContext context, ILogger logger)
        {
            try
            {
                // ✅ Employees
                if (!context.Employees.Any())
                {
                    context.Employees.AddRange(
                        new Employee { FullName = "John Doe", Department = "IT", Designation = "System Admin", Email = "john.doe@company.com", PhoneNumber = "9876543210", Status = "Active", CreatedDate = DateTime.Now },
                        new Employee { FullName = "Jane Smith", Department = "Finance", Designation = "Accountant", Email = "jane.smith@company.com", PhoneNumber = "9988776655", Status = "Active", CreatedDate = DateTime.Now },
                        new Employee { FullName = "David Miller", Department = "Operations", Designation = "Coordinator", Email = "david.miller@company.com", PhoneNumber = "9123456780", Status = "Inactive", CreatedDate = DateTime.Now }
                    );
                    await context.SaveChangesAsync();
                    logger.LogInformation("✅ Seeded Employees");
                }

                // ✅ Assets
                if (!context.Assets.Any())
                {
                    context.Assets.AddRange(
                        new Asset
                        {
                            Name = "Dell Latitude 5420",
                            Type = "Laptop",
                            MakeModel = "Dell 5420 i7",
                            SerialNumber = "DL-2023-001",
                            Condition = "Good",
                            Status = "Available",
                            IsSpare = false,
                            WarrantyExpiryDate = DateTime.Now.AddYears(2),
                            CreatedDate = DateTime.Now
                        },
                        new Asset
                        {
                            Name = "HP LaserJet Pro MFP",
                            Type = "Printer",
                            MakeModel = "HP M404dw",
                            SerialNumber = "HP-404-PRN",
                            Condition = "New",
                            Status = "Assigned",
                            IsSpare = false,
                            WarrantyExpiryDate = DateTime.Now.AddYears(1),
                            CreatedDate = DateTime.Now
                        },
                        new Asset
                        {
                            Name = "MacBook Air M2",
                            Type = "Laptop",
                            MakeModel = "Apple M2 Air",
                            SerialNumber = "MB-M2-0001",
                            Condition = "New",
                            Status = "Available",
                            IsSpare = true,
                            WarrantyExpiryDate = DateTime.Now.AddYears(3),
                            CreatedDate = DateTime.Now
                        }
                    );
                    await context.SaveChangesAsync();
                    logger.LogInformation("✅ Seeded Assets");
                }

                // ✅ AssetHistory
                if (!context.AssetHistories.Any())
                {
                    var asset = context.Assets.FirstOrDefault(a => a.Status == "Assigned");
                    var employee = context.Employees.FirstOrDefault(e => e.Status == "Active");

                    if (asset != null && employee != null)
                    {
                        context.AssetHistories.Add(new AssetHistory
                        {
                            AssetId = asset.Id,
                            EmployeeId = employee.Id,
                            AssignedDate = DateTime.Now.AddDays(-15),
                            Notes = "Assigned during onboarding"
                        });

                        asset.Status = "Assigned";
                        await context.SaveChangesAsync();
                        logger.LogInformation("✅ Seeded AssetHistory");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during sample data seeding");
            }
        }
    }
}
