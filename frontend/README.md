# MindSpark Flask Frontend

## Setup with Virtual Environment (Recommended)

### Option 1: Automatic Setup
Run the setup script from the project root:
```bash
.\setup.ps1
```

### Option 2: Manual Setup

1. **Create a virtual environment**:
   ```bash
   python -m venv venv
   ```

2. **Activate the virtual environment**:
   - **Windows (PowerShell)**: `.\venv\Scripts\Activate.ps1`
   - **Windows (Command Prompt)**: `venv\Scripts\activate.bat`
   - **Linux/Mac**: `source venv/bin/activate`

3. **Install dependencies**:
   ```bash
   pip install -r requirements.txt
   ```

4. **Run the Flask app**:
   ```bash
   python app.py
   ```

The app will be available at http://localhost:5000

## Manual Startup (without virtual environment)

1. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```
2. Run the Flask app:
   ```bash
   python app.py
   ```

## Notes
- Always activate the virtual environment before running the application
- The virtual environment keeps your project dependencies isolated
- Use `deactivate` to exit the virtual environment 