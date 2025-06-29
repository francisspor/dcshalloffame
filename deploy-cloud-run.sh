#!/bin/bash

# Cloud Run Deployment Script for DCS Hall of Fame API
# Make sure you have gcloud CLI installed and configured

set -e

# Configuration
PROJECT_ID="dcshalloffame"
SERVICE_NAME="dcs-hall-of-fame-api"
REGION="us-central1"
IMAGE_NAME="gcr.io/$PROJECT_ID/$SERVICE_NAME"

echo "🚀 Starting Cloud Run deployment..."

# Step 1: Build the Docker image
echo "📦 Building Docker image..."
cd DCSHallOfFameApi
docker build -t $IMAGE_NAME .

# Step 2: Push the image to Google Container Registry
echo "📤 Pushing image to GCR..."
docker push $IMAGE_NAME

# Step 3: Deploy to Cloud Run
echo "☁️ Deploying to Cloud Run..."
gcloud run deploy $SERVICE_NAME \
  --image $IMAGE_NAME \
  --platform managed \
  --region $REGION \
  --allow-unauthenticated \
  --port 8080 \
  --memory 512Mi \
  --cpu 1 \
  --max-instances 10 \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production

echo "✅ Deployment complete!"
echo "🌐 Your API is available at: https://$SERVICE_NAME-$REGION-$PROJECT_ID.a.run.app"