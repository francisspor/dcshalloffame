# Cloud Run Deployment Script for DCS Hall of Fame API (PowerShell)
# Make sure you have gcloud CLI installed and configured

param(
  [string]$ProjectId = "dcshalloffame",
  [string]$ServiceName = "dcs-hall-of-fame-api",
  [string]$Region = "us-central1"
)

# Stop on any error
$ErrorActionPreference = "Stop"

# Configuration
$ImageName = "gcr.io/$ProjectId/$ServiceName"

Write-Host "🚀 Starting Cloud Run deployment..." -ForegroundColor Green

# Step 1: Build the Docker image
Write-Host "📦 Building Docker image..." -ForegroundColor Yellow
Set-Location "DCSHallOfFameApi"
docker build -t $ImageName .

if ($LASTEXITCODE -ne 0) {
  Write-Host "❌ Docker build failed!" -ForegroundColor Red
  exit 1
}

# Step 2: Push the image to Google Container Registry
Write-Host "📤 Pushing image to GCR..." -ForegroundColor Yellow
docker push $ImageName

if ($LASTEXITCODE -ne 0) {
  Write-Host "❌ Docker push failed!" -ForegroundColor Red
  exit 1
}

# Step 3: Deploy to Cloud Run
Write-Host "☁️ Deploying to Cloud Run..." -ForegroundColor Yellow
gcloud run deploy $ServiceName `
  --image $ImageName `
  --platform managed `
  --region $Region `
  --allow-unauthenticated `
  --port 8080 `
  --memory 512Mi `
  --cpu 1 `
  --max-instances 10 `
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production, AllowedOrigins__0=https://dcshalloffame.vercel.app, AllowedOrigins__1=http://localhost:3000, AllowedOrigins__2=https://localhost:3000, JwtSettings__SecretKey=your-super-secret-jwt-key-change-this-in-production

if ($LASTEXITCODE -ne 0) {
  Write-Host "❌ Cloud Run deployment failed!" -ForegroundColor Red
  exit 1
}

# Get the service URL
$ServiceUrl = gcloud run services describe $ServiceName --region $Region --format="value(status.url)"

Write-Host "✅ Deployment complete!" -ForegroundColor Green
Write-Host "🌐 Your API is available at: $ServiceUrl" -ForegroundColor Cyan
Write-Host "📊 API endpoint: $ServiceUrl/api/v1" -ForegroundColor Cyan

# Return to original directory
Set-Location ".."