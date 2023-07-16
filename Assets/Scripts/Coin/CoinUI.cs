using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CoinUI : MonoBehaviour
{
    public TextMeshProUGUI[] _playerCoinsCountsText;

    public static CoinUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateCoinCountText(Dictionary<int, int> playerCoinCounts)
    {
        foreach (var playerCoinCount in playerCoinCounts)
        {
            int playerId = playerCoinCount.Key;
            int coinCount = playerCoinCount.Value;
            _playerCoinsCountsText[playerId - 1].text = $"Player {playerId}: {coinCount} coins";
        }
    }
}
