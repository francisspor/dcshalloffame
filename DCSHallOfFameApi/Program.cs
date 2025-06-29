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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = null;
    options.DefaultChallengeScheme = null;
    options.DefaultScheme = null;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = "dcs-hall-of-fame",
        ValidateAudience = true,
        ValidAudience = "dcs-hall-of-fame-api",
        ClockSkew = TimeSpan.Zero,
        RequireSignedTokens = true,
        ValidateLifetime = true,
        RequireExpirationTime = true
    };

    // Configure to not require authentication by default
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("=== JWT Token validated successfully ===");
            logger.LogInformation("User: {User}", context.Principal?.Identity?.Name);
            logger.LogInformation("IsAuthenticated: {IsAuthenticated}", context.Principal?.Identity?.IsAuthenticated);
            logger.LogInformation("AuthenticationType: {AuthType}", context.Principal?.Identity?.AuthenticationType);

            logger.LogInformation("=== All Claims ===");
            foreach (var claim in context.Principal?.Claims ?? Enumerable.Empty<System.Security.Claims.Claim>())
            {
                logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }

            logger.LogInformation("=== Role Claims ===");
            var roleClaims = context.Principal?.Claims.Where(c => c.Type.Contains("role")).ToList();
            foreach (var claim in roleClaims ?? Enumerable.Empty<System.Security.Claims.Claim>())
            {
                logger.LogInformation("Role Claim: {Type} = {Value}", claim.Type, claim.Value);
            }

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("=== JWT Authentication failed ===");
            logger.LogError("Error: {Error}", context.Exception.Message);
            logger.LogError("Exception type: {ExceptionType}", context.Exception.GetType().Name);
            logger.LogError("Stack trace: {StackTrace}", context.Exception.StackTrace);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("=== JWT Challenge triggered ===");
            logger.LogWarning("Request path: {Path}", context.Request.Path);
            logger.LogWarning("Authorization header present: {HasAuth}", !string.IsNullOrEmpty(context.Request.Headers.Authorization));

            // Skip the challenge for requests without authorization header
            if (string.IsNullOrEmpty(context.Request.Headers.Authorization))
            {
                logger.LogInformation("Skipping challenge - no authorization header");
                context.HandleResponse();
            }
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("=== JWT Message received ===");
            logger.LogInformation("Request path: {Path}", context.Request.Path);
            logger.LogInformation("Authorization header: {AuthHeader}", context.Request.Headers.Authorization);
            return Task.CompletedTask;
        }
    };
});

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
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
