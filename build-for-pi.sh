#!/bin/bash

# Build MindSpark for Raspberry Pi (ARM64 architecture)
# This script creates ARM64-compatible Docker images

set -e  # Exit on error

echo "🚀 Building MindSpark images for Raspberry Pi (ARM64)..."

# Configuration
BACKEND_IMAGE="mindspark-backend"
FRONTEND_IMAGE="mindspark-frontend"
TAG="arm64"

echo "📦 Building backend..."
docker buildx build \
    --platform linux/arm64 \
    --tag ${BACKEND_IMAGE}:${TAG} \
    --tag ${BACKEND_IMAGE}:latest \
    --load \
    ./backend

echo "📦 Building frontend..."
docker buildx build \
    --platform linux/arm64 \
    --tag ${FRONTEND_IMAGE}:${TAG} \
    --tag ${FRONTEND_IMAGE}:latest \
    --load \
    ./frontend

echo "✅ Build complete!"
echo ""
echo "📦 Images created:"
echo "  ${BACKEND_IMAGE}:${TAG}"
echo "  ${FRONTEND_IMAGE}:${TAG}"
echo ""
echo "🔍 Verify architecture:"
echo "  docker inspect ${BACKEND_IMAGE}:${TAG} | grep Architecture"
echo "  docker inspect ${FRONTEND_IMAGE}:${TAG} | grep Architecture"
echo ""
echo "💾 Save images:"
echo "  docker save ${BACKEND_IMAGE}:${TAG} -o ${BACKEND_IMAGE}-arm64.tar"
echo "  docker save ${FRONTEND_IMAGE}:${TAG} -o ${FRONTEND_IMAGE}-arm64.tar"
echo ""
echo "🚚 Transfer to Pi:"
echo "  scp ${BACKEND_IMAGE}-arm64.tar ${FRONTEND_IMAGE}-arm64.tar pi@YOUR_PI_IP:~/"
