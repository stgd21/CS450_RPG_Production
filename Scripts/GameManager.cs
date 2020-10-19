using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using TMPro;

public class GameManager : MonoBehaviourPun
{
    [Header("Players")]
    public string playerPrefabPath;
    public Transform[] spawnPoints;
    public float respawnTime;
    public PlayerController[] players;

    public TextMeshProUGUI gameStartText;
    public TextMeshProUGUI counter;
    public TextMeshProUGUI winText;
    public int preRoundTime = 5;
    public float roundEndTime = 5;
    [HideInInspector]
    public bool canMove = false;

    private int playersInGame;
    public bool isGameEnding = false;

    //instance
    public static GameManager instance;

    private void Awake()
    {
        //make proper singleton
        if (instance != null && instance != this)
            gameObject.SetActive(false);
        else
        {
            instance = this;
        }
    }

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;
        if (playersInGame == PhotonNetwork.PlayerList.Length)
            SpawnPlayer();
    }

    private void Start()
    {
        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        gameStartText.enabled = true;
        canMove = false;
    }

    private void Update()
    {
        if (preRoundTime - (int)Time.timeSinceLevelLoad < 0 && counter.enabled == true)
        {
            counter.enabled = false;
            gameStartText.enabled = false;
            canMove = true;
        } else
        counter.text = (preRoundTime - (int)Time.timeSinceLevelLoad).ToString();
    }

    void SpawnPlayer()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabPath, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);

        //initialize player
        playerObj.GetComponent<PhotonView>().RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    //Called from finish line
    [PunRPC]
    public void EndGame(string winnerName)
    {
        if (isGameEnding == true)
            return;
        else
        {
            isGameEnding = true;
            winText.text = winnerName + " wins!";
            winText.enabled = true;
            Invoke("ReturnToLobby", roundEndTime);
        }
    }

    private void ReturnToLobby()
    {
        NetworkManager.instance.Disconnect();
    }

}
