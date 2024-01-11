using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Match Status Variables")]
    [SerializeField] private GameStatusEventType m_GameStatus;
    [SerializeField] private float m_MatchStartingDuration = 3f;

    [Header("Player Variables")]
    [SerializeField] private int m_ScoreLostOnDeath;
    [SerializeField] private int m_ScoreGainedOnKill;

    [Header("Other")]
    [SerializeField] private GameObject m_SeekerBaseDoor;

    private int m_SeekerPlayerCount;
    private int m_HiderPlayerCount;

    private const float SEKEER_BASE_DOOR_OPEN_ROT = -108f;
    private const float SEKEER_BASE_DOOR_CLOSE_ROT = 0f;

    private Dictionary<int, PlayerController> m_PlayerCache = new Dictionary<int, PlayerController>();
    private float m_MatchStartingTimer;

    private bool m_MatchStarted = false;
    private bool m_SeekerTeamPositionSet = false;

    public static Action<int, PlayerController> OnRegisterPlayerCache;
    public static Action<PlayerTeam> OnIncreasePlayerCount;
    public delegate int OnSetPlayerScoreDelegate(ScoreEventType type);
    public static OnSetPlayerScoreDelegate OnSetPlayerScore;
    private void Start()
    {
        OnRegisterPlayerCache += AddPlayerCache;
        OnIncreasePlayerCount += IncreasePlayerCount;
        OnSetPlayerScore += GetPlayerScore;

        if (PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(SyncInitialGameInfo), RpcTarget.All, m_SeekerPlayerCount, m_HiderPlayerCount);

        m_MatchStartingTimer = m_MatchStartingDuration;
    }
    private bool AreHiderPlayersWaiting() => m_SeekerPlayerCount > 0 && m_HiderPlayerCount < 1;
    private bool AreSeekerPlayersWaiting() => m_HiderPlayerCount > 0 && m_SeekerPlayerCount < 1;
    private bool IsMatchReadyToStart() => m_HiderPlayerCount > 0 && m_SeekerPlayerCount > 0;
    private void Update()
    {
        if (m_GameStatus != GameStatusEventType.MatchStarted)
            UIManager.OnUpdateGameStatus(m_GameStatus, m_MatchStartingTimer);

        switch (m_GameStatus)
        {
            case GameStatusEventType.Preparing:
                if (AreHiderPlayersWaiting())
                    photonView.RPC(nameof(SyncGameStatusInfo), RpcTarget.AllBuffered, GameStatusEventType.WaitingForHiderPlayer);
                else if (AreSeekerPlayersWaiting())
                    photonView.RPC(nameof(SyncGameStatusInfo), RpcTarget.AllBuffered, GameStatusEventType.WaitingForSeekerPlayer);
                break;
            case GameStatusEventType.WaitingForHiderPlayer:
            case GameStatusEventType.WaitingForSeekerPlayer:
                if (IsMatchReadyToStart())
                {
                    m_MatchStartingTimer = m_MatchStartingDuration;
                    m_SeekerTeamPositionSet = false;

                    photonView.RPC(nameof(SyncGameStatusInfo), RpcTarget.AllBuffered, GameStatusEventType.MatchStarting);
                }
                break;
            case GameStatusEventType.MatchStarting:
                photonView.RPC(nameof(NotifyMatchStarting), RpcTarget.AllBuffered);
                photonView.RPC(nameof(SeekerBaseDoorControl), RpcTarget.AllBuffered, false);
                CheckMatchRestarting();
                break;
            case GameStatusEventType.MatchStarted:
                CheckMatchRestarting();
                UIManager.OnShowGameStatusWindow?.Invoke(false);
                if (m_MatchStartingTimer <= 0)
                    photonView.RPC(nameof(SeekerBaseDoorControl), RpcTarget.AllBuffered, true);

                break;
        }
    }
    [PunRPC]
    private void NotifyMatchStarting()
    {
        m_MatchStartingTimer -= Time.deltaTime;
        UIManager.OnUpdateGameStatus(m_GameStatus, m_MatchStartingTimer);
        if (!m_SeekerTeamPositionSet)
        {
            SetSeekerTeamPosition();
            m_SeekerTeamPositionSet = true;
        }
        if (m_MatchStartingTimer <= 0)
        {
            photonView.RPC(nameof(SyncGameStatusInfo), RpcTarget.AllBuffered, GameStatusEventType.MatchStarted);
        }
    }
    [PunRPC]
    private void SyncGameStatusInfo(GameStatusEventType status)
    {
        m_GameStatus = status;
        m_MatchStarted = status == GameStatusEventType.MatchStarted;
    }
    private void CheckMatchRestarting()
    {
        if (AreHiderPlayersWaiting()) m_GameStatus = GameStatusEventType.WaitingForHiderPlayer;
        else if (AreSeekerPlayersWaiting()) m_GameStatus = GameStatusEventType.WaitingForSeekerPlayer;

        UIManager.OnShowGameStatusWindow?.Invoke(true);
    }
    private void OnDestroy()
    {
        OnRegisterPlayerCache -= AddPlayerCache;
        OnIncreasePlayerCount -= IncreasePlayerCount;
        OnSetPlayerScore -= GetPlayerScore;
    }
    private void SetSeekerTeamPosition()
    {
        foreach (Player seekerPlayer in PhotonNetwork.PlayerList)
        {
            if (m_PlayerCache.TryGetValue(seekerPlayer.ActorNumber, out PlayerController existingPlayer))
            {
                if (existingPlayer.GetPlayerTeam != PlayerTeam.Seeker) continue;

                GameObject playerObject = existingPlayer.gameObject;
                Transform spawnTransform = TeamManager.OnPlayerSpawnPoint?.Invoke(PlayerTeam.Seeker);
                playerObject.transform.position = spawnTransform.position;
            }
        }
    }
    public void AddPlayerCache(int id, PlayerController controller)
    {
        if (m_PlayerCache.ContainsKey(id)) return;
        m_PlayerCache.Add(id, controller);

        Debug.Log($"{controller.GetPlayerName}({id}) oyuncu {controller.GetPlayerTeam} takýmýna katýldý ve belleðe eklendi...");
        Debug.Log($"{m_PlayerCache.Count} bellekte ki oyuncu sayýsý...");

    }
    public void IncreasePlayerCount(PlayerTeam team)
    {
        UpdatePlayerCount(team, 1);
        photonView.RPC(nameof(SyncPlayerCounts), RpcTarget.AllBuffered, m_SeekerPlayerCount, m_HiderPlayerCount);
    }

    private void DecreasePlayerCount(PlayerTeam team)
    {
        UpdatePlayerCount(team, -1);
        photonView.RPC(nameof(SyncPlayerCounts), RpcTarget.AllBuffered, m_SeekerPlayerCount, m_HiderPlayerCount);
    }
    private void UpdatePlayerCount(PlayerTeam team, int value)
    {
        switch (team)
        {
            case PlayerTeam.Hider:
                m_HiderPlayerCount += value;
                break;
            case PlayerTeam.Seeker:
                m_SeekerPlayerCount += value;
                break;
        }
    }
    [PunRPC]
    private void SyncInitialGameInfo(int initialSeekerPlayerCount, int initialHiderPlayerCount)
    {
        m_SeekerPlayerCount = initialSeekerPlayerCount;
        m_HiderPlayerCount = initialHiderPlayerCount;
    }
    [PunRPC]
    private void SyncPlayerCounts(int syncedSeekerPlayerCount, int syncedHiderPlayerCount)
    {
        m_SeekerPlayerCount = syncedSeekerPlayerCount;
        m_HiderPlayerCount = syncedHiderPlayerCount;

        UIManager.OnUpdateHiderCount?.Invoke(syncedHiderPlayerCount);
        UIManager.OnUpdateSeekerPlayerCount?.Invoke(syncedSeekerPlayerCount);
    }
    [PunRPC]
    private void SeekerBaseDoorControl(bool param)
    {
        Vector3 targetRotation = new Vector3(0.0f, param ? SEKEER_BASE_DOOR_OPEN_ROT
            : SEKEER_BASE_DOOR_CLOSE_ROT, 0.0f);
        m_SeekerBaseDoor.transform.DORotate(targetRotation, 1f, RotateMode.Fast)
            .SetEase(Ease.Linear);
    }
    private int GetPlayerScore(ScoreEventType type)
    {
        int baseScore = 0;

        switch (type)
        {
            case ScoreEventType.Kill:
                baseScore = m_ScoreGainedOnKill;
                break;
            case ScoreEventType.Death:
                baseScore = m_ScoreLostOnDeath;
                break;
            case ScoreEventType.Win:
                baseScore = m_ScoreGainedOnKill;
                break;
            case ScoreEventType.Lose:
                baseScore = m_ScoreLostOnDeath;
                break;
        }

        return CalculateModifiedScore(baseScore, type);
    }

    private int CalculateModifiedScore(int baseScore, ScoreEventType type)
    {
        int multiplier = (type == ScoreEventType.Win || type == ScoreEventType.Lose) ? 2 : 1;
        return baseScore * multiplier;
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (m_PlayerCache.TryGetValue(otherPlayer.ActorNumber, out PlayerController leavingPlayer))
        {
            DecreasePlayerCount(leavingPlayer.GetPlayerTeam);
            m_PlayerCache.Remove(otherPlayer.ActorNumber);

            Debug.Log($"{otherPlayer.NickName}({otherPlayer.ActorNumber} {leavingPlayer.GetPlayerTeam} takýmýnda ki oyuncu" +
                $"oyundan ayrýldý ve bellekten silindi...)");

            Debug.Log($"{m_PlayerCache.Count} bellekte ki oyuncu sayýsý...");

            leavingPlayer.UI_RemovePlayerContent();
        }
    }
}
