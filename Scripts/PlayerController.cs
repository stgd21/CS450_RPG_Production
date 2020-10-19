using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{
    [HideInInspector]
    public int id;

    [Header("info")]
    public float moveSpeed;
    public int gold;
    public int curHp;
    public int maxHp;
    public bool dead;
    public float explosionMultiplier;

    [Header("Attack")]
    public int damage;
    public float attackRange;
    public float attackRate;
    public float projectileRate;
    private float lastAttackTime;
    private float lastProjectileTime;

    [Header("Components")]
    public Rigidbody2D rig;
    public Player photonPlayer;
    public SpriteRenderer sr;
    public Animator weaponAnim;
    public string projectilePath = "ProjectileTarget";

    public HeaderInfo headerInfo;

    //local player
    public static PlayerController me;

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        if (GameManager.instance.canMove == true)
            Move();

        if (Input.GetMouseButton(0) && Time.time - lastAttackTime > attackRate)
            Attack();

        if (Input.GetMouseButtonDown(1) && Time.time - lastProjectileTime > projectileRate)
            FireProjectile();

        float mouseX = (Screen.width / 2) - Input.mousePosition.x;

        if (mouseX > 0)
            weaponAnim.transform.parent.localScale = new Vector3(-1, 1, 1);
        else
            weaponAnim.transform.parent.localScale = new Vector3(1, 1, 1);
    }

    void Move()
    {
        //get horizontal and vertical inputs
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        //apply to velocity
        rig.velocity = new Vector2(x, y) * moveSpeed;
    }

    void FireProjectile()
    {
        if (GameManager.instance.canMove == false)
            return;
        lastProjectileTime = Time.time;

        //Get mouse pos in world space
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPos.z = 0;
        PhotonNetwork.Instantiate(projectilePath, targetPos, Quaternion.identity);
    }

    //melee attacks toward the mouse
    void Attack()
    {
        lastAttackTime = Time.time;

        //calculate direction
        Vector3 dir = (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized;

        //shoot raycast in dir
        RaycastHit2D hit = Physics2D.Raycast(transform.position + dir, dir, attackRange);

        //did we hit enemy?
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Enemy"))
        {
            //get enemy data and damage them
            //changed for production
            BreakableDoor enemy = hit.collider.GetComponent<BreakableDoor>();
            enemy.photonView.RPC("TakeDamage", RpcTarget.MasterClient, damage);
        }

        //play attack animation
        weaponAnim.SetTrigger("Attack");
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        curHp -= damage;
        //update health bar
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);

        if (curHp <= 0)
            Die();
        else
        {
            StartCoroutine(DamageFlash());
            IEnumerator DamageFlash()
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.05f);
                sr.color = Color.white;
            }
        }
    }

    void Die()
    {
        dead = true;
        rig.isKinematic = true;

        transform.position = new Vector3(0, 99, 0);

        Vector3 spawnPos = GameManager.instance.spawnPoints[Random.Range(0, GameManager.instance.spawnPoints.Length)].position;
        StartCoroutine(Spawn(spawnPos, GameManager.instance.respawnTime));
    }

    IEnumerator Spawn(Vector3 spawnPos, float timeToSpawn)
    {
        yield return new WaitForSeconds(timeToSpawn);

        dead = false;
        transform.position = spawnPos;
        curHp = maxHp;
        rig.isKinematic = false;

        //update health bar
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);
    }

    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;
        GameManager.instance.players[id - 1] = this;
        Debug.Log(GameManager.instance.players[id - 1]);

        //initialize health bar
        headerInfo.Initialize(player.NickName, maxHp);

        if (player.IsLocal)
            me = this;
        else
            rig.isKinematic = true;
    }

    [PunRPC]
    void Heal(int amountToHeal)
    {
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);
        //update health bar
    }



    [PunRPC]
    public void PushBack(Vector3 direction, float distance)
    {
        GameManager.instance.canMove = false;
        rig.velocity = new Vector2(direction.x, direction.y) * explosionMultiplier / distance;
        Invoke("AllowMovement", 1f);
    }
    void AllowMovement()
    {
        GameManager.instance.canMove = true;
    }

}
