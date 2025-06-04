from flask import Flask, request, jsonify
from gradio_client import Client
from flask_cors import CORS
import re
import json

app = Flask(__name__)
CORS(app)  # Allow requests from Unity

# Load Hugging Face token from environment or fallback
HF_TOKEN = "your_huggingface_token_here"  # Replace with your actual token or load from environment variable

# Connect with authentication
client = Client("huggingface-projects/gemma-2-2b-jpn-it", hf_token=HF_TOKEN)
def build_prompt(user_input):
    return f"""
You are a friendly and professional Japanese teacher assistant.

Your main role is to help students understand and learn the Japanese writing systems: Kanji, Hiragana, and Katakana.
You will receive messages from students asking for translations of words or phrases, or questions about the Japanese language, culture, or history.

When the student asks for the translation of a word or phrase, respond in this **JSON format**:

{{
    "teacher_message": "Your response as a friendly teacher here (Always in English)",
    "hiragana": "Hiragana characters here",
    "katakana": "Katakana characters here",
    "kanji": "Kanji characters here",
    "pronunciation": "Pronunciation in Romaji here (optional)"
    "isTranslation": true
}}

An example response would be:
{{
    "teacher_message": "beautiful word! cat in Japanese is neko!",
    "hiragana": "ねこ",
    "katakana": "ネコ",
    "kanji": "猫",
    "pronunciation": "neko"
    "isTranslation": true
}}

if the student ask for a question about Japanese language, culture, or history, respond naturally like a tutor with the next JSON format example:

{{
    "teacher_message": "Japanese culture is rich and diverse, with a history that spans thousands of years. From traditional arts like tea ceremonies to modern pop culture, there's so much to explore!",
    "hiragana": '',
    "katakana": '',
    "kanji": '',
    "pronunciation": ''
    "isTranslation": false
}}

stick to this formats strictly. Do not add any extra text or explanations outside of the JSON structure.

Respond in a friendly and professional manner, as if you were a helpful teacher assistant.
Always identify if the user is asking for a translation or is talking to you about Japanese language, culture, history or just having a conversation.
in case he is just having a conversation, Always respond in a friendly and professional manner, but do not provide any translations.

📝 Notes:
- Always answer in English, but provide the Japanese translations in the specified formats.

User input: "{user_input}"
"""


@app.route('/ask', methods=['POST'])
def ask():
    data = request.get_json()
    print(data)
    user_message = data.get("message", "")

    debug_message = f"Received user message: {user_message}"
    print(debug_message)
    prompt = build_prompt(user_message)
    
    try:
        raw_result = client.predict(
            message=prompt,
            param_2=1024,
            param_3=0.6,
            param_4=0.9,
            param_5=50,
            param_6=1.2,
            api_name="/chat"
        )
        print(f"Model response: {raw_result}")
        match = re.search(r"```json\s*(\{.*?\})\s*```", raw_result, re.DOTALL)
        if match:
            json_str = match.group(1)
            return jsonify(json.loads(json_str))
        else:
            return jsonify({"error": "Could not extract JSON from response", "raw": raw_result}), 500

    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    app.run(host="0.0.0.0", port=5005)
