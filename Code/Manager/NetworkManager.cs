using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        Debug.Log("<color=green>Suncuuya baðlandý...</color>");
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("<color=green>Lobiye girildi...</color>");

        if (PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InRoom)
            PhotonNetwork.JoinOrCreateRoom("Room1", new RoomOptions { IsOpen = true, IsVisible = true, MaxPlayers = 5 }, TypedLobby.Default);
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("<color=green>Odaya girildi...</color>");
    }

}
