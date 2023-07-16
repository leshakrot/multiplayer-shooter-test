using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class CoinManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private Dictionary<int, int> _playerCoinsCounts;

    public static event System.Action<Dictionary<int, int>> OnCoinCountChanged = delegate { };

    private void Start()
    {
        _playerCoinsCounts = new Dictionary<int, int>();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void CollectCoin(int playerId)
    {
        if (!_playerCoinsCounts.ContainsKey(playerId))
        {
            _playerCoinsCounts.Add(playerId, 0);
        }
        _playerCoinsCounts[playerId]++;
        OnCoinCountChanged?.Invoke(_playerCoinsCounts);
        photonView.RPC("UpdateCoinCounts", RpcTarget.MasterClient, _playerCoinsCounts);
        photonView.RPC("CollectCoin", RpcTarget.MasterClient, playerId);
    }

    public Dictionary<int, int> GetPlayerCoinCounts()
    {
        return _playerCoinsCounts;
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 1)
        {
            _playerCoinsCounts = (Dictionary<int, int>)photonEvent.CustomData;
            CoinUI.Instance.UpdateCoinCountText(_playerCoinsCounts);
        }
    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
