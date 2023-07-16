using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class LobbyConnector : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("Lobby");
    }
}
