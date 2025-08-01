from flask import Flask, render_template, request, redirect, url_for, jsonify, session
import requests
import json

app = Flask(__name__)
app.secret_key = 'your-secret-key-here'  # Change this in production

# Backend API configuration
BACKEND_URL = "http://localhost:5163"  # Updated to match backend port

@app.route('/', methods=['GET', 'POST'])
def index():
    if request.method == 'POST':
        topic = request.form.get('topic')
        num_questions = int(request.form.get('num_questions'))
        
        # Call backend API to generate quiz
        try:
            response = requests.post(
                f"{BACKEND_URL}/api/quiz/generate",
                json={
                    "topic": topic,
                    "numberOfQuestions": num_questions
                }
            )
            
            if response.status_code == 200:
                quiz_data = response.json()
                session['current_quiz'] = quiz_data
                return redirect(url_for('quiz'))
            else:
                error_message = f"Failed to generate quiz. HTTP Status: {response.status_code}"
                try:
                    error_data = response.json()
                    if 'error' in error_data:
                        error_message = f"OpenAI Error: {error_data['error']}"
                        if 'details' in error_data:
                            error_message += f" (Details: {error_data['details'][:200]}...)"
                except:
                    error_message = f"Failed to generate quiz. HTTP Status: {response.status_code}, Response: {response.text[:200]}"
                
                return render_template('index.html', error=error_message)
                
        except requests.exceptions.RequestException as e:
            return render_template('index.html', error=f"Unable to connect to backend service: {str(e)}")
    
    return render_template('index.html')

@app.route('/quiz')
def quiz():
    quiz_data = session.get('current_quiz')
    if not quiz_data:
        return redirect(url_for('index'))
    
    return render_template('quiz.html', quiz=quiz_data)

@app.route('/submit_quiz', methods=['POST'])
def submit_quiz():
    quiz_data = session.get('current_quiz')
    if not quiz_data:
        return jsonify({"error": "No quiz found"}), 400
    
    player_name = request.form.get('player_name')
    answers = []
    
    # Collect answers from form
    for question in quiz_data['questions']:
        answer_id = request.form.get(f'question_{question["id"]}')
        if answer_id:
            answers.append({
                "questionId": question["id"],
                "selectedAnswerId": int(answer_id)
            })
    
    # Submit to backend
    try:
        response = requests.post(
            f"{BACKEND_URL}/api/quiz/submit",
            json={
                "quizId": quiz_data['id'],
                "playerName": player_name,
                "answers": answers
            }
        )
        
        if response.status_code == 200:
            result = response.json()
            session['quiz_result'] = result
            return redirect(url_for('results'))
        else:
            return jsonify({"error": "Failed to submit quiz"}), 500
            
    except requests.exceptions.RequestException as e:
        return jsonify({"error": "Unable to connect to backend service"}), 500

@app.route('/results')
def results():
    result = session.get('quiz_result')
    if not result:
        return redirect(url_for('index'))
    
    return render_template('results.html', result=result)

@app.route('/quiz_list')
def quiz_list():
    try:
        response = requests.get(f"{BACKEND_URL}/api/quiz/list")
        if response.status_code == 200:
            quizzes = response.json()
            return render_template('quiz_list.html', quizzes=quizzes)
        else:
            return render_template('quiz_list.html', quizzes=[], error="Failed to load quizzes")
    except requests.exceptions.RequestException as e:
        return render_template('quiz_list.html', quizzes=[], error="Unable to connect to backend service")

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True) 