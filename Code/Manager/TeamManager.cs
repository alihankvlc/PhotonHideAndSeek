using Photon.Pun;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    [SerializeField] private Transform[] m_HiderSpawnPoints;
    [SerializeField] private Transform[] m_SeekerSpawnPoints;

    private const string HIDER_PREFAB_NAME = "Hider_Jock";
    private const string SEEKER_PREFAB_NAME = "Seeker_Butcher";

    public delegate void TeamSelectionDelegate(PlayerTeam team, ref bool isSelection);
    public static TeamSelectionDelegate OnTeamSelection;

    public delegate Transform GetPlayerSpawnPointDelegate(PlayerTeam team);
    public static GetPlayerSpawnPointDelegate OnPlayerSpawnPoint;

    private bool m_IsSelection;
    private void Start()
    {
        OnTeamSelection += DeterminePlayerTeam;
        OnPlayerSpawnPoint += DeterminePlayerSpawnPoint;
    }
    private void OnDestroy()
    {
        OnTeamSelection -= DeterminePlayerTeam;
        OnPlayerSpawnPoint -= DeterminePlayerSpawnPoint;
    }
    protected void DeterminePlayerTeam(PlayerTeam team, ref bool isSelection)
    {
        if (PhotonNetwork.InRoom && !isSelection)
            PlayerSpawn(team);
    }
    private void PlayerSpawn(PlayerTeam team)
    {
        if (UIManager.Instance.IsValidPlayerName() && !m_IsSelection)
        {
            string prefabName = (team == PlayerTeam.Hider) ? HIDER_PREFAB_NAME : SEEKER_PREFAB_NAME;

            GameObject playerObject = PhotonNetwork.Instantiate(prefabName, DeterminePlayerSpawnPoint(team).position, Quaternion.identity);

            m_IsSelection = true;

            UITeamMenu(false);
        }
    }
    private void UITeamMenu(bool param) => UIManager.OnShowTeamMenu(param);
    private Transform DeterminePlayerSpawnPoint(PlayerTeam team)
    {
        Transform[] transforms = team == PlayerTeam.Hider ? m_HiderSpawnPoints : m_SeekerSpawnPoints;
        int randomSpawnIndex = Random.Range(0, transforms.Length);
        return transforms[randomSpawnIndex];
    }
}