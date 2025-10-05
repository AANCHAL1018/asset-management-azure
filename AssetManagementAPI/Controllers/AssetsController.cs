using AssetManagementAPI.Data;
using AssetManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AssetManagementAPI.Controllers
{
    // ---------------- ASSETS ----------------
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AssetsController(AppDbContext db)
        {
            _db = db;
        }

        // 🔹 Get all assets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Asset>>> GetAssets() =>
            await _db.Assets.Include(a => a.Employee).ToListAsync();

        // 🔹 Get asset by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Asset>> GetAsset(int id)
        {
            var asset = await _db.Assets.Include(a => a.Employee).FirstOrDefaultAsync(a => a.Id == id);
            if (asset == null)
                return NotFound(new { message = "Asset not found." });
            return asset;
        }

        // 🔹 Create new asset
        [HttpPost]
        public async Task<ActionResult<Asset>> CreateAsset([FromBody] Asset asset)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _db.Assets.AnyAsync(a => a.SerialNumber == asset.SerialNumber))
                return Conflict(new { message = $"Asset with Serial Number '{asset.SerialNumber}' already exists." });

            if (asset.WarrantyExpiryDate < asset.PurchaseDate)
                return BadRequest(new { message = "Warranty expiry date cannot be earlier than purchase date." });

            // 🔁 Auto-set status based on condition
            asset.Status = GetStatusFromCondition(asset.Condition);

            _db.Assets.Add(asset);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, asset);
        }

        // 🔹 Update asset
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsset(int id, [FromBody] Asset asset)
        {
            if (id != asset.Id)
                return BadRequest(new { message = "Asset ID mismatch." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _db.Assets.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Asset not found." });

            if (await _db.Assets.AnyAsync(a => a.SerialNumber == asset.SerialNumber && a.Id != id))
                return Conflict(new { message = $"Another asset with Serial Number '{asset.SerialNumber}' already exists." });

            if (asset.WarrantyExpiryDate < asset.PurchaseDate)
                return BadRequest(new { message = "Warranty expiry date cannot be earlier than purchase date." });

            // 🔁 Sync condition & status logic
            asset.Status = SyncConditionAndStatus(asset.Condition, existing.Status);

            _db.Entry(existing).CurrentValues.SetValues(asset);
            await _db.SaveChangesAsync();

            return Ok(new { message = "✅ Asset updated successfully." });
        }

        // 🔹 Delete asset
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            var asset = await _db.Assets.FindAsync(id);
            if (asset == null)
                return NotFound(new { message = "Asset not found." });

            if (asset.Status == "Assigned")
                return BadRequest(new { message = "Cannot delete asset while it is assigned." });

            _db.Assets.Remove(asset);
            await _db.SaveChangesAsync();

            return Ok(new { message = "🗑️ Asset deleted successfully." });
        }

        // 🔹 Helper Methods
        private static string GetStatusFromCondition(string condition) =>
            condition switch
            {
                "New" or "Good" => "Available",
                "Needs Repair" => "Under Repair",
                "Damaged" => "Retired",
                _ => "Available"
            };

        private static string SyncConditionAndStatus(string condition, string currentStatus)
        {
            return condition switch
            {
                "New" or "Good" => currentStatus == "Assigned" ? "Assigned" : "Available",
                "Needs Repair" => "Under Repair",
                "Damaged" => "Retired",
                _ => currentStatus
            };
        }
    }

    // ---------------- EMPLOYEES ----------------
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EmployeesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees() =>
            await _db.Employees.Include(e => e.Assets).ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _db.Employees.Include(e => e.Assets)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return NotFound(new { message = "Employee not found." });

            return employee;
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> CreateEmployee([FromBody] Employee employee)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _db.Employees.AnyAsync(e => e.Email == employee.Email))
                return Conflict(new { message = $"Employee with email '{employee.Email}' already exists." });

            if (!string.IsNullOrWhiteSpace(employee.PhoneNumber) &&
                await _db.Employees.AnyAsync(e => e.PhoneNumber == employee.PhoneNumber))
                return Conflict(new { message = $"Employee with phone '{employee.PhoneNumber}' already exists." });

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee employee)
        {
            if (id != employee.Id)
                return BadRequest(new { message = "Employee ID mismatch." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _db.Employees.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Employee not found." });

            if (await _db.Employees.AnyAsync(e => e.Email == employee.Email && e.Id != id))
                return Conflict(new { message = $"Another employee with email '{employee.Email}' already exists." });

            if (!string.IsNullOrWhiteSpace(employee.PhoneNumber) &&
                await _db.Employees.AnyAsync(e => e.PhoneNumber == employee.PhoneNumber && e.Id != id))
                return Conflict(new { message = $"Another employee with phone '{employee.PhoneNumber}' already exists." });

            _db.Entry(existing).CurrentValues.SetValues(employee);
            await _db.SaveChangesAsync();

            return Ok(new { message = "✅ Employee updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _db.Employees.Include(e => e.Assets).FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
                return NotFound(new { message = "Employee not found." });

            if (employee.Assets.Any(a => a.Status == "Assigned"))
                return BadRequest(new { message = "Cannot delete employee with assigned assets." });

            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();

            return Ok(new { message = "🗑️ Employee deleted successfully." });
        }
    }

    // ---------------- ASSET HISTORIES ----------------
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AssetHistoriesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AssetHistoriesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssetHistory>>> GetHistories() =>
            await _db.AssetHistories.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<AssetHistory>> GetHistory(int id)
        {
            var history = await _db.AssetHistories.FindAsync(id);
            if (history == null)
                return NotFound(new { message = "Asset history not found." });
            return history;
        }

        // 🔹 Assign Asset
        [HttpPost]
        public async Task<ActionResult<AssetHistory>> CreateHistory([FromBody] AssetHistory history)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var asset = await _db.Assets.FindAsync(history.AssetId);
            var employee = await _db.Employees.FindAsync(history.EmployeeId);

            if (asset == null)
                return BadRequest(new { message = $"Invalid Asset ID: {history.AssetId}" });
            if (employee == null)
                return BadRequest(new { message = $"Invalid Employee ID: {history.EmployeeId}" });

            if (history.ReturnedDate.HasValue && history.ReturnedDate < history.AssignedDate)
                return BadRequest(new { message = "Returned date cannot be earlier than assigned date." });

            // 🔁 Mark Asset as Assigned (if valid)
            if (asset.Condition == "New" || asset.Condition == "Good")
                asset.Status = "Assigned";

            asset.EmployeeId = employee.Id;

            _db.AssetHistories.Add(history);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHistory), new { id = history.Id }, history);
        }

        // 🔹 Update Asset History (e.g., Return Asset)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHistory(int id, [FromBody] AssetHistory history)
        {
            if (id != history.Id)
                return BadRequest(new { message = "History ID mismatch." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _db.AssetHistories.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Asset history not found." });

            var asset = await _db.Assets.FindAsync(history.AssetId);
            if (asset == null)
                return BadRequest(new { message = "Linked asset not found." });

            if (history.ReturnedDate.HasValue && history.ReturnedDate < history.AssignedDate)
                return BadRequest(new { message = "Returned date cannot be earlier than assigned date." });

            _db.Entry(existing).CurrentValues.SetValues(history);

            // 🔁 Mark Asset Available on Return
            if (history.ReturnedDate.HasValue)
            {
                asset.Status = (asset.Condition == "New" || asset.Condition == "Good") ? "Available" :
                               asset.Condition == "Needs Repair" ? "Under Repair" : "Retired";
                asset.EmployeeId = null;
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "✅ Asset history updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHistory(int id)
        {
            var history = await _db.AssetHistories.FindAsync(id);
            if (history == null)
                return NotFound(new { message = "Asset history not found." });

            _db.AssetHistories.Remove(history);
            await _db.SaveChangesAsync();

            return Ok(new { message = "🗑️ Asset history deleted successfully." });
        }
    }
}
