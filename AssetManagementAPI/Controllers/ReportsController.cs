using AssetManagementAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AssetManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // optional — if reports should be behind login
    public class ReportsController : ControllerBase
    {
        private readonly DapperRepository _repo;

        public ReportsController(DapperRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("assets-by-status")]
        public async Task<IActionResult> GetAssetsByStatus()
        {
            var data = await _repo.GetAssetsByStatusAsync();
            return Ok(data);
        }

        [HttpGet("expiring-assets")]
        public async Task<IActionResult> GetExpiringAssets()
        {
            var data = await _repo.GetExpiringAssetsAsync();
            return Ok(data);
        }

        [HttpGet("employee-utilization")]
        public async Task<IActionResult> GetEmployeeUtilization()
        {
            var data = await _repo.GetEmployeeUtilizationAsync();
            return Ok(data);
        }
    }
}
