using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RiotAPIManager : MonoBehaviour
{

    [Header("Account Display Fields")]
    public Text SummonerName_DisplayField;
    public Text SummonerID_DisplayField;
    public Text SummonerID_ShieldField;

    [Header("API Display Fields")]
    public Text RiotAPI_DisplayField;
    public Text RiotAPI_ShieldField;

    [Header("Match Display Fields")]
    public Text MatchID_DisplayField;
    public Text ParticpantIDField;

    [Header("Live Transcription Fields")]
    public Text transcriptionField;
    public Text statusFeed;

    public GameObject liveStatusFeedback;
    public bool SummonerNameUpdateFlag = false;
    public GameObject startRecordingButton;

    [SerializeField]
    private string summonerName;
    [SerializeField]
    private string summonerId = null;
    [SerializeField]
    private long matchId; //match id of the most recent match
    [SerializeField]
    private int mostRecentParticipantId;

    public string API_KEY = "RGAPI-cdbf8b99-2007-4565-89d8-142a61a9af7f";
    private const string API_URI = "https://na1.api.riotgames.com";
    private const string SUMMONERV4_ENDPOINT = "https://na1.api.riotgames.com/lol/summoner/v4/summoners";

    private string mostRecentRequest;
    private bool requestReturned = false;
    private bool isInLiveGame = false;
    public bool recordingFlag = true;
    private string status;

    public DateTime gameStartTime;
    [SerializeField]
    public Dictionary<DateTime, string> timelineDictionary = new Dictionary<DateTime, string>();

    public static DateTime FromUnixTime(long unixTime)
    {
        return epoch.AddMilliseconds(unixTime);
    }
    private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void AppendtoTranscriptFeed(string text)
    {
        transcriptionField.text = transcriptionField.text + text;
    }

    IEnumerator GetRequest(string uri)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError)
        {
            Debug.LogError("ERROR WHILE SENDING: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            mostRecentRequest = uwr.downloadHandler.text;
            Debug.Log("Most Recent Request = " + mostRecentRequest);
        }
    }


    public void GetSummonerByName()
    {
        StartCoroutine(GetSummonerByNameRequest(summonerName));
        
    }

    IEnumerator GetSummonerByNameRequest(string name)
    {
        string uri = SUMMONERV4_ENDPOINT + "/by-name/" + name + "?api_key=" + API_KEY;
        Debug.Log(uri);
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        Summoner summoner = new Summoner();
        summoner = JsonUtility.FromJson<Summoner>(uwr.downloadHandler.text);
        SummonerID_DisplayField.text = summoner.id;
        summonerId = summoner.id;
        SummonerNameUpdateFlag = true;
    }

    IEnumerator GetParticipantIDByMatchID(long matchID)
    {
        string uri = API_URI + "/lol/match/v4/matches/" + matchID.ToString() + "?api_key=" + API_KEY; ;
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        Debug.Log(uwr);
        Match.MatchDto match = new Match.MatchDto();
        match = JsonUtility.FromJson<Match.MatchDto>(uwr.downloadHandler.text);
        Debug.Log(match);
        if (uwr.responseCode == 404)
        {
            Debug.LogError("404!");
        }
        if (uwr.responseCode == 200)
        {
            Debug.Log("FETCHING PARTICIPANT ID");
            for (int i = 0; i < match.participantIdentities.Count; i++)
            {
                if (match.participantIdentities[i].player.summonerName == summonerName)
                {
                    mostRecentParticipantId = match.participantIdentities[i].participantId;
                }
            }
        }
        ParticpantIDField.text = mostRecentParticipantId.ToString();
        Debug.Log("Exiting");
    }

    IEnumerator GetGameStartByMatchID(long matchID)
    {
        string uri = API_URI + "/lol/match/v4/matches/" + matchID.ToString() + "?api_key=" + API_KEY; ;
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        Debug.Log(uwr);
        Match.MatchDto match = new Match.MatchDto();
        match = JsonUtility.FromJson<Match.MatchDto>(uwr.downloadHandler.text);
        Debug.Log(match);
        if (uwr.responseCode == 404)
        {
            Debug.LogError("404!");
        }
        if (uwr.responseCode == 200)
        {
            Debug.Log("FETCHING GAME START TIME");
            gameStartTime = FromUnixTime(match.gameCreation);
            Debug.Log("Game Start Time : " + gameStartTime.Month.ToString() + "/" + gameStartTime.Day.ToString() + " , " + string.Format("{0:hh:mm:ss tt}", gameStartTime));
            if (!timelineDictionary.ContainsKey(gameStartTime))
            {
                timelineDictionary.Add(gameStartTime, "\n - | | | | GAME START TIME : " + gameStartTime.Month.ToString() + "/" + gameStartTime.Day.ToString() + " , " + string.Format("{0:hh:mm:ss tt}", gameStartTime) + " | | | |  - MATCH ID - " + matchId + " - |");
            }
            else
            {
                string prev;
                prev = timelineDictionary[gameStartTime];
                string newEntry = status + "\n" + prev;
                timelineDictionary[gameStartTime] = newEntry;
            }
        }
    }


    //Called after button pressed to start recording audio sessions
    public void StartCheckingForLiveGame()
    {
        if (!isInLiveGame && summonerId != null)
        {
            //start coroutine that checks api for live game status
            StartCoroutine(GetLiveGameInfoByID(summonerId));
        } else
        {
            Debug.Log("ERROR: Make sure you initialize your player data first. Must obtain Summoner ID");
        }
    }

    IEnumerator GetLiveGameInfoByID(string summonerId)
    {
        string uri = API_URI + "/lol/spectator/v4/active-games/by-summoner/" + summonerId + "?api_key=" + API_KEY;
        Debug.Log("Checking for live game...");
        //statusFeed.text = "Checking for live game...";
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.responseCode == 404)
        {
            //there is no active game
            StartCoroutine(CheckContinuousLiveGameStatus());
        } 
        if (uwr.responseCode == 200)
        {
            Debug.Log("Live game found...");
            //statusFeed.text = "Live game found...";
            //there is active game and request was a success
            isInLiveGame = true;
            liveStatusFeedback.SetActive(true);
            Spectator.CurrentGameInfo spec = new Spectator.CurrentGameInfo();
            spec = JsonUtility.FromJson<Spectator.CurrentGameInfo>(uwr.downloadHandler.text);
            matchId = spec.gameId;
            MatchID_DisplayField.text = matchId.ToString();
            gameStartTime = DateTime.UtcNow;
            status = "(" + string.Format("{0:hh:mm:ss tt}", gameStartTime) +") -  | | | | LEAGUE MATCH STARTED | | | | - MATCH ID - " + matchId + " - |";
            AppendtoTranscriptFeed(status);
            if (!timelineDictionary.ContainsKey(gameStartTime))
            {
                timelineDictionary.Add(gameStartTime, status);
            } else
            {
                string prev;
                prev = timelineDictionary[gameStartTime];
                string newEntry = status + "\n" + prev;
                timelineDictionary[gameStartTime] = newEntry;
            }
            
            StartCoroutine(CheckForEndLiveGameStatus());
        }
    }

    IEnumerator GetTimeLineByMatchID(long matchID)
    {
        StartCoroutine(GetParticipantIDByMatchID(matchID));
        string uri = API_URI + "/lol/match/v4/timelines/by-match/" + matchID.ToString() + "?api_key=" + API_KEY;
        Debug.Log("Fetching Timeline data...");
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.responseCode == 404)
        {
            Debug.LogError("404 - Timeline not found");
        }
        if (uwr.responseCode == 200)
        {
            //timeline was retrieved
            MatchTimeline.MatchTimelineDto timeline = new MatchTimeline.MatchTimelineDto();
            timeline = JsonUtility.FromJson<MatchTimeline.MatchTimelineDto>(uwr.downloadHandler.text);
            GetGameStartTimeByMatchID();

            for (int i = 0; i < timeline.frames.Count; i++)
            {
                for (int k = 0; k < timeline.frames[i].events.Count; k++)
                {
                    //Every Event Type Just Spit out in Case I Forgot one
                    if (timeline.frames[i].events[k].participantId == mostRecentParticipantId)
                    {
                        string eventData;
                        TimeSpan t = TimeSpan.FromMilliseconds(timeline.frames[i].events[k].timestamp);
                        DateTime timelineAdjust = gameStartTime + t;
                        string timeStamp = "(" + string.Format("{0:hh:mm:ss}", timelineAdjust) + ")";
                        string eventType;
                        eventType = timeline.frames[i].events[k].type;
                        eventData = timeStamp + " - " + summonerName + " " + eventType;
                        AppendtoTranscriptFeed("\n" + eventData);
                        DateTime gameTime = gameStartTime + t;
                        if (!timelineDictionary.ContainsKey(gameTime))
                        {
                            timelineDictionary.Add(gameTime, eventData);
                        }
                        else
                        {
                            string prev;
                            prev = timelineDictionary[gameTime];
                            prev = prev + ", " + eventData;
                            timelineDictionary[gameTime] = prev;
                        }
                    }
                    //Even Type Specifically for when you kill an enemy champion
                    if (timeline.frames[i].events[k].type == "CHAMPION_KILL" && timeline.frames[i].events[k].killerId == mostRecentParticipantId)
                    {
                        string eventData;
                        TimeSpan t = TimeSpan.FromMilliseconds(timeline.frames[i].events[k].timestamp);
                        DateTime timelineAdjust = gameStartTime + t;
                        string timeStamp = "(" + string.Format("{0:hh:mm:ss}", timelineAdjust) + ")";
                        eventData = timeStamp + " - " + summonerName + " killed an Enemy Champion!";
                        AppendtoTranscriptFeed("\n" + eventData);
                        DateTime gameTime = gameStartTime + t;
                        if (!timelineDictionary.ContainsKey(gameTime))
                        {
                            timelineDictionary.Add(gameTime, eventData);
                        }
                        else
                        {
                            string prev;
                            prev = timelineDictionary[gameTime];
                            prev = prev + ", " + eventData;
                            timelineDictionary[gameTime] = prev;
                        }
                    }
                    //Event Type specifically for when you are killed by an enemy champion
                    if (timeline.frames[i].events[k].type == "CHAMPION_KILL" && timeline.frames[i].events[k].victimId == mostRecentParticipantId)
                    {
                        string eventData;
                        TimeSpan t = TimeSpan.FromMilliseconds(timeline.frames[i].events[k].timestamp);
                        DateTime timelineAdjust = gameStartTime + t;
                        string timeStamp = "(" + string.Format("{0:hh:mm:ss}", timelineAdjust) + ")";
                        eventData = timeStamp + " - " + summonerName + " was killed by an Enemy Champion!";
                        AppendtoTranscriptFeed("\n" + eventData);
                        DateTime gameTime = gameStartTime + t;
                        if (!timelineDictionary.ContainsKey(gameTime))
                        {
                            timelineDictionary.Add(gameTime, eventData);
                        }
                        else
                        {
                            string prev;
                            prev = timelineDictionary[gameTime];
                            prev = prev + ", " + eventData;
                            timelineDictionary[gameTime] = prev;
                        }
                    }
                    //Event Type Specifically for whn you assist in a kill of another champion
                    if (timeline.frames[i].events[k].type == "CHAMPION_KILL" && timeline.frames[i].events[k].assistingParticipantIds.Contains(mostRecentParticipantId))
                    {
                        string eventData;
                        TimeSpan t = TimeSpan.FromMilliseconds(timeline.frames[i].events[k].timestamp);
                        DateTime timelineAdjust = gameStartTime + t;
                        string timeStamp = "(" + string.Format("{0:hh:mm:ss}", timelineAdjust) + ")";
                        eventData = timeStamp + " - " + summonerName + " assisted in killing an Enemy Champion!";
                        AppendtoTranscriptFeed("\n" + eventData);
                        DateTime gameTime = gameStartTime + t;
                        if (!timelineDictionary.ContainsKey(gameTime))
                        {
                            timelineDictionary.Add(gameTime, eventData);
                        } else
                        {
                            string prev;
                            prev = timelineDictionary[gameTime];
                            prev = prev + ", " + eventData;
                            timelineDictionary[gameTime] = prev;
                        }
                    }
                    //Event Type if you Kill an enemy structure
                    if (timeline.frames[i].events[k].type == "BUILDING_KILL" && timeline.frames[i].events[k].killerId == mostRecentParticipantId)
                    {
                        string eventData;
                        TimeSpan t = TimeSpan.FromMilliseconds(timeline.frames[i].events[k].timestamp);
                        DateTime timelineAdjust = gameStartTime + t;
                        string timeStamp = "(" + string.Format("{0:hh:mm:ss}", timelineAdjust) + ")";
                        eventData = timeStamp + " - You destroyed " + timeline.frames[i].events[k].laneType + " " + timeline.frames[i].events[k].towerType + " " + timeline.frames[i].events[k].buildingType;
                        AppendtoTranscriptFeed("\n" + eventData);
                        DateTime gameTime = gameStartTime + t;
                        if (!timelineDictionary.ContainsKey(gameTime))
                        {
                            timelineDictionary.Add(gameTime, eventData);
                        }
                        else
                        {
                            string prev;
                            prev = timelineDictionary[gameTime];
                            prev = prev + ", " + eventData;
                            timelineDictionary[gameTime] = prev;
                        }


                    }
                    //Event Type if you Assist in Killing an Enemy Structure
                    if (timeline.frames[i].events[k].type == "BUILDING_KILL" && timeline.frames[i].events[k].assistingParticipantIds.Contains(mostRecentParticipantId))
                    {
                        string eventData;
                        TimeSpan t = TimeSpan.FromMilliseconds(timeline.frames[i].events[k].timestamp);
                        DateTime timelineAdjust = gameStartTime + t;
                        string timeStamp = "(" + string.Format("{0:hh:mm:ss}", timelineAdjust) + ")";
                        eventData = timeStamp + " - You assisted in destroying " + timeline.frames[i].events[k].laneType + " " + timeline.frames[i].events[k].towerType + " " + timeline.frames[i].events[k].buildingType;
                        AppendtoTranscriptFeed("\n" + eventData);
                        DateTime gameTime = gameStartTime + t;
                        if (!timelineDictionary.ContainsKey(gameTime))
                        {
                            timelineDictionary.Add(gameTime, eventData);
                        }
                        else
                        {
                            string prev;
                            prev = timelineDictionary[gameTime];
                            prev = prev + ", " + eventData;
                            timelineDictionary[gameTime] = prev;
                        }
                    }
                    //Event type if you ass
                }
            }
        }

    }

    IEnumerator CheckContinuousLiveGameStatus()
    {
        yield return new WaitForSeconds(10);

        if (!isInLiveGame)
        {
            StartCoroutine(GetLiveGameInfoByID(summonerId));
        }
        else
        {
            StartCoroutine(CheckForEndLiveGameStatus());
        }
        
    }

    IEnumerator CheckForEndLiveGameStatus()
    {
        Debug.Log("Checking for end of live game...");
        yield return new WaitForSeconds(30);
        string uri = API_URI + "/lol/spectator/v4/active-games/by-summoner/" + summonerId + "?api_key=" + API_KEY;
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();
        if (uwr.responseCode == 404)
        {
            isInLiveGame = false;
            liveStatusFeedback.SetActive(false);
            DateTime timeStamp = DateTime.UtcNow;
            status = "(" + string.Format("{0:hh:mm:ss}", timeStamp) + ") -  | | | | LEAGUE MATCH ENDED | | | | - - |";
            AppendtoTranscriptFeed(status);
            if (!timelineDictionary.ContainsKey(timeStamp))
            {
                timelineDictionary.Add(timeStamp, status);
            }
            else
            {
                string prev;
                prev = timelineDictionary[timeStamp];
                string newEntry = status + "\n" + prev;
                timelineDictionary[timeStamp] = newEntry;
            }
            
        }
        if (uwr.responseCode == 200)
        {
            StartCoroutine(CheckForEndLiveGameStatus());
        }

    }

    private void Start()
    {
        liveStatusFeedback.SetActive(false);
        RiotAPI_DisplayField.text = API_KEY;
        RiotAPI_DisplayField.gameObject.SetActive(false);
    }

    private void Awake()
    {
    }
    // Update is called once per frame
    void Update()
    {

        if (SummonerNameUpdateFlag && summonerId != null)
        {
            startRecordingButton.gameObject.SetActive(true);
            SummonerID_ShieldField.text = "";
            foreach (char i in summonerId)
            {
                SummonerID_ShieldField.text += "•";
            }
        }
            

    }

    public void UpdateSummonerName(string name)
    {
        summonerName = name;
        SummonerName_DisplayField.text = summonerName;

    }

    public void UpdateCustomMatchID(string newMatchId)
    {
        long newLong = long.Parse(newMatchId);
        matchId = newLong;
        MatchID_DisplayField.text = newLong.ToString();
    }

    public void UpdateAPIKey(InputField field)
    {
        API_KEY = field.text;
        RiotAPI_DisplayField.text = API_KEY;
        RiotAPI_ShieldField.text = "";

        foreach(char i in API_KEY)
        {
            RiotAPI_ShieldField.text += "•";
        }

    }

    public void GetParticipantID()
    {
        StartCoroutine(GetParticipantIDByMatchID(matchId));
    }

    public void GetTimelineData()
    {
        StartCoroutine(GetTimeLineByMatchID(matchId));
    }

    public void GetGameStartTimeByMatchID()
    {
        StartCoroutine(GetGameStartByMatchID(matchId));
    }

    public void Debug_DisplayTransciptionDictionary()
    {
        foreach(KeyValuePair<DateTime, string> i in timelineDictionary)
        {
            Debug.LogFormat("({0}) : {1}", i.Key, i.Value);
        }
    }

    public void ClearTranscriptionDictionary()
    {
        timelineDictionary.Clear();
    }

    public string GetSummonerName()
    {
        return summonerName;
    }

}
