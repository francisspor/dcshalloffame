# Google OAuth Setup Guide

This guide will help you set up Google OAuth for the DCS Hall of Fame admin interface.

## Step 1: Create a Google Cloud Project

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google+ API and Admin SDK API

## Step 2: Configure OAuth Consent Screen

1. In the Google Cloud Console, go to **APIs & Services** > **OAuth consent screen**
2. Choose **External** user type (unless you have a Google Workspace organization)
3. Fill in the required information:
   - **App name**: DCS Hall of Fame Admin
   - **User support email**: Your email address
   - **Developer contact information**: Your email address
4. Add the following scopes:
   - `openid`
   - `email`
   - `profile`
   - `https://www.googleapis.com/auth/admin.directory.group.member.readonly`
5. Add test users (your email and any other admin emails)
6. Save and continue

## Step 3: Create OAuth 2.0 Credentials

1. Go to **APIs & Services** > **Credentials**
2. Click **Create Credentials** > **OAuth 2.0 Client IDs**
3. Choose **Web application**
4. Set the following:
   - **Name**: DCS Hall of Fame Frontend
   - **Authorized JavaScript origins**:
     - `http://localhost:3000` (for development)
     - `https://yourdomain.com` (for production)
   - **Authorized redirect URIs**:
     - `http://localhost:3000/api/auth/callback/google` (for development)
     - `https://yourdomain.com/api/auth/callback/google` (for production)
5. Click **Create**
6. **Save the Client ID and Client Secret** - you'll need these for the environment variables

## Step 4: Set Up Environment Variables

Create a `.env.local` file in the `dcs-hall-of-fame-frontend` directory with the following content:

```env
# Google OAuth Configuration
GOOGLE_CLIENT_ID=your_google_client_id_here
GOOGLE_CLIENT_SECRET=your_google_client_secret_here

# NextAuth Configuration
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=your_nextauth_secret_here

# API Configuration
NEXT_PUBLIC_API_BASE_URL=http://localhost:5232/api/v1
```

### Generate NEXTAUTH_SECRET

You can generate a secure secret using this command:
```bash
openssl rand -base64 32
```

Or use an online generator and copy the result.

## Step 5: Configure Google Workspace Admin API

Since the app checks group membership, you need to set up the Admin SDK:

1. In Google Cloud Console, go to **APIs & Services** > **Library**
2. Search for "Admin SDK API" and enable it
3. Go to **APIs & Services** > **OAuth consent screen**
4. Add the scope: `https://www.googleapis.com/auth/admin.directory.group.member.readonly`
5. Make sure your test users are added to the consent screen

## Step 6: Set Up Google Workspace Group

1. In your Google Workspace admin console, go to **Directory** > **Groups**
2. Create a group called `duanesburgboe@duanesburg.org` (or use existing)
3. Add the admin users to this group
4. Make sure the group is accessible via the Admin SDK

## Step 7: Test the Setup

1. Start your development server:
   ```bash
   npm run dev
   ```

2. Navigate to `http://localhost:3000/admin/login`
3. Try signing in with a Google account that's in the `duanesburgboe` group
4. You should be redirected to the admin dashboard

## Troubleshooting

### Common Issues

1. **"Error: redirect_uri_mismatch"**
   - Make sure the redirect URI in Google Cloud Console matches exactly
   - Check for trailing slashes or protocol mismatches

2. **"Access denied" after Google sign-in**
   - Verify the user is in the `duanesburgboe@duanesburg.org` group
   - Check that the Admin SDK API is enabled
   - Ensure the correct scopes are requested

3. **"Invalid client" error**
   - Double-check the Client ID and Client Secret
   - Make sure they're copied correctly without extra spaces

4. **Group membership check fails**
   - Verify the group exists and is accessible
   - Check that the user has the necessary permissions
   - Ensure the Admin SDK scope is included

### Environment Variables Checklist

- [ ] `GOOGLE_CLIENT_ID` - From Google Cloud Console
- [ ] `GOOGLE_CLIENT_SECRET` - From Google Cloud Console
- [ ] `NEXTAUTH_URL` - Your application URL
- [ ] `NEXTAUTH_SECRET` - Generated secure secret
- [ ] `NEXT_PUBLIC_API_BASE_URL` - Your .NET API URL

### Security Notes

1. **Never commit `.env.local` to version control**
2. **Use different credentials for development and production**
3. **Regularly rotate your client secrets**
4. **Monitor OAuth usage in Google Cloud Console**

## Production Deployment

For production deployment:

1. Update the OAuth consent screen to **Published** status
2. Add your production domain to authorized origins and redirect URIs
3. Update environment variables with production URLs
4. Use a strong, unique `NEXTAUTH_SECRET`
5. Consider using environment-specific `.env` files

## Additional Resources

- [NextAuth.js Documentation](https://next-auth.js.org/)
- [Google OAuth 2.0 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [Google Admin SDK Documentation](https://developers.google.com/admin-sdk)