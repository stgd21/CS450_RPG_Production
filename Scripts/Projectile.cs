using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviour
{
    public float delayTime = 2f;
    public float damageMultiplier;
    private CircleCollider2D projCollider;

    // Start is called before the first frame update
    void Start()
    {
        projCollider = GetComponent<CircleCollider2D>();
        Invoke("Explode", delayTime);
    }

    void Explode()
    {
        //Destroy(gameObject);
        projCollider.enabled = true;
        Invoke("DisableCollider", 0.1f);
        //Invoke another function to enable a collider to calculate pushback?
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!PhotonNetwork.IsMasterClient)
            //Master client will deliver the damage
            return;

        if (collision.CompareTag("Player"))
        {
            Vector3 direction = collision.transform.position - transform.position;
            direction.Normalize();
            float distance = Vector3.Distance(collision.transform.position, transform.position);
            //Calculate pushback in player script
            PlayerController targetPlayer = collision.GetComponent<PlayerController>();
            targetPlayer.photonView.RPC("PushBack", targetPlayer.photonPlayer, direction, distance);
            //Damage player based on distance from center
            targetPlayer.photonView.RPC("TakeDamage", targetPlayer.photonPlayer, (int)Mathf.Min(damageMultiplier, damageMultiplier / distance));
        }
    }

    private void DisableCollider()
    {
        projCollider.enabled = false;
        Destroy(gameObject);
    }
}
