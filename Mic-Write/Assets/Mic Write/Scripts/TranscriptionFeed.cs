using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;

public class TranscriptionFeed : MonoBehaviour
{

    public Text RecogizedText;
    public Text ErrorText;
    public Text TranscriptionField;
    public Text APIKey_DisplayField;
    public Text APIKey_ShieldField;
    public bool inGame;
    public RiotAPIManager riotManager;

    public string CognitiveServicesAPIKey = "db5128aba28e4225a3b49f42cd214337";
    private string SpeechServiceRegion = "westus";
    private string summonerName;

    //Variables for live messages on screen. Must be locked to avoid threading deadlocks since recognition events are raised in a separate thread
    private string recognizedString = "";
    private string errorString = "";
    private string finalString = "";
    private string oldFinalString = "";
    private System.Object threadLocker = new System.Object();

    public Dictionary<DateTime, string> transcriptionDictionary = new Dictionary<DateTime, string>();
    private Dictionary<DateTime, string> combinationDictionary = new Dictionary<DateTime, string>();

    private DateTime time;

    //Cognitive Services Objects
    private SpeechRecognizer recognizer;

    private bool micPermissionGranted = false;

    public void UpdateAPIKey(InputField field)
    {
        CognitiveServicesAPIKey = field.text;
        APIKey_DisplayField.text = CognitiveServicesAPIKey;
        APIKey_ShieldField.text = "";

        foreach (char i in CognitiveServicesAPIKey)
        {
            APIKey_ShieldField.text += "•";
        }

    }


    // Start is called before the first frame update
    void Start()
    {
        micPermissionGranted = true;
        APIKey_DisplayField.text = CognitiveServicesAPIKey;
        APIKey_DisplayField.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        ErrorText.text = errorString;
        RecogizedText.text = recognizedString;

        time = DateTime.UtcNow;

        if (finalString != oldFinalString)
        {
            AppendTranscriptText(finalString);
        }


        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug_DisplayTransciptionDictionary();
        }
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    CombineDictionaries(riotManager.timelineDictionary, transcriptionDictionary);
        //    Debug_DisplayCombinationDictionary();
        //}
        //if (Input.GetKeyDown(KeyCode.H))
        //{
        //    SortDictionary();
        //}

    }

    public void ClearTranscriptionLog()
    {
        riotManager.transcriptionField.text = "";
        riotManager.ClearTranscriptionDictionary();
        combinationDictionary.Clear();
        transcriptionDictionary.Clear();
    }

    public void Debug_DisplayTransciptionDictionary()
    {
        foreach (KeyValuePair<DateTime, string> i in transcriptionDictionary)
        {
            UnityEngine.Debug.LogFormat("({0}) : {1}", i.Key, i.Value);
        }
    }

    public void Debug_DisplayCombinationDictionary()
    {
        foreach (KeyValuePair<DateTime, string> i in combinationDictionary)
        {
            UnityEngine.Debug.LogFormat("({0}) : {1}", i.Key, i.Value);
        }
    }

    private void CombineDictionaries(Dictionary<DateTime, string> dic1, Dictionary<DateTime, string> dic2)
    {
        foreach(var entry in dic1)
        {
            if (dic2.ContainsKey(entry.Key))
            {
                string prev = dic1[entry.Key];
                string newEntry = prev + dic2[entry.Key];
                dic2.Remove(entry.Key);
            }
        }

        combinationDictionary = dic1.Concat(dic2).ToDictionary(e => e.Key, e => e.Value);
        
    }

    private Dictionary<DateTime, string> SortDictionary()
    {
        var list = combinationDictionary.Keys.ToList();
        list.Sort();
        Dictionary<DateTime, string> sortedCombinationDictionary = new Dictionary<DateTime, string>();

        foreach (var key in list)
        {
            sortedCombinationDictionary[key] = combinationDictionary[key];
        }

        return sortedCombinationDictionary;
    }

    public void SaveTranscriptionSession()
    {
        CombineDictionaries(riotManager.timelineDictionary, transcriptionDictionary);
        Dictionary<DateTime, string> sortedCombinationDictionary = SortDictionary();
        DateTime now = DateTime.UtcNow;
        string path = Application.dataPath + "Session_" + now.Month.ToString() + "_" + now.Day.ToString() + "_" + now.Year.ToString() + "_" + now.Hour.ToString() + "_" + now.Minute.ToString() + ".txt";
        using (StreamWriter file = new StreamWriter(path))
            foreach (var entry in sortedCombinationDictionary)
                file.WriteLine("{0}", entry.Value);
        riotManager.ClearTranscriptionDictionary();
        riotManager.transcriptionField.text = "";
        combinationDictionary.Clear();
        transcriptionDictionary.Clear();
        sortedCombinationDictionary.Clear();

    }

    public void ExportSessionToJSON()
    {
        CombineDictionaries(riotManager.timelineDictionary, transcriptionDictionary);
        Dictionary<DateTime, string> sortedCombinationDictionary = SortDictionary();
        //AppendTranscriptText("made a sorted dictionary");
        string json = JsonConvert.SerializeObject(sortedCombinationDictionary, Formatting.Indented);
        //AppendTranscriptText("made a json string");
        //string json = JsonUtility.ToJson(sortedCombinationDictionary);
        DateTime now = DateTime.UtcNow;
        //File.Create(Application.dataPath + "_JSON_Session_" + now.Month.ToString() + "_" + now.Day.ToString() + "_" + now.Year.ToString() + "_" + now.Hour.ToString() + "_" + now.Minute.ToString() + ".json").Close();
        //File.WriteAllText(Application.dataPath +"_JSON_Session_" + now.Month.ToString() + "_" + now.Day.ToString() + "_" + now.Year.ToString() + "_" + now.Hour.ToString() + "_" + now.Minute.ToString() + ".json", json);
        string path = Application.dataPath + "_JSON_Session_" + now.Month.ToString() + "_" + now.Day.ToString() + "_" + now.Year.ToString() + "_" + now.Hour.ToString() + "_" + now.Minute.ToString() + ".txt";
        using (TextWriter writer = new StreamWriter(path))
        {
            writer.WriteLine(json);
            writer.Close();
        }
        //AppendTranscriptText("made a json file...");
        UnityEngine.Debug.Log("We made a json?");
    }


    public void UpdateSubscriptionKey(string key)
    {
        CognitiveServicesAPIKey = key;
    }

    public void UpdateServiceRegion(string region)
    {
        SpeechServiceRegion = region;
    }

    void AppendTranscriptText(string text)
    {
        string timeStampAndText = "\n(" + time.Hour.ToString() + ":" + time.Minute.ToString() + ":" + time.Second.ToString() + ") - " + text;
        TranscriptionField.text = TranscriptionField.text + timeStampAndText;
        oldFinalString = finalString;
        transcriptionDictionary.Add(time, "(" + string.Format("{0:hh:mm:ss}", time ) + ") - " + summonerName + ": " + text);
    }

    //Method to attach to the button used to start continuous recognition

    public void StartContinuous()
    {
        errorString = "";
        if (micPermissionGranted)
        {
            StartContinuousRecognition();
        }
        else
        {
            recognizedString = "This app cannot function without access to the microphone.";
            errorString = "ERROR: Microphone access denied.";
            UnityEngine.Debug.LogFormat(errorString);
        }
    }

    //Method to create Recognizer using Azure credentials and hooks up lifecycle + recognition events
    void CreateSpeechRecognizer()
    {
        //Check for neglect of API key insertage.
        if (CognitiveServicesAPIKey.Length == 0 || CognitiveServicesAPIKey == "YourSubscriptionKey")
        {
            recognizedString = "You forgot to obtain Cognitive Services Speech credentials and insert them in this app." + Environment.NewLine + "Please add your Cognitive Services Subscription Key to the text field on the program.";
            errorString = "ERROR: Missing service credentials (API Key)";
            UnityEngine.Debug.LogFormat(errorString);
            return;
        }
        //Status and debug messages
        UnityEngine.Debug.LogFormat("Creating SpeechRecognizer.");
        recognizedString = "Initializing speech recognition, please wait...";

        if (recognizer == null)
        {
            SpeechConfig config = SpeechConfig.FromSubscription(CognitiveServicesAPIKey, SpeechServiceRegion);
            recognizer = new SpeechRecognizer(config);

            if (recognizer != null)
            {
                // Subscribe to speech events
                recognizer.Recognizing += RecognizingHandler;
                recognizer.Recognized += RecognizedHandler;
                recognizer.SpeechStartDetected += SpeechStartDetectedHandler;
                recognizer.SpeechEndDetected += SpeechEndDetectedHandler;
                recognizer.Canceled += CanceledHandler;
                recognizer.SessionStarted += SessionStartedHandler;
                recognizer.SessionStopped += SessionStoppedHandler;
            }

        }
        UnityEngine.Debug.LogFormat("CreateSpeechRecognizer exit");
    }

    // Initiate Continuous Speech Recognition from default microphone.

    private async void StartContinuousRecognition()
    {
        UnityEngine.Debug.LogFormat("Starting Continuous Speech Recognition.");
        CreateSpeechRecognizer();

        if (recognizer != null)
        {
            UnityEngine.Debug.LogFormat("Starting SpeechRecognizer.");
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

            recognizedString = "Speech Recognizer is now running.";
        }
        UnityEngine.Debug.LogFormat("Start Continuous Speech Recognition exit");
    }

    private void OnDisable()
    {
        StopRecognition();
    }

    public async void StopRecognition()
    {
        if (recognizer != null)
        {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            recognizer.Recognizing -= RecognizingHandler;
            recognizer.Recognized -= RecognizedHandler;
            recognizer.SpeechStartDetected -= SpeechStartDetectedHandler;
            recognizer.SpeechEndDetected -= SpeechEndDetectedHandler;
            recognizer.Canceled -= CanceledHandler;
            recognizer.SessionStarted -= SessionStartedHandler;
            recognizer.SessionStopped -= SessionStoppedHandler;
            recognizer.Dispose();
            recognizer = null;
            recognizedString = "SpeechRecognizer is now stopped.";
            UnityEngine.Debug.LogFormat("Speech Recognizer is now stopped.");
        }
    }



    #region Speech Recognition Event Handlers

    private void SessionStartedHandler(object sender, SessionEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"\n    Session Started event. Event: {e.ToString()}.");
    }

    private void SessionStoppedHandler(object sender, SessionEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"\n    Session Event. Event: {e.ToString()}");
        UnityEngine.Debug.LogFormat($"Session Stop Detected. Stop the recognition.");
    }

    private void SpeechStartDetectedHandler(object sender, RecognitionEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"SpeechStartDetected received: offset: {e.Offset}.");
    }

    private void SpeechEndDetectedHandler(object sender, RecognitionEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"SpeechEndDetected received: offset: {e.Offset}.");
        UnityEngine.Debug.LogFormat($"Speech end detected.");
    }

    // "Recognizing" events are fired every time cognitive services recieves interim results during active recognition. (This is their guesses between transcriptions)
    private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizingSpeech)
        {
            UnityEngine.Debug.LogFormat($"HYPOTHESIS: Text={e.Result.Text}");
            lock (threadLocker)
            {
                recognizedString = $"HYPOTHESIS: {Environment.NewLine}{e.Result.Text}";
            }
        }
    }

    // "Recognized" events are fired when an utterance end was detected by cognitive services
    private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizedSpeech)
        {
            UnityEngine.Debug.LogFormat($"RECOGNIZED : Text+{e.Result.Text}");
            lock (threadLocker)
            {
                recognizedString = $"RESULT: {Environment.NewLine}{e.Result.Text}";
                finalString = e.Result.Text;
            }
            
        } else if (e.Result.Reason == ResultReason.NoMatch)
        {
            UnityEngine.Debug.LogFormat($"NOMATCH: Speech could not be recognized.");
        }
    }

    // "Canceled" Events are fired if the server encounters and error of some kind. Usually, invalid subscription credentials, update API key. 
    private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"CANCELED: Reason={e.Reason}");

        errorString = e.ToString();
        if (e.Reason == CancellationReason.Error)
        {
            UnityEngine.Debug.LogFormat($"CANCELED: ErrorDetails={e.ErrorDetails}");
            UnityEngine.Debug.LogFormat($"CANCELED: Did you update your API key?");
        }
    }

    #endregion


}
