# MindSpark Quiz App

A modern, AI-powered quiz application that generates personalized quizzes using OpenAI's API. Built with a Flask frontend and C# ASP.NET Core backend.

## 🚀 Features

- **AI-Powered Quiz Generation**: Uses OpenAI GPT to create engaging, topic-specific quizzes
- **Modern Web Interface**: Beautiful, responsive UI with smooth animations
- **Real-time Validation**: Form validation and user feedback
- **Score Tracking**: Save and view quiz results
- **Database Storage**: Persistent storage of quizzes and results
- **RESTful API**: Clean API design for easy integration

## 🏗️ Architecture

- **Frontend**: Flask (Python) web application
- **Backend**: C# ASP.NET Core Web API
- **Database**: Entity Framework with in-memory database (easily switchable to SQL Server)
- **AI Integration**: OpenAI GPT-3.5-turbo for quiz generation

## 📋 Prerequisites

- .NET 9.0 SDK
- Python 3.8+
- OpenAI API key

## 🔒 Security Setup

### **Important: Protect Your API Keys**

1. **Never commit API keys to Git**: The `.gitignore` file is configured to exclude sensitive files
2. **Use the template**: Copy `backend/appsettings.template.json` to `backend/appsettings.json`
3. **Add your API key**: Replace `"your-openai-api-key-here"` with your actual OpenAI API key

```bash
# Copy the template
cp backend/appsettings.template.json backend/appsettings.json

# Edit the file and add your API key
# Replace "your-openai-api-key-here" with your actual key
```

### **Files Excluded from Git:**
- ✅ `appsettings.json` (contains your API key)
- ✅ `appsettings.Development.json` (local development settings)
- ✅ `.env` files (environment variables)
- ✅ `*.key`, `*.pem` (certificate files)
- ✅ `secrets.json` (any secret files)

## 🛠️ Setup Instructions

### 1. Clone the Repository
```bash
git clone <repository-url>
cd MindSpark
```

### 2. Configure API Key
```bash
# Copy the template
cp backend/appsettings.template.json backend/appsettings.json

# Edit the file and add your OpenAI API key
# Replace "your-openai-api-key-here" with your actual key
```

### 3. Automatic Setup (Recommended)
Run the setup script to create a virtual environment and install dependencies:
```bash
.\setup.ps1
```

### 4. Manual Setup

#### Backend Setup
1. Navigate to the backend directory:
```bash
cd backend
```

2. Build and run the backend:
```bash
dotnet build
dotnet run
```

The backend will be available at `https://localhost:7001`

#### Frontend Setup with Virtual Environment
1. Navigate to the frontend directory:
```bash
cd frontend
```

2. Create a virtual environment:
```bash
python -m venv venv
```

3. Activate the virtual environment:
   - **Windows (PowerShell)**: `.\venv\Scripts\Activate.ps1`
   - **Windows (Command Prompt)**: `venv\Scripts\activate.bat`
   - **Linux/Mac**: `source venv/bin/activate`

4. Install Python dependencies:
```bash
pip install -r requirements.txt
```

5. Run the Flask application:
```bash
python app.py
```

The frontend will be available at `http://localhost:5000`

## 🚀 Quick Start

After setup, you can start both services using:

**PowerShell:**
```bash
.\start.ps1
```

**Command Prompt:**
```bash
start.bat
```

## 🎯 Usage

1. **Create a Quiz**:
   - Visit `http://localhost:5000`
   - Enter a topic (e.g., "World History", "Science", "Movies")
   - Choose the number of questions (1-20)
   - Click "Generate Quiz"

2. **Take the Quiz**:
   - Enter your name
   - Answer all questions
   - Submit to see your results

3. **View Results**:
   - See your score and performance
   - Get personalized feedback based on your performance

4. **Browse Quizzes**:
   - Click "View Recent Quizzes" to see previously generated quizzes

## 🔧 API Endpoints

### Quiz Generation
- `POST /api/quiz/generate` - Generate a new quiz
- `GET /api/quiz/{id}` - Get a specific quiz
- `GET /api/quiz/list` - Get list of recent quizzes

### Quiz Submission
- `POST /api/quiz/submit` - Submit quiz answers and get results
- `GET /api/quiz/results/{quizId}` - Get results for a specific quiz

## 🗄️ Database Schema

The application uses Entity Framework with the following entities:

- **Quiz**: Main quiz entity with topic and metadata
- **Question**: Individual questions with text and correct answer
- **Answer**: Multiple choice answers for each question
- **QuizResult**: Player results and scores

## 🔒 Security Notes

- ✅ API keys are excluded from Git via `.gitignore`
- ✅ Use environment variables for production deployments
- ✅ Enable HTTPS in production
- ✅ Consider rate limiting for OpenAI API calls
- ✅ Update the Flask secret key in production

## 🚀 Deployment

### Docker (Recommended)
```bash
docker-compose up -d
```

### Kubernetes (Raspberry Pi)
See comprehensive guides:
- **Quick Start**: `QUICK_DEPLOY.md`
- **Full Guide**: `K8S_DEPLOYMENT.md`
- **ARM64 Build**: `BUILD_FOR_ARM.md`

```bash
# Build for ARM64 (Raspberry Pi)
./build-for-pi.ps1  # Windows
# OR
./build-for-pi.sh   # Linux/Mac

# Deploy to Kubernetes
./deploy.ps1  # Windows
# OR
./deploy.sh   # Linux/Mac
```

### Manual Deployment
1. Deploy backend to Azure/AWS/your preferred platform
2. Deploy frontend to a web server
3. Update frontend configuration to point to your backend URL
4. Configure production database (SQL Server/PostgreSQL)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📝 License

This project is licensed under the MIT License.

## 🆘 Troubleshooting

### Common Issues

1. **Backend won't start**: Check if .NET 9.0 SDK is installed
2. **OpenAI API errors**: Verify your API key is correct and has sufficient credits
3. **Frontend can't connect to backend**: Check CORS settings and backend URL configuration
4. **Database issues**: Ensure Entity Framework migrations are applied
5. **Python not found**: Install Python and make sure it's added to PATH
6. **Virtual environment issues**: Make sure to activate the virtual environment before running the frontend
7. **API key not working**: Check if the key is valid and has proper permissions

### Getting Help

- Check the logs in both frontend and backend
- Verify all dependencies are installed
- Ensure ports 5000 and 7001 are available
- Make sure the virtual environment is activated for the frontend
- Verify your OpenAI API key is correctly configured
