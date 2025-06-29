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
        ClockSkew = TimeSpan.Zero
    };

    // Configure to not require authentication by default
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("JWT Token validated successfully");
            logger.LogInformation("User: {User}", context.Principal?.Identity?.Name);
            foreach (var claim in context.Principal?.Claims ?? Enumerable.Empty<System.Security.Claims.Claim>())
            {
                logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("JWT Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Skip the challenge for requests without authorization header
            if (string.IsNullOrEmpty(context.Request.Headers.Authorization))
            {
                context.HandleResponse();
            }
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
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

// Explicitly handle OPTIONS requests for CORS as a fallback
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        var origin = context.Request.Headers.Origin.ToString();
        if (allowedOrigins.Any(allowed => origin.StartsWith(allowed) || allowed.StartsWith("*")))
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
            context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            context.Response.StatusCode = 204;
            await context.Response.CompleteAsync();
            return;
        }
    }
    await next();
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
