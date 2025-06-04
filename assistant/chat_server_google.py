from flask import Flask, request, jsonify
from gradio_client import Client
from flask_cors import CORS
import re
import json

app = Flask(__name__)
CORS(app) 

def build_prompt():
    return f"""
You are a friendly and professional Japanese teacher assistant.

Your main role is to help students understand and learn the Japanese writing systems: Kanji, Hiragana, and Katakana.
You will receive messages from students asking for translations of words or phrases, or questions about the Japanese language, culture, or history.

When the student asks for the translation of a word or phrase, ALWAYS no matter what answer in the next JSON format:

{{
    "teacher_message": "Your answer as a friendly teacher here (Always in English)",
    "hiragana": "Hiragana characters here",
    "katakana": "Katakana characters here",
    "kanji": "Kanji characters here",
    "pronunciation": "Pronunciation in Romaji here (optional)",
    "isTranslation": true
}}

An example answer would be:
{{
    "teacher_message": "beautiful word! cat in Japanese is neko!",
    "hiragana": "ねこ",
    "katakana": "ネコ",
    "kanji": "猫",
    "pronunciation": "neko",
    "isTranslation": true
}}

if the student ask for a question about Japanese language, culture, or history, answer naturally like a tutor with the next JSON format example:

{{
    "teacher_message": "Japanese culture is rich and diverse, with a history that spans thousands of years. From traditional arts like tea ceremonies to modern pop culture, there's so much to explore!",
    "hiragana": "",
    "katakana": "",
    "kanji": "",
    "pronunciation": "",
    "isTranslation": false
}}

stick to this formats strictly. Never add any extra text or explanations outside of the JSON structure.

answer in a friendly and professional manner, as if you were a helpful teacher assistant.
Always identify if the user is asking for a translation or is talking to you about Japanese language, culture, history or just having a conversation.
in case he is just having a conversation, Always answer in a friendly and professional manner, but do not provide any translations.

Restrictions:
- Always answer in English, but provide the Japanese translations in the specified formats.
- If the user is asking to translate always put the characters for the hiragana, katakana and kanji, Never let them empty, make sure they are correct.
Remenber always to answer in the JSON format no matter what the user asks.
"""

# To run this code you need to install the following dependencies:
# pip install google-genai

import base64
import os
from google import genai
from google.genai import types

@app.route('/ask', methods=['POST'])
def ask():
    data = request.get_json()
    print(data)
    user_message = data.get("message", "")

    debug_message = f"Received user message: {user_message}"
    print(debug_message)
    prompt = build_prompt()
    
    try:
        client = genai.Client(api_key="YOU_API_KEY",)
        model = "gemma-3-4b-it"
        contents = [
            types.Content(
                role="model",
                parts=[
                    types.Part.from_text(text=prompt),
                ],
            ),
            types.Content(
                role="user",
                parts=[
                    types.Part.from_text(text=user_message),
                ],
            ),
        ]
        generate_content_config = types.GenerateContentConfig(
            response_mime_type="text/plain",
        )
        completeText = ""
        for chunk in client.models.generate_content_stream(
            model=model,
            contents=contents,
            config=generate_content_config,
        ):
            if chunk.text:
                completeText += chunk.text
        print(f"Complete text received: {completeText}")
        match = re.search(r"```json\s*(\{.*?\})\s*```", completeText, re.DOTALL)
        if match:
            json_str = match.group(1)
            return jsonify(json.loads(json_str))
        else:
            completeText = re.sub(r":\s*''", ': ""', completeText)
            return jsonify(json.loads(completeText))
    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    app.run(host="0.0.0.0", port=5005)
