#!/bin/bash

# Deploy Firestore Security Rules
# This script deploys the firestore.rules file to your Firebase project

# Configuration
PROJECT_ID="dcshalloffame"  # Update this to match your Firebase project ID
RULES_FILE="firestore.rules"

echo "🚀 Deploying Firestore Security Rules to project: $PROJECT_ID"

# Check if firebase CLI is installed
if ! command -v firebase &> /dev/null; then
    echo "❌ Firebase CLI is not installed. Please install it first:"
    echo "npm install -g firebase-tools"
    exit 1
fi

# Check if user is logged in to Firebase
if ! firebase projects:list &> /dev/null; then
    echo "❌ Not logged in to Firebase. Please run:"
    echo "firebase login"
    exit 1
fi

# Check if rules file exists
if [ ! -f "$RULES_FILE" ]; then
    echo "❌ Rules file not found: $RULES_FILE"
    exit 1
fi

# Deploy the rules
echo "📋 Deploying rules from $RULES_FILE..."
firebase deploy --only firestore:rules --project "$PROJECT_ID"

if [ $? -eq 0 ]; then
    echo "✅ Firestore security rules deployed successfully!"
    echo "🔒 Your database is now protected with proper security rules."
    echo ""
    echo "📝 Rules Summary:"
    echo "   - Public read access to hallOfFameMembers collection"
    echo "   - Write access restricted to authenticated service accounts"
    echo "   - All other collections are locked down"
else
    echo "❌ Failed to deploy Firestore security rules"
    exit 1
fi