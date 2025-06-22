# Security Setup Guide

## Overview
The Hall of Fame API now includes comprehensive security measures to protect admin-only endpoints while keeping public read access available.

## Security Features Implemented

### 🔐 JWT Authentication
- **Token-based authentication** using JWT (JSON Web Tokens)
- **Automatic token generation** from NextAuth session
- **Token validation** with proper issuer and audience claims
- **1-hour token expiration** for security

### 🛡️ Authorization
- **Role-based access control** (Admin only for write operations)
- **Protected endpoints** for all CRUD operations
- **Public read access** for member data
- **Custom authorization attributes** for easy endpoint protection

### 🌐 CORS Protection
- **Restrictive CORS policy** allowing only frontend domains
- **Credential support** for authenticated requests
- **Configurable allowed origins** via appsettings.json

## Configuration Required

### 1. Environment Variables

#### Frontend (.env.local)
```bash
# JWT Secret (MUST match API secret)
JWT_SECRET=your-super-secret-jwt-key-here-make-it-long-and-random-in-production

# API URL
NEXT_PUBLIC_API_URL=http://localhost:5232/api/v1
```

#### API (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-jwt-key-here-make-it-long-and-random-in-production"
  },
  "AllowedOrigins": [
    "http://localhost:3000",
    "https://localhost:3000"
  ]
}
```

### 2. Production Security Checklist

- [ ] **Change JWT Secret**: Use a strong, random secret key
- [ ] **Enable HTTPS**: Set `RequireHttpsMetadata = true` in production
- [ ] **Update CORS Origins**: Add your production domain
- [ ] **Secure Headers**: Add security headers to API responses
- [ ] **Rate Limiting**: Implement rate limiting for API endpoints
- [ ] **Logging**: Enable security event logging

## Protected Endpoints

### 🔒 Admin Only (Requires Authentication)
- `POST /api/v1/halloffame` - Create new member
- `PUT /api/v1/halloffame/{id}` - Update member
- `DELETE /api/v1/halloffame/{id}` - Delete member
- `POST /api/v1/halloffame/cache/clear` - Clear all caches
- `POST /api/v1/halloffame/cache/clear/category/{category}` - Clear category cache
- `POST /api/v1/halloffame/cache/clear/member/{id}` - Clear member cache

### 🌍 Public Access (No Authentication Required)
- `GET /api/v1/halloffame` - Get all members
- `GET /api/v1/halloffame/category/{category}` - Get members by category
- `GET /api/v1/halloffame/{id}` - Get member by ID

## How It Works

### 1. Authentication Flow
1. User logs in via NextAuth (Google OAuth)
2. Frontend generates JWT token using user session
3. JWT token includes user email and admin role
4. Token sent with API requests in Authorization header

### 2. Authorization Flow
1. API validates JWT token signature and claims
2. Checks for "admin" role claim
3. Allows/denies access based on role
4. Returns appropriate HTTP status codes

### 3. Error Handling
- **401 Unauthorized**: Invalid or missing token
- **403 Forbidden**: Valid token but insufficient permissions
- **500 Internal Server Error**: Server-side errors

## Testing Security

### Test Unauthorized Access
```bash
# Should return 401 Unauthorized
curl -X POST http://localhost:5232/api/v1/halloffame \
  -H "Content-Type: application/json" \
  -d '{"name":"Test User"}'
```

### Test Authorized Access
```bash
# Should work with valid JWT token
curl -X POST http://localhost:5232/api/v1/halloffame \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{"name":"Test User"}'
```

## Security Best Practices

### 🔑 JWT Security
- Use strong, random secret keys
- Set appropriate token expiration
- Validate issuer and audience claims
- Store secrets securely (not in code)

### 🛡️ API Security
- Always validate input data
- Use HTTPS in production
- Implement rate limiting
- Log security events
- Regular security audits

### 🌐 CORS Security
- Only allow necessary origins
- Don't use wildcard (*) origins
- Validate credentials properly
- Review CORS policy regularly

## Troubleshooting

### Common Issues

1. **401 Unauthorized Errors**
   - Check JWT secret matches between frontend and API
   - Verify token is being sent in Authorization header
   - Check token expiration

2. **CORS Errors**
   - Verify frontend domain is in AllowedOrigins
   - Check credentials are enabled
   - Ensure proper CORS middleware order

3. **403 Forbidden Errors**
   - Verify user has admin role in JWT token
   - Check authorization policy configuration
   - Ensure user is properly authenticated

### Debug Mode
Enable debug logging in development:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore.Authentication": "Debug"
    }
  }
}
```

## Production Deployment

### Environment Variables
```bash
# Production JWT Secret (generate strong random key)
JWT_SECRET=your-production-jwt-secret-key-here

# Production API URL
NEXT_PUBLIC_API_URL=https://your-api-domain.com/api/v1
```

### Security Headers
Add security headers to API responses:
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

## Support

For security-related issues or questions, please review the logs and check the configuration settings above. The security implementation follows industry best practices for JWT authentication and authorization.