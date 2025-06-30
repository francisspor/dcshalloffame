using DCSHallOfFameApi.Services;

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

// Configure Authentication with a default scheme
builder.Services.AddAuthentication("Custom")
    .AddScheme<CustomAuthenticationSchemeOptions, CustomAuthenticationHandler>("Custom", options => { });

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAuthenticatedUser()
              .RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "admin");
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
            .WithExposedHeaders("Content-Disposition")
            .SetIsOriginAllowedToAllowWildcardSubdomains();
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
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-User-Email, X-User-Role");
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
app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization(); // Enable authorization for [AdminOnly] attributes
app.UseHttpsRedirection();

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
