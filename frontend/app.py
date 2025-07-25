from flask import Flask, render_template, request, redirect, url_for

app = Flask(__name__)

@app.route('/', methods=['GET', 'POST'])
def index():
    if request.method == 'POST':
        topic = request.form.get('topic')
        num_questions = request.form.get('num_questions')
        # Placeholder: send to backend API (to be implemented)
        return render_template('quiz.html', topic=topic, num_questions=num_questions, questions=None)
    return render_template('index.html')

@app.route('/quiz', methods=['POST'])
def quiz():
    # Placeholder for quiz logic
    return 'Quiz logic will go here.'

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True) 