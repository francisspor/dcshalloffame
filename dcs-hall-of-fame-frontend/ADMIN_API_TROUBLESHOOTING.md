# Google Admin API Access Denied - Troubleshooting Guide

## The Problem

You're getting "access denied" when trying to sign in, even though the Google OAuth flow completes successfully. This happens in the `signIn` callback when checking if the user is in the `duanesburgboe@duanesburg.org` group.

## Debugging Steps

### 1. Check the Console Logs

The updated NextAuth configuration now includes detailed logging. Check your terminal/console for messages like:
- "SignIn callback triggered for: user@duanesburg.org"
- "Checking group membership for: user@duanesburg.org"
- "Group membership check response status: 403"

### 2. Common Error Status Codes

- **403 Forbidden**: Admin SDK API not enabled or insufficient permissions
- **404 Not Found**: User is not in the group (this is expected for non-members)
- **401 Unauthorized**: Invalid access token or expired token
- **400 Bad Request**: Malformed request

### 3. Test the Admin API Directly

Use the test endpoint to debug the API call:

```
GET /api/test-admin-api?email=your-email@duanesburg.org&token=your-access-token
```

To get your access token:
1. Sign in to your app
2. Open browser dev tools
3. Go to Application/Storage tab
4. Look for the session or token storage
5. Copy the access token

### 4. Google Cloud Console Setup

#### Enable Admin SDK API
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Select your project
3. Go to **APIs & Services** > **Library**
4. Search for "Admin SDK API"
5. Click on it and press **Enable**

#### Check OAuth Consent Screen
1. Go to **APIs & Services** > **OAuth consent screen**
2. Make sure these scopes are added:
   - `https://www.googleapis.com/auth/admin.directory.group.member.readonly`
   - `openid`
   - `email`
   - `profile`
3. Add your email as a test user

#### Verify Credentials
1. Go to **APIs & Services** > **Credentials**
2. Check that your OAuth 2.0 client ID has the correct redirect URIs
3. Make sure the client is configured for "Web application"

### 5. Google Workspace Setup

#### Create the Group (if it doesn't exist)
1. Go to [Google Workspace Admin Console](https://admin.google.com/)
2. Navigate to **Directory** > **Groups**
3. Create a new group called `duanesburgboe@duanesburg.org`
4. Add the admin users to this group

#### Check Group Permissions
1. In the group settings, make sure:
   - Group is visible to organization
   - Members can be viewed by organization
   - Admin SDK can access the group

### 6. Alternative Solutions

#### Option 1: Domain-Based Fallback
The updated code now includes a fallback that allows users with `@duanesburg.org` email addresses to sign in even if the Admin API fails. This is useful for development or if the Admin API setup is complex.

#### Option 2: Disable Group Check Temporarily
For testing, you can temporarily disable the group check by modifying the `signIn` callback:

```typescript
async signIn({ user, account, profile }) {
  // Temporarily allow all duanesburg.org users
  if (user.email && user.email.endsWith('@duanesburg.org')) {
    return true
  }
  return false
}
```

#### Option 3: Use Service Account (Advanced)
For production, consider using a service account with domain-wide delegation instead of user OAuth tokens.

### 7. Environment Variables Check

Make sure your `.env.local` has all required variables:

```env
GOOGLE_CLIENT_ID=your-client-id
GOOGLE_CLIENT_SECRET=your-client-secret
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=your-secret
```

### 8. Testing Checklist

- [ ] Admin SDK API is enabled in Google Cloud Console
- [ ] OAuth consent screen includes the Admin SDK scope
- [ ] Your email is added as a test user
- [ ] The `duanesburgboe@duanesburg.org` group exists
- [ ] Your email is a member of the group
- [ ] Your OAuth client has correct redirect URIs
- [ ] All environment variables are set correctly

### 9. Common Issues and Solutions

#### "API not enabled" error
- Enable the Admin SDK API in Google Cloud Console

#### "Insufficient permissions" error
- Make sure the OAuth consent screen includes the Admin SDK scope
- Verify the group exists and is accessible

#### "Group not found" error
- Create the `duanesburgboe@duanesburg.org` group in Google Workspace
- Make sure the group name is exactly correct

#### "User not in group" error
- Add the user's email to the `duanesburgboe@duanesburg.org` group
- Wait a few minutes for changes to propagate

### 10. Production Considerations

For production deployment:
1. Publish the OAuth consent screen
2. Use a service account for better reliability
3. Implement proper error handling and user feedback
4. Consider implementing a backup authentication method