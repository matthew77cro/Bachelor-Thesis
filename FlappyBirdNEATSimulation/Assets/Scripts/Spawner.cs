using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public const int PIPE_INIT_COUNT = 5;
    public const float PIPE_MAX_Y_DEVIATION = 2.8f;
    public const float PIPE_INIT_X_POS = 10f;
    public const float PIPE_DISTANCE = 6.5f;
    public const int PIPE_MAX_DESPAWN_COUNT = 3;

    public GameObject birdPrefab;
    public GameObject pipePrefab;

    public Dictionary<int, GameObject> birds = new Dictionary<int, GameObject>(); // id -> bird
    public Queue<GameObject> despawnPipes = new Queue<GameObject>();
    public Queue<GameObject> pipes = new Queue<GameObject>();

    private readonly System.Random rnd = new System.Random();

    private float lastPipeX;
    private Bird.GameOverFunction gof;

    public void Restart()
    {

        foreach (var bird in birds.Values)
            Destroy(bird);
        birds.Clear();
        foreach (var pipe in pipes)
            Destroy(pipe);
        pipes.Clear();
        foreach (var pipe in despawnPipes)
            Destroy(pipe);
        despawnPipes.Clear();

    }

    public void GameStart()
    {

        int numOfBirds = GameLogic.gameMode == GameLogic.GameMode.AI ? AI.POPULATION_SIZE : 1;

        for (int i = 0; i < numOfBirds; i++)
        {
            var bird = Instantiate(birdPrefab);
            bird.GetComponent<Bird>().id = i;
            birds.Add(i, bird);
            bird.GetComponent<Bird>().GameOverSubscribe(gof);
        }

        lastPipeX = PIPE_INIT_X_POS - PIPE_DISTANCE;
        for (int i = 0; i < PIPE_INIT_COUNT; i++)
        {
            SpawnPipe();
        }

    }

    public void GameOverSubscribe(Bird.GameOverFunction gof)
    {
        this.gof = gof;
    }

    private void SpawnPipe()
    {
        float yPos = ((float)rnd.NextDouble()) * PIPE_MAX_Y_DEVIATION * 2 - PIPE_MAX_Y_DEVIATION;
        lastPipeX += PIPE_DISTANCE;
        pipes.Enqueue(Instantiate(pipePrefab, new Vector3(lastPipeX, yPos, 0), Quaternion.Euler(0, 0, 0)));
    }

    private void RemovePipe()
    {
        despawnPipes.Enqueue(pipes.Dequeue());
        if (despawnPipes.Count > PIPE_MAX_DESPAWN_COUNT)
        {
            Destroy(despawnPipes.Dequeue());
        }
    }

    public void NextPipe()
    {
        SpawnPipe();
        RemovePipe();
    }

}
