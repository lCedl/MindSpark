from flask import Flask, render_template, request, redirect, url_for, jsonify, session
import requests
import json
import os

app = Flask(__name__)
app.secret_key = 'your-secret-key-here'

BACKEND_URL = os.environ.get("BACKEND_URL", "http://localhost:5163")

@app.route('/', methods=['GET', 'POST'])
def index():
    if request.method == 'POST':
        topic = request.form.get('topic')
        num_questions = int(request.form.get('num_questions'))
        try:
            response = requests.post(
                f"{BACKEND_URL}/api/quiz/generate",
                json={"topic": topic, "numberOfQuestions": num_questions}
            )
            if response.status_code == 200:
                quiz_data = response.json()
                session['current_quiz'] = quiz_data
                return redirect(url_for('quiz'))
            else:
                return render_template('index.html', error=f"Failed to generate quiz. HTTP Status: {response.status_code}")
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
    for question in quiz_data['questions']:
        answer_id = request.form.get(f'question_{question["id"]}')
        if answer_id:
            answers.append({"questionId": question["id"], "selectedAnswerId": int(answer_id)})
    try:
        response = requests.post(
            f"{BACKEND_URL}/api/quiz/submit",
            json={"quizId": quiz_data['id'], "playerName": player_name, "answers": answers}
        )
        if response.status_code == 200:
            result = response.json()
            session['quiz_result'] = result
            return redirect(url_for('results'))
        else:
            return jsonify({"error": "Failed to submit quiz"}), 500
    except requests.exceptions.RequestException:
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
    except requests.exceptions.RequestException:
        return render_template('quiz_list.html', quizzes=[], error="Unable to connect to backend service")

@app.route('/quiz/<int:quiz_id>')
def take_quiz(quiz_id):
    try:
        response = requests.get(f"{BACKEND_URL}/api/quiz/{quiz_id}")
        if response.status_code == 200:
            quiz_data = response.json()
            session['current_quiz'] = quiz_data
            return render_template('quiz.html', quiz=quiz_data)
        else:
            return render_template('index.html', error="Quiz not found")
    except requests.exceptions.RequestException:
        return render_template('index.html', error="Unable to connect to backend service")

@app.route('/quiz/<int:quiz_id>/review')
def review_quiz(quiz_id):
    try:
        quiz_response = requests.get(f"{BACKEND_URL}/api/quiz/{quiz_id}")
        if quiz_response.status_code != 200:
            return render_template('index.html', error="Quiz not found")
        quiz_data = quiz_response.json()
        results_response = requests.get(f"{BACKEND_URL}/api/quiz/results/{quiz_id}")
        results = []
        if results_response.status_code == 200:
            results = results_response.json()
        return render_template('quiz_review.html', quiz=quiz_data, results=results)
    except requests.exceptions.RequestException:
        return render_template('index.html', error="Unable to connect to backend service")

@app.route('/delete_quiz/<int:quiz_id>', methods=['DELETE'])
def delete_quiz(quiz_id):
    try:
        response = requests.delete(f"{BACKEND_URL}/api/quiz/{quiz_id}")
        if response.status_code == 200:
            return jsonify({"message": "Quiz deleted successfully"}), 200
        else:
            return jsonify({"error": "Failed to delete quiz"}), response.status_code
    except requests.exceptions.RequestException:
        return jsonify({"error": "Unable to connect to backend service"}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)

