using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Spectator
{ 
    [System.Serializable]
    public class CurrentGameInfo
    {
        public long gameId;
        public long gameStartTime;
        public string platformId;
        public string gameMode;
        public long mapId;
        public string gameType;
        public List<BannedChampion> bannedChampions;
        public Observer observers;
        public List<CurrentGameParticipant> participants;
        public long gameLength;
        public long gameQueueConfigId;
    }

    [System.Serializable]
    public class BannedChampion
    {
        public int pickTurn;
        public long championId;
        public long teamId;
    }

    [System.Serializable]
    public class Observer
    {
        public string encryptionKey;
    }

    [System.Serializable]
    public class CurrentGameParticipant
    {
        public long profileIconId;
        public long championId;
        public string summonerName;
        public List<GameCustomizationObject> gameCustomizationObjects;
        public bool bot;
        public Perks perks;
        public long spell2Id;
        public long teamId;
        public long spell1Id;
        public string summonerId;
    }

    public class GameCustomizationObject
    {
        public string category;
        public string content;
    }

    [System.Serializable]
    public class Perks
    {
        public long perkStyle;
        public List<long> perkIds;
        public long perSubStyle;
    }


}
