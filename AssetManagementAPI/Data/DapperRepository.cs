using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagementAPI.Data
{
    public class DapperRepository
    {
        private readonly string _connectionString;

        public DapperRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Example 1 — Quick count of assets by status
        public async Task<IEnumerable<dynamic>> GetAssetsByStatusAsync()
        {
            using var connection = new SqlConnection(_connectionString);
                    string sql = @"SELECT Id, Name, SerialNumber, Condition, Status, WarrantyExpiryDate
                   FROM Assets";
                    return await connection.QueryAsync(sql);
        }

        // Example 2 — Assets nearing warranty expiry (within 6 months)
        public async Task<IEnumerable<dynamic>> GetExpiringAssetsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT Id, Name, SerialNumber, WarrantyExpiryDate
                           FROM Assets
                           WHERE DATEDIFF(day, GETDATE(), WarrantyExpiryDate) <= 180";
            return await connection.QueryAsync(sql);
        }

        // Example 3 — Employee utilization stats
        public async Task<IEnumerable<dynamic>> GetEmployeeUtilizationAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT e.FullName, COUNT(h.Id) AS AssignedAssets
                FROM Employees e
                LEFT JOIN AssetHistories h ON e.Id = h.EmployeeId AND h.ReturnedDate IS NULL
                GROUP BY e.FullName
                ORDER BY AssignedAssets DESC";
            return await connection.QueryAsync(sql);
        }
    }
}
