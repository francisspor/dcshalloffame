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

        if (string.IsNullOrEmpty(userEmail) || userRole != "admin")
        {
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

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}