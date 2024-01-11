using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;


public class UIPlayerBoard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TMP_PlayerId;
    [SerializeField] private TextMeshProUGUI TMP_PlayerName;
    [SerializeField] private TextMeshProUGUI TMP_PlayerScore;
    [SerializeField] private TextMeshProUGUI TMP_PlayerTeam;
    [SerializeField] private TextMeshProUGUI TMP_PlayerMs;

    private const string TEAM_HIDER_NAME = "Hider";
    private const string TEAM_SEEKER_NAME = "Seeker";
    public void UIPlayerId(int id)
    {
        TMP_PlayerId.SetText(id.ToString());
    }
    public void UIPlayerName(string name)
    {
        TMP_PlayerName.SetText(name);
    }
    public void UIPlayerScore(int score)
    {
        TMP_PlayerScore.SetText(score.ToString());
    }
    public void UIPlayerTeam(PlayerTeam team)
    {
        TMP_PlayerTeam.SetText(team == PlayerTeam.Hider ? TEAM_HIDER_NAME : TEAM_SEEKER_NAME);
        TMP_PlayerTeam.color = team == PlayerTeam.Hider ? Color.cyan : Color.red;
    }
    public void UIPlayerMs(int ms)
    {
        TMP_PlayerMs.SetText(ms.ToString());
    }
}