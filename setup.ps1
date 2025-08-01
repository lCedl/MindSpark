Write-Host "Setting up MindSpark Quiz Application..." -ForegroundColor Green
Write-Host ""

# Check if Python is installed
try {
    $pythonVersion = python --version 2>&1
    Write-Host "Python found: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "Python not found! Please install Python first." -ForegroundColor Red
    Write-Host "Download from: https://www.python.org/downloads/" -ForegroundColor Yellow
    Write-Host "Make sure to check 'Add Python to PATH' during installation." -ForegroundColor Yellow
    exit 1
}

# Navigate to frontend directory
Set-Location frontend

# Create virtual environment
Write-Host "Creating virtual environment..." -ForegroundColor Yellow
python -m venv venv

# Activate virtual environment
Write-Host "Activating virtual environment..." -ForegroundColor Yellow
& ".\venv\Scripts\Activate.ps1"

# Install dependencies
Write-Host "Installing Python dependencies..." -ForegroundColor Yellow
pip install -r requirements.txt

# Go back to root directory
Set-Location ..

# Check if .NET is available
try {
    $dotnetVersion = dotnet --version
    Write-Host ".NET found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host ".NET not found! Please install .NET 9.0 SDK." -ForegroundColor Red
    Write-Host "Download from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Setup complete! 🎉" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Configure your OpenAI API key in backend/appsettings.json" -ForegroundColor White
Write-Host "2. Run the application using: .\start.ps1" -ForegroundColor White
Write-Host ""
Write-Host "To activate the virtual environment manually:" -ForegroundColor Gray
Write-Host "cd frontend" -ForegroundColor Gray
Write-Host ".\venv\Scripts\Activate.ps1" -ForegroundColor Gray 