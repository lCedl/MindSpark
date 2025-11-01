# Building for Raspberry Pi (ARM64)

MindSpark uses cross-platform technologies, but Docker images must be built for the correct architecture.

## 🎯 The Problem

- **Your PC/Laptop**: Likely running x86_64/AMD64
- **Raspberry Pi**: Running ARM64/ARMv8
- **Default Docker builds**: Native architecture only (AMD64 on your PC)

Building with default `docker build` creates AMD64 images that **won't run** on ARM64!

## ✅ The Solution

Use Docker **Buildx** with multi-platform support to build ARM64 images.

## 🛠️ Prerequisites

### 1. Enable Buildx (if not already enabled)

```bash
# Check if buildx is available
docker buildx version

# If not, enable experimental features
# Edit ~/.docker/config.json and add:
# {
#   "experimental": "enabled"
# }

# Create a builder instance
docker buildx create --name multiarch --use
```

### 2. Verify Multi-Platform Support

```bash
docker buildx inspect --bootstrap
```

You should see `linux/arm64` in the platforms list.

## 🚀 Building for ARM64

### Option 1: Use the Build Script (Recommended)

**Linux/Mac:**
```bash
chmod +x build-for-pi.sh
./build-for-pi.sh
```

**Windows (PowerShell):**
```powershell
.\build-for-pi.ps1
```

### Option 2: Manual Build Commands

**Backend:**
```bash
docker buildx build \
    --platform linux/arm64 \
    --tag mindspark-backend:arm64 \
    --tag mindspark-backend:latest \
    --load \
    ./backend
```

**Frontend:**
```bash
docker buildx build \
    --platform linux/arm64 \
    --tag mindspark-frontend:arm64 \
    --tag mindspark-frontend:latest \
    --load \
    ./frontend
```

## 🔍 Verify Architecture

```bash
# Check backend image architecture
docker inspect mindspark-backend:arm64 | grep Architecture

# Expected output:
# "Architecture": "arm64"
```

## 💾 Save and Transfer Images

### Save Images to Tar Files

```bash
docker save mindspark-backend:arm64 -o mindspark-backend-arm64.tar
docker save mindspark-frontend:arm64 -o mindspark-frontend-arm64.tar
```

**Windows:**
```powershell
docker save mindspark-backend:arm64 -o mindspark-backend-arm64.tar
docker save mindspark-frontend:arm64 -o mindspark-frontend-arm64.tar
```

### Transfer to Raspberry Pi

```bash
# Using scp
scp mindspark-backend-arm64.tar mindspark-frontend-arm64.tar pi@YOUR_PI_IP:~/

# Or using any file transfer method:
# - USB drive
# - Network share
# - SCP/SFTP client
# - etc.
```

### Load on Raspberry Pi

```bash
# SSH into your Pi
ssh pi@YOUR_PI_IP

# Load the images
docker load -i mindspark-backend-arm64.tar
docker load -i mindspark-frontend-arm64.tar

# Verify
docker images | grep mindspark
```

## 🏗️ Alternative: Build Directly on Pi

If building on Pi is preferred:

```bash
# Clone/transfer the project to Pi
scp -r . pi@YOUR_PI_IP:~/mindspark/

# SSH into Pi
ssh pi@YOUR_PI_IP
cd ~/mindspark

# Build (will be slower, but no architecture issues)
docker build -t mindspark-backend:latest ./backend
docker build -t mindspark-frontend:latest ./frontend
```

**Note**: Building .NET applications on ARM can be quite slow!

## 📝 Cross-Platform Compatibility

### Why MindSpark Works on ARM

✅ **ASP.NET Core**: Fully cross-platform (C# is managed code)  
✅ **Python**: Fully cross-platform  
✅ **SQLite**: Fully cross-platform  
✅ **Flask**: Pure Python, no native dependencies

### What About Dependencies?

| Component | ARM Support | Notes |
|-----------|-------------|-------|
| .NET 9.0 ASP.NET | ✅ Yes | Official ARM64 images |
| Python 3.9 | ✅ Yes | Official ARM64 images |
| Flask | ✅ Yes | Pure Python |
| Entity Framework | ✅ Yes | Managed code |
| SQLite EF Core | ✅ Yes | Cross-platform |
| OpenAI HTTP Client | ✅ Yes | Just HTTP calls |

**No native dependencies = Easy ARM support!** 🎉

## 🐛 Troubleshooting

### Buildx Not Found

```bash
# On Docker Desktop for Windows/Mac, buildx is included
# If missing, update Docker

# On Linux:
sudo apt-get update
sudo apt-get install docker-buildx-plugin
```

### Architecture Mismatch Error on Pi

```bash
# If you see "exec format error" on Pi:
# - The image was built for wrong architecture
# - Rebuild using buildx with --platform linux/arm64
```

### Build Too Slow on Pi

Use the cross-compile approach (build on your PC, transfer to Pi). It's **much faster**.

### Image Transfer Issues

For large images, consider:

1. **Compress before transfer:**
```bash
gzip mindspark-backend-arm64.tar
scp mindspark-backend-arm64.tar.gz pi@YOUR_PI_IP:~/
# On Pi: gunzip mindspark-backend-arm64.tar.gz
```

2. **Use Docker Hub** (if you have account):
```bash
docker tag mindspark-backend:arm64 YOUR_USERNAME/mindspark-backend:arm64
docker push YOUR_USERNAME/mindspark-backend:arm64
# On Pi: docker pull YOUR_USERNAME/mindspark-backend:arm64
```

## ✅ Quick Checklist

- [ ] Docker buildx installed and working
- [ ] Built images with `--platform linux/arm64`
- [ ] Verified architecture with `docker inspect`
- [ ] Saved images to tar files
- [ ] Transferred to Raspberry Pi
- [ ] Loaded images on Pi
- [ ] Ready to deploy with Kubernetes

## 🚀 Next Steps

After building and transferring images:

1. **Load images on Pi:** `docker load -i *.tar`
2. **Edit secrets:** Update `backend/k8s/secret.yaml`
3. **Deploy:** Run `./deploy.sh` or `.\deploy.ps1` on Pi
4. **Access:** Open your app in browser

See `K8S_DEPLOYMENT.md` for full deployment guide.
