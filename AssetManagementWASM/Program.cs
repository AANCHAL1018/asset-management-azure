using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using AssetManagementWASM;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --------------------------------------------------------
// 🧩 HTTP CLIENT CONFIGURATION
// --------------------------------------------------------
// In Development → Uses local backend (localhost:5299)
// In Production  → Uses same domain (Azure deployment)
// --------------------------------------------------------
builder.Services.AddScoped(sp =>
{
    var http = new HttpClient
    {
        BaseAddress = new Uri("https://am-api-aanchal.azurewebsites.net/") // ✅ your backend API
    };
    http.DefaultRequestHeaders.Add("Accept", "application/json");
    return http;
});

// --------------------------------------------------------
// 🔐 AUTHENTICATION STATE PROVIDER
// --------------------------------------------------------
// Handles JWT-based authentication and user state
// --------------------------------------------------------
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<CustomAuthStateProvider>());

// --------------------------------------------------------
// 🚀 BUILD AND RUN
// --------------------------------------------------------
await builder.Build().RunAsync();
