using AssetManagementAPI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ==================================================
// DATABASE + IDENTITY
// ==================================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DapperRepository>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ==================================================
// COOKIE AUTH
// ==================================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;            // ✅ Required for cross-site cookies
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ✅ HTTPS only
    options.Cookie.Name = ".AspNetCore.Identity.Application";
    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    
    // ✅ CRITICAL: Handle 401 responses properly for API
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// ==================================================
// CORS POLICY - SIMPLIFIED AND FIXED
// ==================================================
var allowedOrigins = new[]
{
    "https://am-frontend-aanchal.azurewebsites.net",
    "http://localhost:5074",
    "https://localhost:5074"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains(); // ✅ Additional flexibility
    });
});

// ==================================================
// CONTROLLERS + JSON SETTINGS
// ==================================================
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.WriteIndented = true;
    });

// ==================================================
// SWAGGER
// ==================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AssetManagementAPI",
        Version = "v1",
        Description = "API for managing assets, employees, and assignments."
    });
});

var app = builder.Build();

// ==================================================
// MIDDLEWARE PIPELINE - CORRECT ORDER
// ==================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ CORS MUST COME BEFORE Authentication/Authorization
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ==================================================
// SEED ADMIN + SAMPLE DATA
// ==================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var cfg = services.GetRequiredService<IConfiguration>();
        var context = services.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        var adminUser = cfg["AdminCredentials:Username"] ?? "admin";
        var adminPass = cfg["AdminCredentials:Password"] ?? "admin@123";
        var adminRole = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRole))
            await roleManager.CreateAsync(new IdentityRole(adminRole));

        var existing = await userManager.FindByNameAsync(adminUser);
        if (existing == null)
        {
            var user = new IdentityUser { UserName = adminUser, Email = $"{adminUser}@local" };
            var result = await userManager.CreateAsync(user, adminPass);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, adminRole);
                logger.LogInformation("✅ Admin user seeded successfully");
            }
            else
            {
                logger.LogError("❌ Failed to create admin user: {errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("ℹ️ Admin user already exists.");
        }

        await DbSeeder.SeedSampleDataAsync(context, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during seeding process");
    }
}

app.Run();