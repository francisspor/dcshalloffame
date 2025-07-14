using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace DCSHallOfFameApi.Services;

public class CustomAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
}

public class CustomAuthenticationHandler : AuthenticationHandler<CustomAuthenticationSchemeOptions>
{
    public CustomAuthenticationHandler(
        IOptionsMonitor<CustomAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userEmail = Context.Request.Headers["X-User-Email"].ToString();
        var userRole = Context.Request.Headers["X-User-Role"].ToString();

        // Log authentication attempt for debugging
        Logger.LogInformation("Authentication attempt - Email: {Email}, Role: {Role}",
            userEmail, userRole);

        // Check if we have the required headers
        if (string.IsNullOrEmpty(userEmail))
        {
            Logger.LogWarning("Authentication failed - Missing X-User-Email header");
            return Task.FromResult(AuthenticateResult.Fail("Missing X-User-Email header"));
        }

        // More flexible role checking - allow if role is admin or if user has duanesburg.org email
        var isAdmin = !string.IsNullOrEmpty(userRole) && userRole == "admin";
        var hasValidEmail = userEmail.EndsWith("@duanesburg.org", StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && !hasValidEmail)
        {
            Logger.LogWarning("Authentication failed - Invalid role or email domain. Email: {Email}, Role: {Role}",
                userEmail, userRole);
            return Task.FromResult(AuthenticateResult.Fail("Invalid authentication headers"));
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userEmail),
            new Claim(ClaimTypes.Email, userEmail),
            new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "admin")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        Logger.LogInformation("Authentication successful for user: {Email}", userEmail);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}