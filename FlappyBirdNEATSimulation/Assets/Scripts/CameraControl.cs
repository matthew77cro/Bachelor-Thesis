using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{

    public delegate void ScoreChangeFunction(int oldScore, int newScore);

    public const string scoreDefault = "Start";
    public const string buttonInfoDefault = "Space => play\nA => AI\nL => load trained AI";
    public const string infoDefault = "";

    public GameObject cameraControl;
    public GameObject scoreObject;
    public GameObject buttonInfoObject;
    public GameObject infoObject;

    public int score = 0;

    private List<ScoreChangeFunction> scf = new List<ScoreChangeFunction>();
    private bool gameStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            scoreObject.GetComponent<Text>().text = score.ToString();

            if (GameLogic.gameMode == GameLogic.GameMode.AI)
            {
                infoObject.GetComponent<Text>().text = "Gen : " + GameLogic.ai.Generation + "\nESC to stop";
            }
        }
    }

    void FixedUpdate()
    {

        if (gameStarted)
        {
            cameraControl.GetComponent<Rigidbody>().velocity = new Vector3(Bird.MOVE_SPEED, 0, 0);

            // Update the score and spawn a new pipe
            // - 0.75 -> half the width of a pipe
            int newScore = (int)Math.Ceiling((cameraControl.transform.position.x - 0.75 - Spawner.PIPE_INIT_X_POS) / Spawner.PIPE_DISTANCE);
            if (newScore > score)
            {
                foreach (var s in scf)
                    s(score, newScore);
                score = newScore;
            }
        }
        
    }

    public void Restart()
    {
        gameStarted = false;
        cameraControl.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        cameraControl.transform.position = new Vector3(0, 0, 0);
        score = 0;

        scoreObject.GetComponent<Text>().text = scoreDefault;
        buttonInfoObject.GetComponent<Text>().text = buttonInfoDefault;
        infoObject.GetComponent<Text>().text = infoDefault;
    }

    public void GameStart()
    {
        gameStarted = true;
        buttonInfoObject.GetComponent<Text>().text = "";

        if (GameLogic.gameMode == GameLogic.GameMode.LOAD_AI)
        {
            infoObject.GetComponent<Text>().text = "AI\nESC to stop";
        }
        else if (GameLogic.gameMode == GameLogic.GameMode.PLAYER)
        {
            infoObject.GetComponent<Text>().text = "Player";
        }
    }

    public void ScoreChangeSubscribe(ScoreChangeFunction s)
    {
        scf.Add(s);
    }

}
