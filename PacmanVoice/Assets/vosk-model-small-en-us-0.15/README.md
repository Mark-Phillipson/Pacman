Download the Vosk small English model (`vosk-model-small-en-us-0.15`) from:

https://alphacephei.com/vosk/models

Place the extracted model files (model/*) into this folder so the Android build can package them as assets.

Notes:
- Model files are large (~50+ MB). Consider using the small model for testing.
- The app will need code to initialize the Vosk recognizer and load the model from the Android assets folder (example: using Android `AssetManager` or `TitleContainer.OpenStream`).
- Native Vosk libraries (NDK) are required for Android; ensure the correct native binaries are added to appropriate `libs`/`runtimes` folders for the target ABI (arm64-v8a, armeabi-v7a, etc.).
