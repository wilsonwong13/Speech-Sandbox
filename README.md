# Speech-Sandbox

This project was inspired by [IBM Speech Sandbox](https://www.ibm.com/innovation/milab/work/speech-sandbox/)

## Before Getting Started 
If you don't have an IBM Cloud account and have API keys for Speech-to-Text (STT), Text-to-Speech (TTS), and Natural Language Understand(NLU), please follow these steps. Otherwise skip ahead. 
1. Sign up for IBM Cloud Account: https://console.bluemix.net/registration/
2. Once your in your dashboard go to Catalog and choose Watson on the left hand side 
3. Then sign up an lite/free verison of STT, TTS, and NLU and save the API keys 

If you don't have a Poly API key you need to get one 

## Setting Up
1. Download this repo and open it up in Unity 
2. In Unity menu, select Poly -> Poly Toolkit Settings and under Runtime tab add in your API key: https://developers.google.com/poly/develop/unity 
3. Then go to Script and open up Watson Services and enter in your API for STT, TTS, and NLU 
4. Then press play 

## Commands
- To make your request, start your request with "Watson" 
- For example: "Watson, I want a piano" or "Watson get me a car"
- This request get sent to NLU and the keywords of the string will to be sent to Poly for keywords 
- Then choose which asset you want by clicking on them, make sure you see a green dot over the object you want to select 
- You can restart the scene by saying restart 
