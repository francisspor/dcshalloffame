# Admin Interface Setup Guide

## Environment Variables

Create a `.env.local` file in the `dcs-hall-of-fame-frontend` directory with the following variables:

```env
# NextAuth Configuration
NEXTAUTH_URL=http://localhost:3001
NEXTAUTH_SECRET=your-nextauth-secret-key-here

# Google OAuth Configuration
GOOGLE_CLIENT_ID=your-google-client-id-here
GOOGLE_CLIENT_SECRET=your-google-client-secret-here

# API Configuration
NEXT_PUBLIC_API_URL=http://localhost:5232/api/v1
```

## Google OAuth Setup

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google+ API and Google Admin SDK API
4. Go to "Credentials" and create an OAuth 2.0 Client ID
5. Set the authorized redirect URI to: `http://localhost:3001/api/auth/callback/google`
6. Copy the Client ID and Client Secret to your `.env.local` file

## Google Admin SDK Setup

To check group membership, you'll need to:

1. Enable the Google Admin SDK API in your Google Cloud project
2. Create a service account with domain-wide delegation
3. Grant the service account access to read group membership
4. Configure the OAuth consent screen to include the necessary scopes

## NextAuth Secret

Generate a secure random string for NEXTAUTH_SECRET:

```bash
openssl rand -base64 32
```

## Access Control

The admin interface is restricted to members of the `duanesburgboe@duanesburg.org` Google group. Users must:

1. Sign in with their Duanesburg Google account
2. Be a member of the duanesburgboe group
3. Have the necessary permissions to access the admin panel

## Features

- **Authentication**: Google OAuth with group-based authorization
- **Member Management**: View, add, edit, and delete Hall of Fame members
- **Real-time Data**: Connected to the .NET API backend
- **Secure Access**: Protected routes with session management
- **User-friendly Interface**: Modern admin dashboard with tabbed navigation

## Usage

1. Navigate to `/admin` to access the admin panel
2. Sign in with your Duanesburg Google account
3. Use the tabs to manage different member categories
4. Add new members or edit existing ones
5. View member profiles on the public site

## Security Notes

- All admin routes are protected by authentication
- Group membership is verified on each request
- Sessions are managed securely with JWT tokens
- API calls are authenticated and authorized