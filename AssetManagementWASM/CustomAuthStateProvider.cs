using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http.Json;
using System.Security.Claims;

/// <summary>
/// Provides authentication state management for the Blazor WASM app.
/// Communicates with backend API endpoints for login, logout, and session status.
/// Works with cookie-based authentication in both local and Azure environments.
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _http;

    public CustomAuthStateProvider(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Gets the current authentication state from the backend.
    /// Called automatically by Blazor when user state is required.
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/status");

            // ✅ Ensure the backend knows we want JSON, not HTML
            request.Headers.Add("Accept", "application/json");

            // ✅ Include cookies for ASP.NET Identity authentication
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var status = await response.Content.ReadFromJsonAsync<AuthStatus>();

                if (status?.IsAuthenticated == true)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, status.Username)
                    };

                    var identity = new ClaimsIdentity(claims, "CustomAuth");
                    var user = new ClaimsPrincipal(identity);

                    return new AuthenticationState(user);
                }
            }
            else
            {
                Console.WriteLine($"[Auth Status] Server returned {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthState Error] {ex.Message}");
        }

        // Default: Unauthenticated state
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    /// <summary>
    /// Attempts to log in using provided credentials.
    /// On success, triggers state update and returns true.
    /// </summary>
    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
            {
                Content = JsonContent.Create(new { username, password })
            };

            request.Headers.Add("Accept", "application/json");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // Notify Blazor components that authentication state has changed
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return true;
            }
            else
            {
                Console.WriteLine($"[Login Failed] {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Login Error] {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Logs out the current user and resets authentication state.
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");

            request.Headers.Add("Accept", "application/json");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            await _http.SendAsync(request);

            // Trigger authentication state change after logout
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Logout Error] {ex.Message}");
        }
    }
}

/// <summary>
/// DTO representing the authentication status returned by the API.
/// </summary>
public class AuthStatus
{
    public bool IsAuthenticated { get; set; }
    public string Username { get; set; } = string.Empty;
}