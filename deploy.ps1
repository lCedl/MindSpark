# MindSpark Kubernetes Deployment Script for Raspberry Pi
# PowerShell version

Write-Host "🚀 Deploying MindSpark to Kubernetes..." -ForegroundColor Green
Write-Host ""

# Step 1: Create namespace
Write-Host "📦 Creating namespace..." -ForegroundColor Yellow
kubectl apply -f mindspark.yaml

# Step 2: Create secret
Write-Host "🔐 Creating secrets..." -ForegroundColor Yellow
Write-Host "⚠️  IMPORTANT: Edit backend/k8s/secret.yaml with your OpenAI API key first!" -ForegroundColor Red
$confirm = Read-Host "Press enter to continue after editing secret.yaml, or Ctrl+C to cancel"

kubectl apply -f backend/k8s/secret.yaml

# Step 3: Create persistent volume claim
Write-Host "💾 Creating persistent volume claim..." -ForegroundColor Yellow
kubectl apply -f backend/k8s/pvc.yaml

# Step 4: Deploy backend
Write-Host "🔧 Deploying backend..." -ForegroundColor Yellow
kubectl apply -f backend/k8s/service.yaml
kubectl apply -f backend/k8s/deployment.yaml

# Step 5: Deploy frontend
Write-Host "🎨 Deploying frontend..." -ForegroundColor Yellow
kubectl apply -f frontend/k8s/service.yaml
kubectl apply -f frontend/k8s/deployment.yaml

# Step 6: Wait for deployments
Write-Host "⏳ Waiting for deployments to be ready..." -ForegroundColor Cyan
kubectl wait --for=condition=available --timeout=300s deployment/mindspark-backend -n mindspark
kubectl wait --for=condition=available --timeout=300s deployment/mindspark-frontend -n mindspark

# Step 7: Show status
Write-Host "✅ Deployment complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Deployment Status:" -ForegroundColor Cyan
kubectl get all -n mindspark

Write-Host ""
Write-Host "📝 To view logs:" -ForegroundColor Cyan
Write-Host "  Backend: kubectl logs -f deployment/mindspark-backend -n mindspark"
Write-Host "  Frontend: kubectl logs -f deployment/mindspark-frontend -n mindspark"
Write-Host ""
Write-Host "🗑️  To delete: kubectl delete namespace mindspark" -ForegroundColor Red
