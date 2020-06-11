using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graphics : MonoBehaviour
{

    public delegate void ScoreChangeFunction(int oldScore, int newScore);

    public GameObject graphics;
    public GameObject scoreObject;

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
            scoreObject.GetComponent<TextMesh>().text = score.ToString();
        }
    }

    void FixedUpdate()
    {

        if (gameStarted)
        {
            graphics.GetComponent<Rigidbody>().velocity = new Vector3(Bird.MOVE_SPEED, 0, 0);

            // Update the score and spawn a new pipe
            // - 0.75 -> half the width of a pipe
            int newScore = (int)Math.Ceiling((graphics.transform.position.x - 0.75 - Spawner.PIPE_INIT_X_POS) / Spawner.PIPE_DISTANCE);
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
        graphics.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        graphics.transform.position = new Vector3(0, 0, 0);
        score = 0;
        scoreObject.GetComponent<TextMesh>().text = GameLogic.defaultText;
    }

    public void GameStart()
    {
        gameStarted = true;
    }

    public void ScoreChangeSubscribe(ScoreChangeFunction s)
    {
        scf.Add(s);
    }

}
