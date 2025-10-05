using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AssetManagementAPI.Models;

namespace AssetManagementAPI.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Asset> Assets { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<AssetHistory> AssetHistories { get; set; } = null!;
    }
}
