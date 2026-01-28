# Azure Speech Provisioning (Dev notes) ✅

Quick steps to create and configure an Azure Speech resource for development:

1. Create resource
   - Go to the Azure portal → Create a resource → "Speech" (Cognitive Services)
   - Use the **Free** tier if you qualify (check the pricing page for limits)
   - Note the **Key** and **Region** values

2. Local development setup
   - Add environment variables to your dev machine (PowerShell example):
     ```powershell
     setx AZURE_SPEECH_KEY "<your-key>"
     setx AZURE_SPEECH_REGION "<your-region>"
     ```
   - Restart the terminal / IDE to pick up environment variables

3. Client security note
   - **Do not** embed subscription keys in distributed client builds.
   - For production, implement a token-broker service (server-side) that exchanges short-lived tokens for clients.
   - See: https://learn.microsoft.com/azure/cognitive-services/speech-service/rest-authentication

4. Quick SDK snippet (C#)
   - Add NuGet: `Microsoft.CognitiveServices.Speech`
   - Initialize:
     ```csharp
     var config = SpeechConfig.FromSubscription(key, region);
     using var recognizer = new SpeechRecognizer(config);
     var result = await recognizer.RecognizeOnceAsync();
     Console.WriteLine(result.Text);
     ```

5. Testing
   - Use the `voice-commands.json` command set for initial tests ("up", "down", "left", "right", "start", "pause", "quit")
   - Confirm microphone permission prompts appear on mobile devices

6. Monitoring & cost
   - Keep an eye on the Azure portal usage/quotas during testing; disable intensive telemetry if unnecessary.

7. Links
   - Azure Speech SDK docs: https://learn.microsoft.com/azure/cognitive-services/speech-service/
   - Pricing: https://azure.microsoft.com/pricing/details/cognitive-services/speech-services/
