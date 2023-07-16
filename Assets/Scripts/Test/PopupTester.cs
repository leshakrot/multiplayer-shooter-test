using UnityEngine;
using Photon.Pun;

public class PopupTester : MonoBehaviourPun
{
    [SerializeField] private GameObject popupObject;

    private void Start()
    {
        popupObject.SetActive(false);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && photonView.IsMine)
        {
            Debug.Log("CORRECT");
            photonView.RPC("TogglePopup", RpcTarget.All, !popupObject.activeSelf);
        }
    }

    [PunRPC]
    private void TogglePopup(bool isActive)
    {
        popupObject.SetActive(isActive);
    }
}
