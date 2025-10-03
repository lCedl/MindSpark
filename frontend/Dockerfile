FROM python:3.9-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# copy src and templates separately to avoid copying venv etc.
COPY src/ ./

EXPOSE 5000

CMD ["python", "app.py"]