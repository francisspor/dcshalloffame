# Cloud Run Deployment Guide

## Prerequisites

1. **Google Cloud CLI** installed and configured
2. **Docker** installed
3. **Google Cloud Project** with billing enabled
4. **Firebase credentials** file

## Setup Steps

### 1. Install Google Cloud CLI
```bash
# Download and install from: https://cloud.google.com/sdk/docs/install
gcloud init
gcloud auth configure-docker
```

### 2. Enable Required APIs
```bash
gcloud services enable run.googleapis.com
gcloud services enable containerregistry.googleapis.com
```

### 3. Configure Your Project
Edit `deploy-cloud-run.sh` and replace:
- `your-gcp-project-id` with your actual GCP project ID

### 4. Set Up Firebase Credentials
1. Copy your `firebase-credentials.json` to `DCSHallOfFameApi/`
2. Make sure it's included in the Docker build

### 5. Update Environment Configuration
Edit `DCSHallOfFameApi/appsettings.Production.json`:
- Update `AllowedOrigins` with your frontend domain
- Change the JWT secret to a production value
- Update Firebase project ID if needed

## Deployment

### Quick Deploy
```bash
chmod +x deploy-cloud-run.sh
./deploy-cloud-run.sh
```

### Manual Deploy
```bash
# Build and push image
cd DCSHallOfFameApi
docker build -t gcr.io/YOUR_PROJECT_ID/dcs-hall-of-fame-api .
docker push gcr.io/YOUR_PROJECT_ID/dcs-hall-of-fame-api

# Deploy to Cloud Run
gcloud run deploy dcs-hall-of-fame-api \
  --image gcr.io/YOUR_PROJECT_ID/dcs-hall-of-fame-api \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated \
  --port 8080
```

## Environment Variables

Set these in Cloud Run console or via CLI:
```bash
gcloud run services update dcs-hall-of-fame-api \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production
```

## Frontend Configuration

Update your frontend environment variables:
```env
NEXT_PUBLIC_API_URL=https://dcs-hall-of-fame-api-xxxxx-uc.a.run.app/api/v1
```

## Cost Optimization

- **Memory**: 512Mi (minimum for .NET)
- **CPU**: 1 (minimum)
- **Max instances**: 10 (prevents runaway costs)
- **Concurrency**: Default (80 requests per instance)

## Monitoring

- **Cloud Run Console**: View logs and metrics
- **Cloud Logging**: Centralized logging
- **Cloud Monitoring**: Performance metrics

## Security Notes

- ✅ HTTPS automatically enabled
- ✅ JWT authentication configured
- ✅ CORS properly configured
- ⚠️ Update JWT secret for production
- ⚠️ Restrict CORS origins to your domain