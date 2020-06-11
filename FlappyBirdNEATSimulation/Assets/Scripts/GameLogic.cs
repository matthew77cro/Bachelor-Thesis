using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{

    public Spawner spawner;
    public Graphics graphics;

    public const string defaultText = "space = play; a = AI";
    public static GameMode gameMode = GameMode.NONE;
    
    private bool jumpReleased = true;
    private AI ai;

    // Start is called before the first frame update
    void Start()
    {

        spawner.Restart();
        spawner.GameOverSubscribe(GameOver);
        graphics.Restart();
        graphics.ScoreChangeSubscribe(ScoreChanged);

    }

    // Update is called once per frame
    void Update()
    {

        if (gameMode == GameMode.NONE)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                gameMode = GameMode.PLAYER;
                spawner.GameStart();
                graphics.GameStart();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                gameMode = GameMode.AI;
                ai = new AI();
                spawner.GameStart();
                graphics.GameStart();
            }
        }
        else if (gameMode == GameMode.PLAYER)
        {
            if (Input.GetKeyDown(KeyCode.Space) && jumpReleased)
            {
                foreach (var bird in spawner.birds.Values)
                    bird.GetComponent<Bird>().Jump();
                jumpReleased = false;
            }
            else if (!Input.GetKeyDown(KeyCode.Space))
            {
                jumpReleased = true;
            }
        }

    }

    // FixedUpdate is called once per tick
    void FixedUpdate()
    {
        if (gameMode == GameMode.AI && spawner.pipes.Count != 0)
        {

            var nextPipe = spawner.pipes.Peek();
            var nextPipeT = nextPipe.transform.Find("Top Cover");
            var nextPipeB = nextPipe.transform.Find("Bottom Cover");

            foreach (var kvp in spawner.birds)
            {
                var index = kvp.Key;
                var bird = kvp.Value;


                double d = nextPipe.transform.position.x - bird.transform.position.x;
                double dt = nextPipeT.transform.position.y - bird.transform.position.y;
                double db = bird.transform.position.y - nextPipeB.transform.position.y;

                double output = ai.EvaluateNetwork(index, d, dt, db);
                if (output >= 0.5)
                    bird.GetComponent<Bird>().Jump();
            }
        }
    }

    public void GameOver(int id)
    {
        if (!spawner.birds.ContainsKey(id))
            return;
        var bird = spawner.birds[id];
        spawner.birds.Remove(id);
        Destroy(bird);

        if (gameMode == GameMode.AI)
        {
            var genom = ai.Network(id);
            genom.Fitness = bird.transform.position.x;
            genom.FitnessCalculated = true;
        }

        if (spawner.birds.Count == 0)
        {
            spawner.Restart();
            graphics.Restart();

            if (gameMode == GameMode.AI)
            {
                ai.Advance();
                spawner.GameStart();
                graphics.GameStart();
            }
            else
            {
                AI.Reset();
                gameMode = GameMode.NONE;
            }
        }
    }

    public void ScoreChanged(int oldScore, int newScore)
    {
        spawner.SpawnPipe();
    }

    public enum GameMode
    {
        PLAYER,
        AI,
        NONE
    }

}
