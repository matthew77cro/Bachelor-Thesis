using UnityEngine;

public class GameLogic : MonoBehaviour
{

    public Spawner spawner;
    public CameraControl camcontrol;

    public static GameMode gameMode = GameMode.NONE;
    public static AI ai;

    private bool jumpReleased = true;

    // Start is called before the first frame update
    void Start()
    {

        spawner.Restart();
        spawner.GameOverSubscribe(GameOver);
        camcontrol.Restart();
        camcontrol.ScoreChangeSubscribe(ScoreChanged);

    }

    // Update is called once per frame
    void Update()
    {

        if (gameMode == GameMode.NONE)
        {
            // Enter gamemode based on player input
            if (Input.GetKeyDown(KeyCode.Space))
            {
                gameMode = GameMode.PLAYER;
                spawner.GameStart();
                camcontrol.GameStart();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                gameMode = GameMode.AI;
                AI.Reset();
                ai = new AI();
                spawner.GameStart();
                camcontrol.GameStart();
            }
            else if (Input.GetKeyDown(KeyCode.L) && ai != null && ai.Best != null)
            {
                gameMode = GameMode.LOAD_AI;
                spawner.GameStart();
                camcontrol.GameStart();
            }
        }
        else if (gameMode == GameMode.PLAYER)
        {
            // Player controls bird jumps
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
        else if (gameMode == GameMode.AI || gameMode == GameMode.LOAD_AI)
        {
            // Exit AI gamemode
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var fitness = camcontrol.cameraControl.transform.position.x;

                spawner.Restart();
                camcontrol.Restart();

                if (gameMode == GameMode.AI)
                {
                    foreach (var genom in ai.Networks())
                    {
                        if (genom.FitnessCalculated)
                            continue;

                        genom.Fitness = fitness;
                        genom.FitnessCalculated = true;
                    }

                    ai.FitnessCalculated();
                }

                gameMode = GameMode.NONE;

            }
        }

    }

    // FixedUpdate is called once per tick
    void FixedUpdate()
    {
        if ((gameMode == GameMode.AI || gameMode == GameMode.LOAD_AI) && spawner.pipes.Count != 0)
        {
            // Feed the AI and get output
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
            camcontrol.Restart();

            if (gameMode == GameMode.AI)
            {
                ai.FitnessCalculated();
                ai.Advance();
                spawner.GameStart();
                camcontrol.GameStart();
            }
            else if (gameMode == GameMode.PLAYER || gameMode == GameMode.LOAD_AI)
            {
                gameMode = GameMode.NONE;
            }
        }
    }

    public void ScoreChanged(int oldScore, int newScore)
    {
        spawner.NextPipe();
    }

    public enum GameMode
    {
        PLAYER,
        AI,
        LOAD_AI,
        NONE
    }

}
