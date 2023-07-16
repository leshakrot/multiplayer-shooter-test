using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class HealthBar : MonoBehaviourPun
{
    [SerializeField] private Image _healthBarFilling;

    private DamageReceiver _damageReceiver;

    private void Awake()
    {
        _damageReceiver = transform.root.GetComponent<DamageReceiver>();
        _damageReceiver.OnHealthChanged += OnHealthChanged;
    }

    private void OnDestroy()
    {
        _damageReceiver.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float valueAsPercentage)
    {
        _healthBarFilling.fillAmount = valueAsPercentage;

        if (photonView.IsMine)
        {
            photonView.RPC("SyncHealthBar", RpcTarget.Others, valueAsPercentage);
        }
    }

    [PunRPC]
    private void SyncHealthBar(float valueAsPercentage)
    {
        _healthBarFilling.fillAmount = valueAsPercentage;
    }
}
