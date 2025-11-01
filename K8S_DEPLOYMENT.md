# MindSpark Kubernetes Deployment Guide for Raspberry Pi

This guide explains how to deploy MindSpark to a Kubernetes cluster on your Raspberry Pi.

## 📋 Prerequisites

- Kubernetes cluster running on Raspberry Pi (k3s, k8s, etc.)
- `kubectl` configured to access your cluster
- Docker images built and available
- OpenAI API key

## 🏗️ Architecture Overview

- **Namespace**: `mindspark`
- **Backend**: ASP.NET Core API on port 7001
- **Frontend**: Flask web app on port 5000 (exposed via LoadBalancer on port 80)
- **Database**: SQLite stored in PersistentVolume
- **Secrets**: OpenAI API key stored in Kubernetes Secret

## 🖼️ Image Handling

### Option 1: Push to Docker Hub

```bash
# Tag your images
docker tag mindspark-backend:latest YOUR_DOCKERHUB_USERNAME/mindspark-backend:latest
docker tag mindspark-frontend:latest YOUR_DOCKERHUB_USERNAME/mindspark-frontend:latest

# Push to Docker Hub
docker push YOUR_DOCKERHUB_USERNAME/mindspark-backend:latest
docker push YOUR_DOCKERHUB_USERNAME/mindspark-frontend:latest

# On Raspberry Pi, update deployment.yaml to use:
# image: YOUR_DOCKERHUB_USERNAME/mindspark-backend:latest
```

### Option 2: Load Images Directly (Recommended for Pi)

```bash
# On your build machine, save images
docker save mindspark-backend:latest -o mindspark-backend.tar
docker save mindspark-frontend:latest -o mindspark-frontend.tar

# Transfer to Raspberry Pi
scp mindspark-backend.tar pi@your-pi-ip:~/
scp mindspark-frontend.tar pi@your-pi-ip:~/

# On Raspberry Pi, load images
docker load -i mindspark-backend.tar
docker load -i mindspark-frontend.tar
```

### Option 3: Build on Raspberry Pi

```bash
# Copy entire project to Pi
scp -r . pi@your-pi-ip:~/mindspark/

# SSH into Pi and build
ssh pi@your-pi-ip
cd ~/mindspark
docker build -t mindspark-backend:latest ./backend
docker build -t mindspark-frontend:latest ./frontend
```

⚠️ **Note**: Building .NET images on ARM can be slow. Option 2 is recommended.

## 🔧 Deployment Steps

### 1. Configure Secrets

Edit `backend/k8s/secret.yaml` and replace `your-openai-api-key-here` with your actual API key:

```yaml
stringData:
  OpenAI__ApiKey: "sk-your-actual-key-here"
```

### 2. Adjust Storage Class (if needed)

Edit `backend/k8s/pvc.yaml` and update `storageClassName` based on your cluster:

```yaml
storageClassName: local-path  # For k3s
# OR
storageClassName: hostpath  # For microk8s
# OR  
storageClassName: standard  # For other setups
```

### 3. Deploy Everything

**Option A: Using the script**

```bash
chmod +x deploy.sh
./deploy.sh
```

**Option B: Manual deployment**

```bash
# Create namespace
kubectl apply -f mindspark.yaml

# Create secret
kubectl apply -f backend/k8s/secret.yaml

# Create PVC
kubectl apply -f backend/k8s/pvc.yaml

# Deploy backend
kubectl apply -f backend/k8s/service.yaml
kubectl apply -f backend/k8s/deployment.yaml

# Deploy frontend
kubectl apply -f frontend/k8s/service.yaml
kubectl apply -f frontend/k8s/deployment.yaml

# Wait for deployment
kubectl wait --for=condition=available --timeout=300s deployment/mindspark-backend -n mindspark
kubectl wait --for=condition=available --timeout=300s deployment/mindspark-frontend -n mindspark
```

## 🔍 Verify Deployment

```bash
# Check pods
kubectl get pods -n mindspark

# Check services
kubectl get svc -n mindspark

# Check logs
kubectl logs -f deployment/mindspark-backend -n mindspark
kubectl logs -f deployment/mindspark-frontend -n mindspark
```

## 🌐 Access Your Application

### Get Service Info

```bash
# Check LoadBalancer external IP
kubectl get svc mindspark-frontend-service -n mindspark
```

### Access Options

1. **Via LoadBalancer**: If your cluster supports LoadBalancer, use the external IP
2. **Via NodePort**: Change service type to NodePort
3. **Via Port Forward** (for testing):

```bash
# Forward frontend
kubectl port-forward svc/mindspark-frontend-service 8080:80 -n mindspark

# Forward backend
kubectl port-forward svc/mindspark-backend-service 7001:7001 -n mindspark
```

Then access:
- Frontend: http://localhost:8080
- Backend: http://localhost:7001

## 🔄 Update Application

```bash
# After rebuilding and loading new images
kubectl rollout restart deployment/mindspark-backend -n mindspark
kubectl rollout restart deployment/mindspark-frontend -n mindspark
```

## 🗑️ Cleanup

```bash
# Delete everything in namespace
kubectl delete namespace mindspark

# Or delete individual resources
kubectl delete -f backend/k8s/
kubectl delete -f frontend/k8s/
kubectl delete -f mindspark.yaml
```

## 🐛 Troubleshooting

### Pods not starting

```bash
# Check pod status
kubectl describe pod -n mindspark

# Check events
kubectl get events -n mindspark --sort-by='.lastTimestamp'
```

### Image pull errors

```bash
# Verify images are loaded
docker images | grep mindspark

# Check image architecture
docker inspect mindspark-backend:latest | grep Architecture
```

### Database issues

```bash
# Check PVC status
kubectl get pvc -n mindspark

# Check volume mount
kubectl describe pod -n mindspark -l app=mindspark-backend
```

### API key issues

```bash
# Verify secret exists
kubectl get secret mindspark-secrets -n mindspark

# Check if key is set (be careful!)
kubectl get secret mindspark-secrets -n mindspark -o yaml
```

## 📝 Notes for Raspberry Pi

- **Single replica**: Deployment uses 1 replica to save resources
- **SQLite**: ReadWriteOnce PV means only one pod can write
- **LoadBalancer**: May not work in all Pi setups; consider NodePort or Ingress
- **Resources**: Adjust CPU/memory limits in deployments for Pi constraints

## 🔐 Security Considerations

1. **API Key**: Never commit secrets to git
2. **HTTPS**: Consider adding ingress with TLS
3. **Network Policy**: Restrict pod-to-pod communication
4. **RBAC**: Use proper service accounts

## 📚 Additional Resources

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [k3s Documentation](https://k3s.io/)
- [Minikube for Pi](https://github.com/rancher/k3s)
