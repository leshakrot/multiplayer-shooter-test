using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private GameObject _popupPrefab;
    [SerializeField] private TextMeshProUGUI _winnerNameText;
    [SerializeField] private TextMeshProUGUI _coinsCollectedText;
    [SerializeField] private float _coinSpawnInterval = 5f;

    private PhotonView _photonView;
    private Dictionary<PhotonView, PlayerController> _alivePlayers = new Dictionary<PhotonView, PlayerController>();
    private bool _isGameOver;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayer();

            if(PhotonNetwork.PlayerList.Length >= 2)
                InvokeRepeating(nameof(SpawnCoin), 0f, _coinSpawnInterval);
        }
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPosition = GetRandomPlayerSpawnPosition();
        PhotonNetwork.Instantiate(_playerPrefab.name, spawnPosition, Quaternion.identity);
    }

    private void SpawnCoin()
    {
        Vector3 spawnPosition = GetRandomCoinSpawnPosition();
        PhotonNetwork.Instantiate(_coinPrefab.name, spawnPosition, Quaternion.identity);
    }

    private Vector3 GetRandomPlayerSpawnPosition()
    {
        float playerSpawnRangeX = Camera.main.aspect * Camera.main.orthographicSize;
        float playerSpawnRangeY = Camera.main.orthographicSize;

        float randomX = Random.Range(-playerSpawnRangeX, playerSpawnRangeX);
        float randomY = Random.Range(-playerSpawnRangeY, playerSpawnRangeY);

        return new Vector3(randomX, randomY, 0f);
    }

    private Vector3 GetRandomCoinSpawnPosition()
    {
        float coinSpawnRangeX = Camera.main.aspect * Camera.main.orthographicSize;
        float coinSpawnRangeY = Camera.main.orthographicSize;

        float randomX = Random.Range(-coinSpawnRangeX, coinSpawnRangeX);
        float randomY = Random.Range(-coinSpawnRangeY, coinSpawnRangeY);

        Vector3 spawnPosition = new Vector3(randomX, randomY, 0f);
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(spawnPosition);

        if (screenPoint.x < 0.1f || screenPoint.x > 0.9f || screenPoint.y < 0.1f || screenPoint.y > 0.9f)
        {
            return GetRandomCoinSpawnPosition();
        }

        return spawnPosition;
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

    [PunRPC]
    private void CollectCoin(int playerId)
    {
        if (!_alivePlayers.ContainsKey(_photonView))
        {
            return;
        }

        PlayerController player = _alivePlayers[_photonView];
        player.CollectCoin(playerId);
    }

    [PunRPC]
    private void DestroyCoin(int coinViewId)
    {
        PhotonNetwork.Destroy(PhotonView.Find(coinViewId).gameObject);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 1)
        {
            object[] data = (object[])photonEvent.CustomData;
            int playerId = (int)data[0];

            _photonView.RPC("CollectCoin", RpcTarget.All, playerId);
        }
        else if (photonEvent.Code == 2)
        {
            object[] data = (object[])photonEvent.CustomData;
            int coinViewId = (int)data[0];

            _photonView.RPC("DestroyCoin", RpcTarget.All, coinViewId);
        }
    }

    [PunRPC]
    public void ShowWinnerPopup(string winnerName, int coinsCollected)
    {
        _winnerNameText.text = winnerName;
        _coinsCollectedText.text = coinsCollected.ToString();
        _popupPrefab.SetActive(true);
    }

    public void GameOver()
    {
        _isGameOver = true;

            PlayerController winnerPlayer = null;

            foreach (var player in _alivePlayers.Values)
            {
                if (winnerPlayer == null)
                {
                    winnerPlayer = player;
                }
            }
            Debug.Log("B");

            string winnerName = winnerPlayer != null ? winnerPlayer.GetPlayerName() : "No Winner";
            int coinsCollected = winnerPlayer != null ? winnerPlayer.GetPlayerCoinsCollected() : 0;

            _photonView.RPC("ShowWinnerPopup", RpcTarget.All, winnerName, coinsCollected);
    }

    public void RegisterPlayer(PhotonView playerView, PlayerController player)
    {
        _alivePlayers.Add(playerView, player);
    }

    public void UnregisterPlayer(PhotonView playerView)
    {
        if (_alivePlayers.ContainsKey(playerView))
        {
            _alivePlayers.Remove(playerView);
        }
    }
}
