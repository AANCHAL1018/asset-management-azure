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
    options.Cookie.SameSite = SameSiteMode.None;            // ✅ crucial for cross-site auth
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ✅ only HTTPS cookies
    options.Cookie.Name = ".AspNetCore.Identity.Application";
    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
});

// ==================================================
// CORS CONFIG (GLOBAL + PRODUCTION SAFE)
// ==================================================
var allowedOrigins = new[]
{
    "https://am-frontend-aanchal.azurewebsites.net",
    "http://localhost:5074",
    "https://localhost:5074"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins(allowedOrigins)
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

// ==================================================
// CONTROLLERS + JSON OPTIONS
// ==================================================
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.WriteIndented = true;
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
        Description = "Asset management backend API"
    });
});

var app = builder.Build();

// ==================================================
// MIDDLEWARE ORDER (Azure + Cookie Auth Safe)
// ==================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ the magic line — apply CORS before cookies & auth
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// ✅ ensure OPTIONS preflights don’t get blocked
app.MapControllers().RequireCors("AllowFrontend");

app.Run();