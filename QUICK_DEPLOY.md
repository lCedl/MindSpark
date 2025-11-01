# 🚀 Quick Deployment Guide - Raspberry Pi

## Prerequisites Check

- [ ] Kubernetes cluster running on your Pi (k3s/minikube/etc.)
- [ ] `kubectl` installed and configured
- [ ] ARM64 Docker images built

## Quick Start (4 Steps)

### 1️⃣ Build ARM64 Images

**On your build machine (PC):**

```powershell
# Build for Raspberry Pi (ARM64)
.\build-for-pi.ps1
```

**Linux/Mac:**
```bash
chmod +x build-for-pi.sh && ./build-for-pi.sh
```

### 2️⃣ Transfer Images to Pi

**On your build machine:**

```bash
# Save images
docker save mindspark-backend:arm64 -o mindspark-backend.tar
docker save mindspark-frontend:arm64 -o mindspark-frontend.tar

# Transfer to Pi
scp mindspark-backend.tar mindspark-frontend.tar pi@YOUR_PI_IP:~/
```

**On Raspberry Pi:**

```bash
# Load images
docker load -i mindspark-backend.tar
docker load -i mindspark-frontend.tar
```

### 3️⃣ Configure Your API Key

**On Raspberry Pi:**

Edit `backend/k8s/secret.yaml`:

```yaml
stringData:
  OpenAI__ApiKey: "sk-your-actual-key-here"  # 👈 CHANGE THIS
```

### 4️⃣ Deploy

**On Raspberry Pi:**

```bash
# Make script executable (Linux/Mac)
chmod +x deploy.sh
./deploy.sh

# OR run on Windows (PowerShell)
.\deploy.ps1
```

## Done! 🎉

Access your app:
- Frontend: `http://YOUR_PI_IP`
- Backend: `http://YOUR_PI_IP:7001`

## Need Help?

- **Full Guide**: See `K8S_DEPLOYMENT.md`
- **Check Status**: `kubectl get all -n mindspark`
- **View Logs**: `kubectl logs -f deployment/mindspark-backend -n mindspark`

## Cleanup

```bash
kubectl delete namespace mindspark
```
