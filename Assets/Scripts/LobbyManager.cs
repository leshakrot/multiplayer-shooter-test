using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI _logText;
    [SerializeField] private TMP_InputField _createRoomInput;
    [SerializeField] private TMP_InputField _joinRoomInput;
    [SerializeField] private TMP_InputField _playerNameInput;

    private void Start()
    {
        _playerNameInput.text = PlayerPrefs.GetString("name");

        PhotonNetwork.NickName = _playerNameInput.text;
        _logText.text += "Player's name is set to " + PhotonNetwork.NickName + "\n";

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        _logText.text += "Connected to Master\n";
    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(_createRoomInput.text, roomOptions);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(_joinRoomInput.text);
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        _logText.text += "Joined the room\n";
        PhotonNetwork.LoadLevel("Game");
    }

    public void SaveName()
    {
        PlayerPrefs.SetString("name", _playerNameInput.text);
        PhotonNetwork.NickName = _playerNameInput.text;
        _logText.text += "Player's name is set to " + PhotonNetwork.NickName + "\n";
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Lobby");
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }
}
