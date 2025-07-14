# Firestore Security Rules Setup

## Overview

This guide helps you set up proper Firestore security rules for your DCS Hall of Fame application. The rules ensure that your database is secure while allowing your application to function properly.

## Security Rules Explanation

The `firestore.rules` file contains the following security configuration:

### Public Read Access
- **All users** can read from the `hallOfFameMembers` collection
- This allows your public website to display Hall of Fame members without authentication

### Restricted Write Access
- **Only authenticated service accounts** can create, update, or delete members
- Your .NET API uses service account credentials to perform all database operations
- This prevents unauthorized users from modifying the data

### Default Deny
- All other collections are locked down by default
- This follows the principle of least privilege

## Current Architecture

Your application uses this security model:

```
Public Website (Frontend) → .NET API → Firestore Database
     ↓
   Read-only access    Admin operations    Service account auth
```

1. **Public Website**: Can read member data (no authentication required)
2. **Admin Interface**: Authenticated via Google OAuth, communicates through .NET API
3. **.NET API**: Uses service account credentials to access Firestore
4. **Firestore**: Protected by security rules

## Deployment Instructions

### Prerequisites

1. **Install Firebase CLI**:
   ```bash
   npm install -g firebase-tools
   ```

2. **Login to Firebase**:
   ```bash
   firebase login
   ```

3. **Verify your project ID**:
   - Open `firestore.rules` and check the project ID in the deployment scripts
   - Update `PROJECT_ID` in the scripts if needed

### Deploy Security Rules

#### Option 1: Using the deployment script (Recommended)

**For Windows:**
```powershell
.\deploy-firestore-rules.ps1
```

**For macOS/Linux:**
```bash
chmod +x deploy-firestore-rules.sh
./deploy-firestore-rules.sh
```

#### Option 2: Manual deployment

```bash
firebase deploy --only firestore:rules --project dcshalloffame
```

### Verify Deployment

After deployment, you can verify the rules are active by:

1. **Check Firebase Console**:
   - Go to [Firebase Console](https://console.firebase.google.com/)
   - Select your project
   - Navigate to Firestore Database → Rules
   - Verify the rules are deployed

2. **Test your application**:
   - Ensure the public website can still read member data
   - Ensure admin operations work through the API
   - Try accessing Firestore directly (should be denied)

## Security Rules Details

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    // Hall of Fame Members Collection
    match /hallOfFameMembers/{documentId} {
      // Allow public read access to all members
      allow read: if true;

      // Allow write operations only to authenticated service accounts
      // Your .NET API uses service account credentials for all database operations
      allow create, update, delete: if request.auth != null;
    }

    // Deny access to all other collections by default
    match /{document=**} {
      allow read, write: if false;
    }
  }
}
```

### What Each Rule Does

1. **`allow read: if true`**: Anyone can read member data (public access)
2. **`allow create, update, delete: if request.auth != null`**: Only authenticated service accounts can modify data
3. **`allow read, write: if false`**: All other collections are completely locked down

## Troubleshooting

### Common Issues

1. **"Permission denied" errors**:
   - Ensure your .NET API is using the correct service account credentials
   - Check that the service account has the necessary permissions

2. **Public website can't read data**:
   - Verify the `allow read: if true` rule is deployed
   - Check that your frontend is making requests to the correct collection

3. **Admin operations failing**:
   - Ensure your .NET API is authenticated with service account
   - Check that the API is using the correct Firebase project

### Testing Security Rules

You can test your rules using the Firebase Console:

1. Go to Firestore Database → Rules
2. Click "Test rules" tab
3. Create test cases to verify your rules work as expected

## Security Best Practices

1. **Regular Reviews**: Review security rules periodically
2. **Principle of Least Privilege**: Only grant necessary permissions
3. **Monitor Access**: Use Firebase Console to monitor database access
4. **Backup Rules**: Keep a backup of your security rules
5. **Test Changes**: Always test rule changes in a development environment first

## Next Steps

After deploying these rules:

1. **Test your application** thoroughly
2. **Monitor for any issues** in the Firebase Console
3. **Consider additional security measures** like:
   - Rate limiting
   - Data validation rules
   - Audit logging

## Support

If you encounter issues:

1. Check the Firebase Console for error messages
2. Review the Firebase documentation on security rules
3. Test with simplified rules first, then add complexity
4. Ensure your service account has the correct permissions

## Emergency Access

If you need to temporarily disable security rules (not recommended):

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /{document=**} {
      allow read, write: if true;
    }
  }
}
```

⚠️ **Warning**: This removes all security - only use for emergency debugging and revert immediately.