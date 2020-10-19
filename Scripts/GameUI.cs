using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    private float startTime = -1f;
    //instance
    public static GameUI instance;

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

    void Update()
    {
        if (GameManager.instance.canMove == true && startTime == -1f)
            startTime = Time.timeSinceLevelLoad;
        if (!GameManager.instance.isGameEnding)
            timerText.text = "<b>Time:</b> " + Time.timeSinceLevelLoad;
    }
}

