using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Coin : MonoBehaviourPun
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null && photonView.IsMine)
        {
            int playerId = photonView.OwnerActorNr;
            PhotonNetwork.RaiseEvent(1, new object[] { playerId }, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
            PhotonNetwork.RaiseEvent(2, new object[] { photonView.ViewID }, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);

            photonView.RPC("DestroyCoin", RpcTarget.All);
        }
    }

    [PunRPC]
    public void DestroyCoin()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
