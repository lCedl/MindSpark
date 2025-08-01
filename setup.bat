@echo off
echo Setting up MindSpark Quiz Application...
echo.

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo Python not found! Please install Python first.
    echo Download from: https://www.python.org/downloads/
    echo Make sure to check 'Add Python to PATH' during installation.
    pause
    exit /b 1
) else (
    echo Python found!
)

REM Navigate to frontend directory
cd frontend

REM Create virtual environment
echo Creating virtual environment...
python -m venv venv

REM Activate virtual environment
echo Activating virtual environment...
call venv\Scripts\activate.bat

REM Install dependencies
echo Installing Python dependencies...
pip install -r requirements.txt

REM Go back to root directory
cd ..

REM Check if .NET is available
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo .NET not found! Please install .NET 9.0 SDK.
    echo Download from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
) else (
    echo .NET found!
)

echo.
echo Setup complete! 
echo.
echo Next steps:
echo 1. Configure your OpenAI API key in backend/appsettings.json
echo 2. Run the application using: start.bat
echo.
echo To activate the virtual environment manually:
echo cd frontend
echo venv\Scripts\activate.bat
echo.
pause 