using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Services.NaturalLanguageUnderstanding.v1;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;
using System.Collections.Generic;
using UnityEngine.UI;
using IBM.Watson.DeveloperCloud.Connection;
using PolyToolkit;
using UnityEngine.SceneManagement;

public class WatsonServices : MonoBehaviour {
    //Streaming, Speech to Text
    private string sttUsername = null;
    private string sttPassword = null;
    private string sttUrl = null;
    private string userMessage;
    private bool newMessage = false;
    private int _recordingRoutine = 0;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingBufferSize = 1;
    private int _recordingHZ = 22050;
    private SpeechToText _speechToText;

    //Natural Language Understanding
    private string nluUsername = null;
    private string nluPassword = null;
    private string nluUrl = null;
    NaturalLanguageUnderstanding _naturalLanguageUnderstanding;

    //Text to Speech 
    private string ttsUsername = null;
    private string ttsPassword = null;
    private string ttsUrl = null;
    TextToSpeech _textToSpeech;

    //PolyAPI 
    private string nluResults = "";
    private int assetCount = 0;
    public Text attributionsText;
    public Text statusText;

    //Camera 
    Camera m_MainCamera;
        
    //Canvas
    public GameObject menu;
    MulitAssets createAssestfun = new MulitAssets();

    public static bool removeBackboard;

    //AuthenicationTokens
    private string _authenticationTokenSST;
    private string _authenticationTokenTTS;
    private string _authenticationTokenNLU;

    void Start()
    {
        LogSystem.InstallDefaultReactors();
        if (!Utility.GetToken(OnGetTokenSST, sttUrl, sttUsername, sttPassword))
            Log.Debug("ExampleGetToken.Start()", "Failed to get token.");

        if (!Utility.GetToken(OnGetTokenTTS, ttsUrl, ttsUsername, ttsPassword))
            Log.Debug("ExampleGetToken.Start()", "Failed to get token.");

        if (!Utility.GetToken(OnGetTokenNLU, nluUrl, nluUsername, nluPassword))
            Log.Debug("ExampleGetToken.Start()", "Failed to get token.");

        //STT
        Credentials credentials = new Credentials(sttUsername, sttPassword, sttUrl)
        {
            AuthenticationToken = _authenticationTokenSST
        };
        _speechToText = new SpeechToText(credentials);
        Active = true;
        StartRecording();

        //TTS
        Credentials ttscredentials = new Credentials(ttsUsername, ttsPassword, ttsUrl)
        {
            AuthenticationToken = _authenticationTokenTTS
        };
        _textToSpeech = new TextToSpeech(ttscredentials);

        //NLU 
        Credentials nlucredentials = new Credentials(nluUsername, nluPassword, nluUrl)
        {
            AuthenticationToken = _authenticationTokenNLU
        };
        _naturalLanguageUnderstanding = new NaturalLanguageUnderstanding(nlucredentials);

        //Canvas
        menu.SetActive(false);
    }


    void Update()
    {
        //When the user makes a new Request, this method is called 
        if (newMessage)
        {
            newMessage = false;
            //Setting the user message as part of the parameters, needed for NLU 
            parameters.text = userMessage;
            //Creating the Canvas in front of the user
            menu.gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
            menu.gameObject.transform.rotation = Camera.main.transform.rotation;
            //Setting the canvas true to make it visible to the user
            menu.SetActive(true);
            //Setting assetsAppeared false, due to user not choosing an asset yet 
            singleAssestDisplay.assetAppeared = false;
            //Calling NLU 
            _naturalLanguageUnderstanding.Analyze(OnAnalyze, OnFail, parameters);
        }
        //When the user has choosen an asset this method is called
        if(singleAssestDisplay.assetAppeared) {
            //Each child is destory on the canvas panel, the reason is that if left, the childern will still appear in the next user request 
            MulitAssets.destoryChildern();
            //Removing the canvas after the user choosen their asset 
            menu.SetActive(false);
        }
        //When the user request came in, this method is called 
        if(MulitAssets.creatingAssestsMessageFlag) {
            //Setting the flag to false 
            MulitAssets.creatingAssestsMessageFlag = false;
            //Invoking TTS
            string ttsString = "I have found " + MulitAssets.numberOfAssetsFoundFromPoly + " objects that match your request";
            Synthesize(ttsString);
        }
        //When the user has choosen an asset this method is called
        if(singleAssestDisplay.assetGenerate) {
            string ttsString = singleAssestDisplay.assetName.ToString() + " ... Is there anything else you want to create?";
            Synthesize(ttsString);
            singleAssestDisplay.assetGenerate = false; 
        }
    }

    //Getting Authentication Token for Watson Services 
    private void OnGetTokenSST(AuthenticationToken authenticationToken, string customData)
    {
        _authenticationTokenSST = authenticationToken.ToString();
    }

    private void OnGetTokenTTS(AuthenticationToken authenticationToken, string customData)
    {
        _authenticationTokenTTS = authenticationToken.ToString();
    }

    private void OnGetTokenNLU(AuthenticationToken authenticationToken, string customData)
    {
        _authenticationTokenNLU = authenticationToken.ToString();
    }


    //Making a request Poly method 
    private void requestPolyAssets() {
        PolyListAssetsRequest req = new PolyListAssetsRequest();
        req.keywords = nluResults;
        req.curated = true;
        req.orderBy = PolyOrderBy.BEST; 
        PolyApi.ListAssets(req, ListAssetsCallback);
    }

    // Poly API 
    private void ListAssetsCallback(PolyStatusOr<PolyListAssetsResult> result)
    {
        createAssestfun.createAssest(result);
    }


    private void GetAssetCallback(PolyStatusOr<PolyAsset> result)
    {
        if (!result.Ok)
        {
            Debug.LogError("Failed to get assets. Reason: " + result.Status);
            //statusText.text = "ERROR: " + result.Status;
            return;
        }
        Debug.Log("Successfully got asset!");
        PolyImportOptions options = PolyImportOptions.Default();
        options.rescalingMode = PolyImportOptions.RescalingMode.FIT;
        options.desiredSize = 0.5f;
        PolyApi.Import(result.Value, options, ImportAssetCallback);
    }

    // Callback invoked when an asset has just been imported.
    private void ImportAssetCallback(PolyAsset asset, PolyStatusOr<PolyImportResult> result)
    {
        if (!result.Ok)
        {
            Debug.LogError("Failed to import asset. :( Reason: " + result.Status);
            return;
        }
        Debug.Log("Successfully imported asset!");
        result.Value.gameObject.gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1;
        result.Value.gameObject.AddComponent<Rotate>();
    }

    //TTS
    private void Synthesize(string response)
    {
        _textToSpeech.Voice = VoiceType.en_US_Allison;
        if (!_textToSpeech.ToSpeech(OnSynthesize, OnFail, response, true))
            Log.Debug("ExampleTextToSpeech.ToSpeech()", "Failed to synthesize!");
    }

    private void OnSynthesize(AudioClip clip, Dictionary<string, object> customData)
    {
        PlayClip(clip);
    }

    private void PlayClip(AudioClip clip)
    {
        if (Application.isPlaying && clip != null)
        {
            GameObject audioObject = new GameObject("AudioObject");
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.spatialBlend = 0.0f;
            source.loop = false;
            source.clip = clip;
            source.Play();

            Destroy(audioObject, clip.length);
        }
    }

    //NLU
    Parameters parameters = new Parameters()
    {
        text = "Dummy Text",
        return_analyzed_text = true,
        language = "en",
        features = new Features()
        {
            keywords = new KeywordsOptions()
            {
                limit = 50
            }
        }
    };

    private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        Log.Error("ExampleNaturalLanguageUnderstanding.OnFail()", "Error received: {0}", error.ToString());
    }

    private void Analyze()
    {
        if (!_naturalLanguageUnderstanding.Analyze(OnAnalyze, OnFail, parameters))
            Log.Debug("ExampleNaturalLanguageUnderstanding.Analyze()", "Failed to get models.");
    }
   
    private void OnAnalyze(AnalysisResults resp, Dictionary<string, object> customData)
    {
        var keywords = resp.keywords;
        nluResults = "";
        foreach (var keyword in keywords)
        {
            if (keyword.text.ToLower() != "watson") {
                nluResults += keyword.text;
            }
        }
        requestPolyAssets();
    }

    //Streaming SST
    public bool Active
    {
        get { return _speechToText.IsListening; }
        set
        {
            if (value && !_speechToText.IsListening)
            {
                _speechToText.DetectSilence = true;
                _speechToText.EnableWordConfidence = true;
                _speechToText.EnableTimestamps = true;
                _speechToText.SilenceThreshold = 0.01f;
                _speechToText.MaxAlternatives = 0;
                _speechToText.EnableInterimResults = true;
                _speechToText.OnError = OnError;
                _speechToText.InactivityTimeout = -1;
                _speechToText.ProfanityFilter = false;
                _speechToText.SmartFormatting = true;
                _speechToText.SpeakerLabels = false;
                _speechToText.WordAlternativesThreshold = null;
                _speechToText.StartListening(OnRecognize, OnRecognizeSpeaker);
            }
            else if (!value && _speechToText.IsListening)
            {
                _speechToText.StopListening();
            }
        }
    }

    private void StartRecording()
    {
        if (_recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            _recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    private void StopRecording()
    {
        if (_recordingRoutine != 0)
        {
            Microphone.End(_microphoneID);
            Runnable.Stop(_recordingRoutine);
            _recordingRoutine = 0;
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
    }

    private IEnumerator RecordingHandler()
    {
        Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
        _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
        yield return null;      // let _recordingRoutine get set..

        if (_recording == null)
        {
            StopRecording();
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = _recording.samples / 2;
        float[] samples = null;

        while (_recordingRoutine != 0 && _recording != null)
        {
            int writePos = Microphone.GetPosition(_microphoneID);
            if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
            {
                Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint)
              || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
                record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
                record.Clip.SetData(samples, 0);

                _speechToText.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio, 
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)_recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }

        }

        yield break;
    }

    private void OnRecognize(SpeechRecognitionEvent result)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    string text = string.Format("{0} ({1}, {2:0.00})\n", alt.transcript, res.final ? "Final" : "Interim", alt.confidence);
                    //Once Watson finshing Processing SST, it sets the userMessage
                    if (res.final) {
                        if (text.Split(new char[] { ' ' })[0].ToLower() == "watson")
                        {
                            userMessage = text;
                            newMessage = true;
                            Log.Debug("This is your message:", userMessage);
                        } 
                        else if (text.Split(new char[] { ' ' })[0].ToLower() == "restart") 
                        {
                            int scene = SceneManager.GetActiveScene().buildIndex;
                            SceneManager.LoadScene(scene, LoadSceneMode.Single);
                        }
                    }
                }

                if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                    }
                }

                if (res.word_alternatives != null)
                {
                    foreach (var wordAlternative in res.word_alternatives)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
                        foreach (var alternative in wordAlternative.alternatives)
                            Log.Debug("ExampleStreaming.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
                    }
                }
            }
        }
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result)
    {
        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
            {
                Log.Debug("ExampleStreaming.OnRecognize()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
    }
}
