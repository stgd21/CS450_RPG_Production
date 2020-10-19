using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class FinishLine : MonoBehaviour
{
    string winnerName;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        //The master client will do the comparison
        if (collision.CompareTag("Player"))
        {
            winnerName = collision.GetComponent<PlayerController>().photonPlayer.NickName;
            GameManager.instance.photonView.RPC("EndGame", RpcTarget.All, winnerName);
        }
    }
}
