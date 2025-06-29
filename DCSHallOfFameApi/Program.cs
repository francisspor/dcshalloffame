using DCSHallOfFameApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add memory cache
builder.Services.AddMemoryCache();

// Configure Firebase service
builder.Services.AddSingleton<IFirebaseService, FirebaseService>();

// Configure cache service
builder.Services.AddSingleton<ICacheService, CacheService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var key = Encoding.ASCII.GetBytes(secretKey);

// Debug logging for JWT secret
var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
logger.LogInformation("=== JWT Configuration ===");
logger.LogInformation("SecretKey length: {Length}", secretKey?.Length ?? 0);
logger.LogInformation("SecretKey starts with: {Start}", secretKey?.Substring(0, Math.Min(10, secretKey?.Length ?? 0)) + "...");
logger.LogInformation("Key length: {KeyLength}", key?.Length ?? 0);

// Note: JWT authentication disabled - using custom header-based authentication

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAuthenticatedUser()
              .RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "admin");

        // Add debugging to the policy
        policy.RequireAssertion(context =>
        {
            var logger = context.Resource as ILogger<Program>;

            if (logger != null)
            {
                logger.LogInformation("Checking AdminOnly policy");
                logger.LogInformation("User authenticated: {IsAuthenticated}", context.User.Identity?.IsAuthenticated);
                logger.LogInformation("User name: {Name}", context.User.Identity?.Name);

                var hasRoleClaim = context.User.HasClaim(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && c.Value == "admin");
                logger.LogInformation("Has role claim 'admin': {HasRole}", hasRoleClaim);

                foreach (var claim in context.User.Claims)
                {
                    logger.LogInformation("Policy check - Claim: {Type} = {Value}", claim.Type, claim.Value);
                }
            }

            return context.User.Identity?.IsAuthenticated == true &&
                   context.User.HasClaim(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && c.Value == "admin");
        });
    });
});

// Configure CORS - More restrictive for production
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? new[] {
        "http://localhost:3000",
        "https://localhost:3000",
        "https://dcshalloffame.vercel.app"
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policyBuilder =>
    {
        policyBuilder
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .WithExposedHeaders("Content-Disposition");
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// Explicitly handle OPTIONS requests for CORS as a fallback
app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].ToString();
    if (!string.IsNullOrEmpty(origin))
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
        context.Response.Headers.Add("Vary", "Origin");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
    }

    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 204;
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});

app.UseCors("AllowFrontend");
// app.UseAuthentication(); // Disabled - using custom authentication
// app.UseAuthorization(); // Disabled - using custom authentication
app.UseHttpsRedirection();

// Simple authentication middleware for admin requests
app.Use(async (context, next) =>
{
    var userEmail = context.Request.Headers["X-User-Email"].ToString();
    var userRole = context.Request.Headers["X-User-Role"].ToString();

    if (!string.IsNullOrEmpty(userEmail) && userRole == "admin")
    {
        // Create a simple claims principal for admin users
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, userEmail),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, userEmail),
            new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "admin")
        };

        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Custom");
        context.User = new System.Security.Claims.ClaimsPrincipal(identity);

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("=== Custom Authentication ===");
        logger.LogInformation("User authenticated: {User}", userEmail);
        logger.LogInformation("Role: {Role}", userRole);
    }

    await next();
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
