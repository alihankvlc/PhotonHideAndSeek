using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

[System.Serializable]
public class PlayerInfo
{
    [SerializeField] private GameObject m_UsernameParentObject;

    public string PlayerName;
    public PlayerTeam Team;
    public int PlayerMS;
    public int PlayerScore;
    public int PlayerID;

}
