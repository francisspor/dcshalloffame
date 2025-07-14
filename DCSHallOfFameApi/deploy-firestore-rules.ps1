# Deploy Firestore Security Rules
# This script deploys the firestore.rules file to your Firebase project

# Configuration
$PROJECT_ID = "dcshalloffame"  # Update this to match your Firebase project ID
$RULES_FILE = "firestore.rules"

Write-Host "🚀 Deploying Firestore Security Rules to project: $PROJECT_ID" -ForegroundColor Green

# Check if firebase CLI is installed
try {
  $null = Get-Command firebase -ErrorAction Stop
}
catch {
  Write-Host "❌ Firebase CLI is not installed. Please install it first:" -ForegroundColor Red
  Write-Host "npm install -g firebase-tools" -ForegroundColor Yellow
  exit 1
}

# Check if user is logged in to Firebase
try {
  firebase projects:list | Out-Null
}
catch {
  Write-Host "❌ Not logged in to Firebase. Please run:" -ForegroundColor Red
  Write-Host "firebase login" -ForegroundColor Yellow
  exit 1
}

# Check if rules file exists
if (-not (Test-Path $RULES_FILE)) {
  Write-Host "❌ Rules file not found: $RULES_FILE" -ForegroundColor Red
  exit 1
}

# Deploy the rules
Write-Host "📋 Deploying rules from $RULES_FILE..." -ForegroundColor Blue
firebase deploy --only firestore:rules --project $PROJECT_ID

if ($LASTEXITCODE -eq 0) {
  Write-Host "✅ Firestore security rules deployed successfully!" -ForegroundColor Green
  Write-Host "🔒 Your database is now protected with proper security rules." -ForegroundColor Green
  Write-Host ""
  Write-Host "📝 Rules Summary:" -ForegroundColor Cyan
  Write-Host "   - Public read access to hallOfFameMembers collection" -ForegroundColor White
  Write-Host "   - Write access restricted to authenticated service accounts" -ForegroundColor White
  Write-Host "   - All other collections are locked down" -ForegroundColor White
}
else {
  Write-Host "❌ Failed to deploy Firestore security rules" -ForegroundColor Red
  exit 1
}