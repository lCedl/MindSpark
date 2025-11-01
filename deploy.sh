#!/bin/bash

# MindSpark Kubernetes Deployment Script for Raspberry Pi
# This script deploys the MindSpark application to your Kubernetes cluster

set -e  # Exit on error

echo "🚀 Deploying MindSpark to Kubernetes..."

# Step 1: Create namespace
echo "📦 Creating namespace..."
kubectl apply -f mindspark.yaml

# Step 2: Create secret (update this with your actual API key!)
echo "🔐 Creating secrets..."
echo "⚠️  IMPORTANT: Edit backend/k8s/secret.yaml with your OpenAI API key first!"
read -p "Press enter to continue after editing secret.yaml, or Ctrl+C to cancel..."

kubectl apply -f backend/k8s/secret.yaml

# Step 3: Create persistent volume claim
echo "💾 Creating persistent volume claim..."
kubectl apply -f backend/k8s/pvc.yaml

# Step 4: Deploy backend
echo "🔧 Deploying backend..."
kubectl apply -f backend/k8s/service.yaml
kubectl apply -f backend/k8s/deployment.yaml

# Step 5: Deploy frontend
echo "🎨 Deploying frontend..."
kubectl apply -f frontend/k8s/service.yaml
kubectl apply -f frontend/k8s/deployment.yaml

# Step 6: Wait for deployments
echo "⏳ Waiting for deployments to be ready..."
kubectl wait --for=condition=available --timeout=300s deployment/mindspark-backend -n mindspark
kubectl wait --for=condition=available --timeout=300s deployment/mindspark-frontend -n mindspark

# Step 7: Show status
echo "✅ Deployment complete!"
echo ""
echo "📊 Deployment Status:"
kubectl get all -n mindspark

echo ""
echo "🌐 Access your application:"
echo "  Frontend: http://$(kubectl get nodes -o wide | awk 'NR==2 {print $6}')/"
echo "  Backend: http://$(kubectl get nodes -o wide | awk 'NR==2 {print $6}'):7001"
echo ""
echo "📝 To view logs:"
echo "  Backend: kubectl logs -f deployment/mindspark-backend -n mindspark"
echo "  Frontend: kubectl logs -f deployment/mindspark-frontend -n mindspark"
echo ""
echo "🗑️  To delete: kubectl delete namespace mindspark"
