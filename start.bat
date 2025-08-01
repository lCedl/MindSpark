@echo off
echo Starting MindSpark Quiz Application...
echo.

echo Starting Backend (C# API)...
start "Backend" cmd /k "cd backend && dotnet run"

echo Waiting for backend to start...
timeout /t 5 /nobreak > nul

echo Starting Frontend (Flask) with virtual environment...
start "Frontend" cmd /k "cd frontend && venv\Scripts\activate.bat && python app.py"

echo.
echo Services are starting...
echo Backend will be available at: https://localhost:7001
echo Frontend will be available at: http://localhost:5000
echo.
echo Press any key to exit this launcher...
pause > nul 