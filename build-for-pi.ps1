# Build MindSpark for Raspberry Pi (ARM64 architecture)
# PowerShell version

Write-Host "🚀 Building MindSpark images for Raspberry Pi (ARM64)..." -ForegroundColor Green

# Configuration
$BACKEND_IMAGE = "mindspark-backend"
$FRONTEND_IMAGE = "mindspark-frontend"
$TAG = "arm64"

Write-Host "📦 Building backend..." -ForegroundColor Yellow
docker buildx build `
    --platform linux/arm64 `
    --tag ${BACKEND_IMAGE}:${TAG} `
    --tag ${BACKEND_IMAGE}:latest `
    --load `
    ./backend

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Backend build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "📦 Building frontend..." -ForegroundColor Yellow
docker buildx build `
    --platform linux/arm64 `
    --tag ${FRONTEND_IMAGE}:${TAG} `
    --tag ${FRONTEND_IMAGE}:latest `
    --load `
    ./frontend

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Frontend build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📦 Images created:" -ForegroundColor Cyan
Write-Host "  ${BACKEND_IMAGE}:${TAG}"
Write-Host "  ${FRONTEND_IMAGE}:${TAG}"
Write-Host ""
Write-Host "🔍 Verify architecture:" -ForegroundColor Cyan
Write-Host "  docker inspect ${BACKEND_IMAGE}:${TAG} | findstr Architecture"
Write-Host "  docker inspect ${FRONTEND_IMAGE}:${TAG} | findstr Architecture"
Write-Host ""
Write-Host "💾 Save images:" -ForegroundColor Cyan
Write-Host "  docker save ${BACKEND_IMAGE}:${TAG} -o ${BACKEND_IMAGE}-arm64.tar"
Write-Host "  docker save ${FRONTEND_IMAGE}:${TAG} -o ${FRONTEND_IMAGE}-arm64.tar"
Write-Host ""
Write-Host "🚚 Transfer to Pi:" -ForegroundColor Cyan
Write-Host "  scp ${BACKEND_IMAGE}-arm64.tar ${FRONTEND_IMAGE}-arm64.tar pi@YOUR_PI_IP:~/"
