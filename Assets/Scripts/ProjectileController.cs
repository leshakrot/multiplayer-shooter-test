using UnityEngine;
using Photon.Pun;

public class ProjectileController : MonoBehaviourPun
{
    public float _projectileSpeed = 20f;
    public float _projectileLifetime = 2f;

    private Vector2 _movementDirection;
    private Rigidbody2D _rb;

    private void Start()
    {
        if (photonView.IsMine)
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.isKinematic = true;
            Invoke("DestroyProjectile", _projectileLifetime);
        }
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            Vector2 velocity = _movementDirection.normalized * _projectileSpeed;
            _rb.MovePosition(_rb.position + velocity * Time.fixedDeltaTime);
        }
    }

    public void SetMovementDirection(Vector2 direction)
    {
        _movementDirection = direction;
        transform.up = direction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (photonView.IsMine)
        {
            DamageReceiver damageReceiver = collision.GetComponent<DamageReceiver>();
            if (damageReceiver != null)
            {
                damageReceiver.photonView.RPC("TakeDamage", RpcTarget.All, 10);
            }
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
