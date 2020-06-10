using System.Collections.Generic;
using UnityEngine;
using NEAT;
using System;

public class Simulation : MonoBehaviour
{

    private static System.Random rnd = new System.Random();

    private const int POPULATION_SIZE = 10;
    private const int MAX_TICK = 5 * 50;
    private const int NEURAL_NETWORK_INPUT = 2 * Agent.RAYS_ROWS * Agent.RAYS_PER_ROW + 1; // +1 for the bias
    private const int LEGAL_AGENTACTION_TUPLES_COUNT = 22;
    private const int NEURAL_NETWORK_OUTPUT = 1;

    private static Vector3 INITIAL_AGENT_POSITION = new Vector3(-7.0f, 1.0f, 7.0f);
    private static Vector3 INITIAL_TARGET_POSITION = new Vector3(8.5f, 1.0f, -8.0f);

    public GameObject agentPrefab;
    public GameObject targetPrefab;

    private GameObject agent;
    private GameObject target;

    public GameObject door;
    public GameObject pressurePlate;

    private NEATPopulation neat;
    private int currentEvalutaionID = 0;
    private int tick = 0;
    private bool activtedPressurePlate = false;

    // Start is called before the first frame update
    void Start()
    {
        neat = new NEATPopulation(NEURAL_NETWORK_INPUT, NEURAL_NETWORK_OUTPUT, POPULATION_SIZE, 0.8, 0.9, 0.03, 0.05, () =>
        {
            int sign = rnd.NextDouble() < 0.5 ? -1 : 1;
            return sign * rnd.NextDouble();
        }, 1, 1, 0.4, 3, 5, 0.25, Chooser);

        Reset();
    }

    void Update()
    {

        /* TESTING
        bool[] actions = new bool[Agent.AGENT_ACTION_COUNT];

        if (Input.GetKey(KeyCode.W))
            actions[(int)Agent.AgentAction.FORWARD] = true;
        if (Input.GetKey(KeyCode.S))
            actions[(int)Agent.AgentAction.BACKWARD] = true;
        if (Input.GetKey(KeyCode.A))
            actions[(int)Agent.AgentAction.LEFT] = true;
        if (Input.GetKey(KeyCode.D))
            actions[(int)Agent.AgentAction.RIGHT] = true;
        if (Input.GetKey(KeyCode.J))
            actions[(int)Agent.AgentAction.ROTATE_AC] = true;
        if (Input.GetKey(KeyCode.L))
            actions[(int)Agent.AgentAction.ROTATE_C] = true;

        agent.GetComponent<Agent>().ApplyAgentAction(actions);
        */

    }

    void FixedUpdate()
    {

        // Has the time passed?
        // Which genom to evaluate
        Debug.Log("Generation " + neat.GenerationNumber + " Genom: " + currentEvalutaionID + " Tick " + tick);
        tick++;
        if (tick >= MAX_TICK)
        {
            CalculateFitnessAndSerializePopulation();
            Reset();
            return;
        }

        // Evaluate genom
        UpdateAgent(agent, 
            neat.Population[currentEvalutaionID]);

        if (agent.GetComponent<Agent>().PressurePlateActivated && !activtedPressurePlate)
        {
            activtedPressurePlate = true;
            door.GetComponent<MeshRenderer>().enabled = false;
            door.GetComponent<Collider>().enabled = false;
        }

    }

    void CalculateFitness()
    {

        double fitness = 0;

        double initialDistance = (INITIAL_AGENT_POSITION-INITIAL_TARGET_POSITION).magnitude;
        double distance = (agent.transform.position - target.transform.position).magnitude;

        fitness = Math.Pow(2, initialDistance - distance);

        if (activtedPressurePlate)
            fitness *= 2;

        neat.Population[currentEvalutaionID].Fitness = fitness;
        neat.Population[currentEvalutaionID].FitnessCalculated = true;

    }

    void SerializePopulation()
    {
        
    }

    void PrintBestFitness()
    {

    }

    void Reset()
    {

        door.GetComponent<BoxCollider>().enabled = true;
        door.GetComponent<MeshRenderer>().enabled = true;

        GameObject.Destroy(agent);
        GameObject.Destroy(target);
        agent = Instantiate(agentPrefab, INITIAL_AGENT_POSITION, Quaternion.Euler(0, 120, 0));
        target = Instantiate(targetPrefab, INITIAL_TARGET_POSITION, Quaternion.Euler(0, 0, 0));

        tick = 0;
        currentEvalutaionID++;
        activtedPressurePlate = false;

        if (currentEvalutaionID >= neat.PopulationSize)
        {
            SerializePopulation();
            PrintBestFitness();
            neat.Advance();
            currentEvalutaionID = 0;
        }

    }

    double ActivationFunction(double value)
    {
        return (LEGAL_AGENTACTION_TUPLES_COUNT) * 1.0 / (1 + Math.Pow(Math.E, -value)) - 0.5;
    }

    Connection Chooser(Genom parent1, Genom parent2, int innovation)
    {

        byte disabledInParent = 0;
        if (!parent1.GetConnection(innovation).Enabled)
            disabledInParent = 1;
        if (!parent2.GetConnection(innovation).Enabled)
            disabledInParent = disabledInParent == 1 ? (byte)0 : (byte)2;

        if (disabledInParent == 0)
            if (rnd.NextDouble() < 0.5)
                return parent1.GetConnection(innovation);
            else
                return parent2.GetConnection(innovation);
        else if (disabledInParent == 1)
            if (rnd.NextDouble() < 0.75)
                return parent1.GetConnection(innovation);
            else
                return parent2.GetConnection(innovation);
        else
            if (rnd.NextDouble() < 0.75)
            return parent2.GetConnection(innovation);
        else
            return parent1.GetConnection(innovation);
    }

    void UpdateAgent(GameObject agent, Genom nn)
    {

        double[] neuralNetworkInput = new double[NEURAL_NETWORK_INPUT];

        int i = 0;
        foreach (var ray in agent.GetComponent<Agent>().Rays())
        {
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, float.MaxValue)) // origin, direction, hitinfo, maxdistance
            {
                neuralNetworkInput[i++] = GetID(hit.collider.gameObject.tag);
                neuralNetworkInput[i++] = hit.distance;
            }
            else
            {
                neuralNetworkInput[i++] = 0;
                neuralNetworkInput[i++] = 0;
            }
        }

        Dictionary<int, double> dict = new Dictionary<int, double>();
        dict.Add(0, 1); // Adding the bias node value
        for (i = 0; i < NEURAL_NETWORK_INPUT; i++)
        {
            dict.Add(i + 1, neuralNetworkInput[i]);
        }

        NEAT.Algorithms.EvaluateNetwork(nn, dict, ActivationFunction, true);
        double output = nn.GetNode(NEURAL_NETWORK_INPUT).Value;
        agent.GetComponent<Agent>().ApplyAgentAction(GetActionsForLegalActionID((int)Math.Round(output, MidpointRounding.AwayFromZero)));

    }

    public static int GetID(string tag)
    {
        if (tag == "floor") return 1;
        else if (tag == "wall") return 2;
        else if (tag == "agent") return 3;
        else if (tag == "target") return 4;
        else throw new Exception("Error, tag does not exist : " + tag);
    }

    public static bool[] GetActionsForLegalActionID(int id)
    {

        if (id < 0 || id > LEGAL_AGENTACTION_TUPLES_COUNT)
            throw new Exception("Illegal argument " + id);

        bool[] actions = new bool[Agent.AGENT_ACTION_COUNT];

        switch (id)
        {
            case 0:
                actions[(int)Agent.AgentAction.FORWARD] = true;
                break;
            case 1:
                actions[(int)Agent.AgentAction.BACKWARD] = true;
                break;
            case 2:
                actions[(int)Agent.AgentAction.LEFT] = true;
                break;
            case 3:
                actions[(int)Agent.AgentAction.RIGHT] = true;
                break;
            case 4:
                actions[(int)Agent.AgentAction.ROTATE_AC] = true;
                break;
            case 5:
                actions[(int)Agent.AgentAction.ROTATE_C] = true;
                break;
            case 6:
                actions[(int)Agent.AgentAction.FORWARD] = true;
                actions[(int)Agent.AgentAction.LEFT] = true;
                break;
            case 7:
                actions[(int)Agent.AgentAction.FORWARD] = true;
                actions[(int)Agent.AgentAction.RIGHT] = true;
                break;
            case 8:
                actions[(int)Agent.AgentAction.FORWARD] = true;
                actions[(int)Agent.AgentAction.ROTATE_AC] = true;
                break;
            case 9:
                actions[(int)Agent.AgentAction.FORWARD] = true;
                actions[(int)Agent.AgentAction.ROTATE_C] = true;
                break;
            case 10:
                actions[(int)Agent.AgentAction.FORWARD] = true;
                actions[(int)Agent.AgentAction.LEFT] = true;
                actions[(int)Agent.AgentAction.ROTATE_AC] = true;
                break;
            case 11:
                actions[(int)Agent.AgentAction.FORWARD] = true;
                actions[(int)Agent.AgentAction.LEFT] = true;
                actions[(int)Agent.AgentAction.ROTATE_C] = true;
                break;
            case 12:
                actions[(int)Agent.AgentAction.FORWARD] = true;
                actions[(int)Agent.AgentAction.RIGHT] = true;
                actions[(int)Agent.AgentAction.ROTATE_AC] = true;
                break;
            case 13:
                actions[(int)Agent.AgentAction.FORWARD] = true;
                actions[(int)Agent.AgentAction.RIGHT] = true;
                actions[(int)Agent.AgentAction.ROTATE_C] = true;
                break;
            case 14:
                actions[(int)Agent.AgentAction.BACKWARD] = true;
                actions[(int)Agent.AgentAction.LEFT] = true;
                break;
            case 15:
                actions[(int)Agent.AgentAction.BACKWARD] = true;
                actions[(int)Agent.AgentAction.RIGHT] = true;
                break;
            case 16:
                actions[(int)Agent.AgentAction.BACKWARD] = true;
                actions[(int)Agent.AgentAction.ROTATE_AC] = true;
                break;
            case 17:
                actions[(int)Agent.AgentAction.BACKWARD] = true;
                actions[(int)Agent.AgentAction.ROTATE_C] = true;
                break;
            case 18:
                actions[(int)Agent.AgentAction.BACKWARD] = true;
                actions[(int)Agent.AgentAction.LEFT] = true;
                actions[(int)Agent.AgentAction.ROTATE_AC] = true;
                break;
            case 19:
                actions[(int)Agent.AgentAction.BACKWARD] = true;
                actions[(int)Agent.AgentAction.LEFT] = true;
                actions[(int)Agent.AgentAction.ROTATE_C] = true;
                break;
            case 20:
                actions[(int)Agent.AgentAction.BACKWARD] = true;
                actions[(int)Agent.AgentAction.RIGHT] = true;
                actions[(int)Agent.AgentAction.ROTATE_AC] = true;
                break;
            case 21:
                actions[(int)Agent.AgentAction.BACKWARD] = true;
                actions[(int)Agent.AgentAction.RIGHT] = true;
                actions[(int)Agent.AgentAction.ROTATE_C] = true;
                break;
        }

        return actions;

    }

}
