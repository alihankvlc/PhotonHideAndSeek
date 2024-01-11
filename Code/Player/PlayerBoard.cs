using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBoard
{
    private UIPlayerBoard m_UIElement;
    private Dictionary<int, GameObject> m_PlayerBoardContentCache = new Dictionary<int, GameObject>();
    public PlayerBoard(int id, string name, int score, PlayerTeam team)
    {
        GameObject playerBoardContent = PhotonNetwork.Instantiate("PlayerBoardContent", Vector3.zero, Quaternion.identity);
        UIManager.OnSetPlayerBoardContentObjectTransform?.Invoke(playerBoardContent);
        UIPlayerBoard uiElement = playerBoardContent.GetComponent<UIPlayerBoard>();

        if (!m_PlayerBoardContentCache.ContainsKey(id)) m_PlayerBoardContentCache.Add(id, playerBoardContent);

        m_UIElement = uiElement;

        if (uiElement != null)
        {
            uiElement.UIPlayerId(id);
            uiElement.UIPlayerName(name);
            uiElement.UIPlayerScore(score);
            uiElement.UIPlayerTeam(team);
        }
    }
    public void RemoveContentCache(int id)
    {
        if (m_PlayerBoardContentCache.TryGetValue(id, out GameObject leavingPlayerContent))
            PhotonNetwork.Destroy(leavingPlayerContent);
    }

    public void SetUpdatePlayerMs(int ms)
    {
        m_UIElement.UIPlayerMs(ms);
    }
    public void SetUpdateScore(int score)
    {
        m_UIElement.UIPlayerScore(score);
    }
}
