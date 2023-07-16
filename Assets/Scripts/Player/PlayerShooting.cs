using UnityEngine;
using Photon.Pun;

public class PlayerShooting : MonoBehaviourPun
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _bulletSpeed = 10f;
    [SerializeField] private float _bulletSpawnPositionOffsetY;

    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerController.OnShootInput += Shoot;
    }

    private void OnDestroy()
    {
        _playerController.OnShootInput -= Shoot;
    }

    private void Update()
    {
        if (!photonView.IsMine || _playerController.IsDead()) return;

        if (IsShootInput())
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        Vector2 bulletDirection = _playerController.GetMovementDirection();
        if (bulletDirection.magnitude == 0)
        {
            bulletDirection = _playerController.transform.right;
        }
        Vector2 bulletSpawnPosition = new Vector2 (_playerController.transform.position.x, 
                                                   _playerController.transform.position.y + 
                                                   _bulletSpawnPositionOffsetY);

        if (bulletDirection.magnitude == 0)
        {
            bulletSpawnPosition += (Vector2)_playerController.transform.right * 2f;
        }
        else
        {
            bulletSpawnPosition += bulletDirection.normalized * 2f;
        }

        GameObject bullet = PhotonNetwork.Instantiate(_bulletPrefab.name, bulletSpawnPosition, Quaternion.identity);
        ProjectileController projectileController = bullet.GetComponent<ProjectileController>();
        projectileController.SetMovementDirection(bulletDirection);
    }

    private bool IsShootInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Touch secondTouch = Input.GetTouch(1);
            if (touch.phase == TouchPhase.Began || (secondTouch.phase == TouchPhase.Began && touch.phase == TouchPhase.Moved))
            {
                return true;
            }
        }
        return false;
    }
}
