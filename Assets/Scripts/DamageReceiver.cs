using Photon.Pun;
using System;

public class DamageReceiver : MonoBehaviourPun
{
    private int _maxHealth = 100;
    private int _currentHealth; 
    private PlayerController _playerController;

    public event Action<float> OnHealthChanged;

    private void Start()
    {
        _currentHealth = _maxHealth;
        _playerController = GetComponent<PlayerController>();
    }

    [PunRPC]
    public void TakeDamage(int amount)
    {
        if (!photonView.IsMine)
            return;

        _currentHealth -= amount;

        if (_currentHealth <= 0)
        {
            Die();
            _playerController.Die();
        }
        else
        {
            float healthPercentage = (float)_currentHealth / _maxHealth;
            OnHealthChanged?.Invoke(healthPercentage);
        }
    }

    private void Die()
    {
        OnHealthChanged?.Invoke(0f);
    }
}
