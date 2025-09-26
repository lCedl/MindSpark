Write-Host "Starting MindSpark Quiz Application..." -ForegroundColor Green
Write-Host ""

Write-Host "Starting Backend (C# API)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd backend; dotnet run"

Write-Host "Waiting for backend to start..." -ForegroundColor Cyan
Start-Sleep -Seconds 5

Write-Host "Starting Frontend (Flask) with virtual environment..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd frontend; .\venv\Scripts\Activate.ps1; python src\app.py"

Write-Host ""
Write-Host "Services are starting..." -ForegroundColor Green
Write-Host "Backend will be available at: https://localhost:7001" -ForegroundColor Cyan
Write-Host "Frontend will be available at: http://localhost:5000" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to exit this launcher..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") 