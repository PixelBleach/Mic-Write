using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Match
{

    [System.Serializable]
    public class MatchDto
    {
        public int seasonId;
        public int queueId;
        public long gameId;
        public List<ParticipantIdentityDto> participantIdentities;
        public string gameVersion;
        public string platfromId;
        public string gameMode;
        public int mapId;
        public string gameType;
        public List<TeamStatsDto> teams;
        public List<ParticipantDto> participants;
        public long gameduration;
        public long gameCreation;
    }

    [System.Serializable]
    public class ParticipantIdentityDto
    {
        public PlayerDto player;
        public int participantId;
    }

    [System.Serializable]
    public class PlayerDto
    {
        public string currentPlatformId;
        public string summonerName;
        public string matchHistoryUri;
        public string platformId;
        public string currentAccountId;
        public int profileIcon;
        public string summonerId;
        public string accountId;
    }

    [System.Serializable]
    public class TeamStatsDto
    {
        public bool firstDragon;
        public bool firstInhibitor;
        public List<TeamBansDto> bans;
        public int baronKills;
        public bool firstRiftHerald;
        public bool firstBaron;
        public int riftHeraldKills;
        public bool firstBlood;
        public int teamId;
        public bool firstTower;
        public int vilemawKills;
        public int inhibitorKills;
        public int towerKills;
        public int dominionVictoryScore;
        public string win; //either "Fail" or "Win"
        public int dragonKills;

    }

    [System.Serializable]
    public class TeamBansDto
    {
        public int pickTurn;
        public int championId;

    }

    [System.Serializable]
    public class ParticipantDto
    {
        public ParticipantStatsDto stats;
        public int participantId;
        public List<RuneDto> runes;
        public ParticipantTimelineDto timeline;
        public int teamId;
        public int spell2Id;
        public List<MasteryDto> masteries;
        public string highestAchievedSeasonTier;
        public int spell1Id;
        public int championId;

    }

    [System.Serializable]
    public class ParticipantStatsDto
    {
        public bool firstBloodAssist;
        public long visionScore;
        public long magicDamageDealtToChampions;
        public long DamageDealtToObjectives;
        public int totalTimeCrowdControlDealt;
        public int longestTimeSpentLiving;
        public int perk1Var1;
        public int perk1Var3;
        public int perk1Var2;
        public int tripleKills;
        public int perk3Var3;
        public int nodeNeutralizeAssist;
        public int perk3Var2;
        public int playerScore9;
        public int playerScore8;
        public int kills;
        public int playerScore1;
        public int playerScore2;
        public int playerScore3;
        public int playerScore4;
        public int playerScore5;
        public int playerScore6;
        public int playerScore7;
        public int perk5Var1;
        public int perk5Var2;
        public int perk5Var3;
        public int totalScoreRank;
        public int neutralMinionsKilled;
        public long damageDealtToTurrets;
        public long physicalDamageDealtToChampions;
        public int nodeCapture;
        public int largestMultiKill;
        public int perk2Var2;
        public int perk2Var1;
        public int perk2Var3;
        public int totalUnitsHealed;
        public int perk4Var2;
        public int perk4Var1;
        public int perk4Var3;
        public int wardsKilled;
        public int largestCriticalStrike;
        public int largestKillingSpree;
        public int quadraKills;
        public int teamObjective;
        public long magicDamageDealt;
        public int item2;
        public int item3;
        public int item0;
        public int neutralMinionsKilledTeamJungle;
        public int item6;
        public int item4;
        public int item5;
        public int perk1;
        public int perk0;
        public int perk3;
        public int perk2;
        public int perk5;
        public int perk4;
        public int perk3Var1;
        public long damageSelfMitigated;
        public long magicalDamageTaken;
        public bool firstInhibitorKill;
        public long trueDamageTaken;
        public int nodeNeutralize;
        public int assists;
        public int combatPlayerScore;
        public int perkPrimaryStyle;
        public int goldSpent;
        public long trueDamageDealt;
        public int participantId;
        public long totalDamageTaken;
        public long physicalDamageDealt;
        public int sightWardsBoughtInGame;
        public long totalDamageDealtToChampions;
        public long physicalDamageTaken;
        public int totalPlayerScore;
        public bool win;
        public int objectivePlayerScore;
        public long totalDamageDealt;
        public int item1;
        public int neutralMinionsKilledEnemyJungle;
        public int deaths;
        public int wardsPlaced;
        public int perkSubStyle;
        public int turretKills;
        public bool firstBloodKill;
        public long trueDamageDealtToChampions;
        public int goldEarned;
        public int killingSprees;
        public int unrealKills;
        public int altarsCaptured;
        public bool firstTowerAssist;
        public bool firstTowerKill;
        public int champLevel;
        public int doubleKills;
        public int nodeCaptureAssist;
        public int ibhibitorKills;
        public bool firstInhibitorAssist;
        public int perk0Var1;
        public int perk0Var2;
        public int perk0Var3;
        public int visionWardsBoughtInGame;
        public int altarsNeutralized;
        public int pentaKills;
        public long totalHeal;
        public int totalMinionsKilled;
        public long timeCCingOthers;

    }

    [System.Serializable]
    public class RuneDto
    {
        public int runeId;
        public int rank;
    }

    [System.Serializable]
    public class ParticipantTimelineDto
    {
        public string lane;
        public int participantId;
        public Dictionary<string, double> csDiffPerMinDeltas;
        public Dictionary<string, double> goldPerMinDeltas;
        public Dictionary<string, double> xpDiffPerMinDeltas;
        public Dictionary<string, double> creepsPerMinDeltas;
        public Dictionary<string, double> xpPerMinDeltas;
        public string role;
        public Dictionary<string, double> damageTakenDiffPerMinDeltas;
        public Dictionary<string, double> damageTakenPerMinDeltas;
    }

    [System.Serializable]
    public class MasteryDto
    {
        public int masteryId;
        public int rank;
    }




}
