using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{

    public Spawner spawner;
    public Graphics graphics;

    public const int POPULATION_SIZE = 1;
    
    private bool jumpReleased = true;

    // Start is called before the first frame update
    void Start()
    {

        spawner = GetComponent<Spawner>();
        spawner.Restart(GameOver);
        graphics.ScoreChangeSubscribe(ScoreChanged);

    }

    // Update is called once per frame
    void Update()
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

    // FixedUpdate is called once per tick
    void FixedUpdate()
    {

    }

    public void GameOver(int id)
    {
        var bird = spawner.birds[id];
        spawner.birds.Remove(id);
        Destroy(bird);

        if (spawner.birds.Count == 0)
        {
            spawner.Restart(GameOver);
            graphics.Restart();
        }
    }

    public void ScoreChanged(int oldScore, int newScore)
    {
        Debug.Log(oldScore + " : " + newScore);
        spawner.SpawnPipe();
        spawner.RemoveFirstPipe();
    }

}
