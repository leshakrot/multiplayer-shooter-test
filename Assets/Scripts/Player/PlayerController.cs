using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine.Events;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float _speed = 4f;
    [SerializeField] private TextMeshProUGUI _playerNameText;

    private Dictionary<int, int> _playerCoinCounts = new Dictionary<int, int>();

    private PhotonView _photonView;
    private Vector2 _lastMovementDirection;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private Vector2 _minBound;
    private Vector2 _maxBound;
    private float _playerSize;

    private bool _isDead;

    public event Action<PlayerController> OnPlayerDeath;
    public event UnityAction<Vector2> OnMoveInput;
    public event UnityAction OnShootInput;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        _animator = GetComponent<Animator>();
        
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_photonView.IsMine)
        {
            _playerCoinCounts.Add(_photonView.OwnerActorNr, 0);
        }
    }

    private void Start()
    {
        _playerSize = GetComponent<CircleCollider2D>().bounds.extents.x;

        float cameraHeight = Camera.main.orthographicSize;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        _minBound = new Vector2(-cameraWidth + _playerSize, -cameraHeight + _playerSize);
        _maxBound = new Vector2(cameraWidth - _playerSize, cameraHeight - _playerSize);

        _playerNameText.text = _photonView.Owner.NickName;

        _isDead = false;

        if (_photonView.IsMine)
        {
            RegisterPlayer();
        }
    }

    private void Update()
    {
        if (!_photonView.IsMine || _isDead)
        {
            return;
        }

        float moveX = SimpleInput.GetAxis("Horizontal") * _speed * Time.deltaTime;
        float moveY = SimpleInput.GetAxis("Vertical") * _speed * Time.deltaTime;

        if (moveX > 0 || moveY > 0) _animator.SetBool("isRunning", true);
        else _animator.SetBool("isRunning", false);
        _photonView.RPC("SyncAnimation", RpcTarget.Others, _animator.GetBool("isRunning"));

        Vector2 movement = new Vector2(moveX, moveY);
        OnMoveInput?.Invoke(movement);
        Vector2 clampedPosition = (Vector2)transform.position + movement;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, _minBound.x, _maxBound.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, _minBound.y, _maxBound.y);

        transform.position = clampedPosition;

        if (movement.magnitude > 0)
        {        
            _lastMovementDirection = movement.normalized;
            _spriteRenderer.flipX = _lastMovementDirection.x > 0;
        }
        

        if (Input.GetMouseButtonDown(0))
        {
            OnShootInput?.Invoke();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Coin coin = collision.GetComponent<Coin>();
        if (coin != null)
        {
            int playerId = _photonView.OwnerActorNr;
            _photonView.RPC("CollectCoin", RpcTarget.MasterClient, playerId);
            _photonView.RPC("DestroyCoin", RpcTarget.All, coin.photonView.ViewID);
        }
    }

    [PunRPC]
    public void CollectCoin(int playerId)
    {
        _photonView.RPC("AddCoin", RpcTarget.All, playerId);
    }

    [PunRPC]
    private void AddCoin(int playerId)
    {
        if (!_playerCoinCounts.ContainsKey(playerId))
        {
            _playerCoinCounts.Add(playerId, 0);
        }
        _playerCoinCounts[playerId]++;
        if (CoinUI.Instance != null)
        {
            CoinUI.Instance.UpdateCoinCountText(_playerCoinCounts);
        }
        _photonView.RPC("RequestUpdateCoinCounts", RpcTarget.All);
    }

    [PunRPC]
    private void DestroyCoin(int coinViewId)
    {
        PhotonView.Find(coinViewId).GetComponent<Coin>().DestroyCoin();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_lastMovementDirection);
            stream.SendNext(_spriteRenderer.flipX);
        }
        else
        {
            _lastMovementDirection = (Vector2)stream.ReceiveNext();
            _spriteRenderer.flipX = (bool)stream.ReceiveNext();
        }
    }

    public void Die()
    {
        _isDead = true;

        UnregisterPlayer();

        GameManager.Instance.GameOver();

        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    private void ShowPlayerDeath()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowWinnerPopup(GetPlayerName(), GetPlayerCoinsCollected());
        }
    }

    public string GetPlayerName()
    {
        return _photonView.Owner.NickName;
    }

    public int GetPlayerCoinsCollected()
    {
        if (_playerCoinCounts.ContainsKey(_photonView.OwnerActorNr))
        {
            return _playerCoinCounts[_photonView.OwnerActorNr];
        }
        return 0;
    }

    public bool IsDead()
    {
        return _isDead;
    }

    public Vector2 GetMovementDirection()
    {
        return _lastMovementDirection;
    }

    [PunRPC]
    private void RequestUpdateCoinCounts(PhotonMessageInfo info)
    {
        if (info.Sender.IsMasterClient)
        {
            _photonView.RPC("UpdateCoinCounts", RpcTarget.Others, _playerCoinCounts);
        }
    }

    [PunRPC]
    private void UpdateCoinCounts(Dictionary<int, int> coinCounts)
    {
        CoinUI.Instance.UpdateCoinCountText(coinCounts);
    }

    public void RegisterPlayer()
    {
        _photonView.RPC("PlayerJoined", RpcTarget.OthersBuffered, _photonView.ViewID);
    }

    public void UnregisterPlayer()
    {
        _photonView.RPC("PlayerLeft", RpcTarget.OthersBuffered, _photonView.ViewID);
    }

    [PunRPC]
    private void PlayerJoined(int playerViewId)
    {
        PhotonView playerView = PhotonView.Find(playerViewId);
        PlayerController player = playerView.GetComponent<PlayerController>();

        if (player != null)
        {
            GameManager.Instance.RegisterPlayer(playerView, player);
        }
    }

    [PunRPC]
    private void PlayerLeft(int playerViewId)
    {
        PhotonView playerView = PhotonView.Find(playerViewId);
        PlayerController player = playerView.GetComponent<PlayerController>();

        if (player != null)
        {
            GameManager.Instance.UnregisterPlayer(playerView);
        }
    }

    [PunRPC]
    private void SyncAnimation(bool isRunning)
    {
        _animator.SetBool("isRunning", isRunning);
    }

}
