using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MatchTimeline : MonoBehaviour
{
    [System.Serializable]
    public class MatchTimelineDto
    {
        public List<MatchFrameDto> frames;
        public long frameInterval;
    }

    [System.Serializable]
    public class MatchFrameDto
    {
        public long timestamp;
        public Dictionary<string, MatchParticipantFrameDto> participantFrames;
        public List<MatchEventDto> events;
    }

    [System.Serializable]
    public class MatchParticipantFrameDto
    {
        public int totalGold;
        public int teamScore;
        public int participantId;
        public int level;
        public int currentGold;
        public int minionsKilled;
        public int dominionScore;
        public MatchPositionDto position;
        public int xp;
        public int jungleMinionsKilled;
    }

    [System.Serializable]
    public class MatchPositionDto
    {
        public int y;
        public int x;

    }

    [System.Serializable]
    public class MatchEventDto
    {
        public string eventType;
        public string towerType;
        public int teamId;
        public string ascendedType;
        public int killerId;
        public string levelUpType;
        public string pointCaptured;
        public List<int> assistingParticipantIds;
        public string wardType;
        public string monsterType;
        public string type; // acceptable values CHAMPION_KILL, WARD_PLACED, WARD_KILL, BUILDING_KILL, ELITE_MONSTER_KILL, ITEM_PURCHASED, ITEM_SOLD, ITEM_DESTROYED, ITEM_UNDO, SKILL_LEVEL_UP, ASCENDED_EVENT, CAPTURE_POINT, PORO_KING_SUMMON
        public int skillSlot;
        public int victimId;
        public long timestamp;
        public int afterId;
        public string monsterSubType;
        public string laneType;
        public int itemId;
        public int participantId;
        public string buildingType;
        public int creatorId;
        public MatchPositionDto position;
        public int beforeId;

    }



}
