# Microsoft Cognitive Services

The sample uses Microsoft Cognitive Services to obtain some 
environment information using HoloLens.

The algorith is the following:

1. Wait for a voice command: "Describe" or "Read this text"
2. Capture a frame via MediaCapture (ignore holograms)
3. Send the frame as a jpg file to the Microsoft Vision API service (https://www.microsoft.com/cognitive-services/en-us/computer-vision-api)
4. Use text-to-speech API to present the results.

Video (click to play):

[![HoloLens + MS Cognition Services (Vision API) + UrhoSharp ](http://img.youtube.com/vi/Kq1NkrURTAo/0.jpg)](http://www.youtube.com/watch?v=Kq1NkrURTAo "HoloLens + MS Cognition Services (Vision API) + UrhoSharp ")
