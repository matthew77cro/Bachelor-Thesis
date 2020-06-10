using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NEAT;
using System;

public class AI : MonoBehaviour
{

    public GameObject seeker;
    public GameObject hider;

    private static System.Random rnd = new System.Random();

    private const int POPULATION_SIZE = 150;

    private NEATPopulation neatHider;
    private NEATPopulation neatSeeker;
    private int currentEvalutaionPairID = 0;
    private int tick = 0;

    // Start is called before the first frame update
    void Start()
    {
        neatHider = new NEATPopulation(Agent.NEURAL_NETWORK_INPUT, Agent.NEURAL_NETWORK_OUTPUT, POPULATION_SIZE, 0.8, 0.9, 0.03, 0.05, () =>
        {
            int sign = rnd.NextDouble() < 0.5 ? -1 : 1;
            return sign * rnd.NextDouble() * double.MaxValue;
        }, FitnessHider, 1, 1, 0.4, 3, 5, 0.25, Chooser);

        neatSeeker = new NEATPopulation(Agent.NEURAL_NETWORK_INPUT, Agent.NEURAL_NETWORK_OUTPUT, POPULATION_SIZE, 0.8, 0.9, 0.03, 0.05, () =>
        {
            int sign = rnd.NextDouble() < 0.5 ? -1 : 1;
            return sign * rnd.NextDouble() * double.MaxValue;
        }, FitnessSeeker, 1, 1, 0.4, 3, 5, 0.25, Chooser);
    }

    void FixedUpdate()
    {

        UpdateAgents(hider, null);
        UpdateAgents(seeker, null);

    }

    double FitnessHider(Genom g)
    {
        return 0;
    }

    double FitnessSeeker(Genom g)
    {
        return 0;
    }

    double ActivationFunction(double value)
    {
        return 1.0 / (1 + Math.Pow(Math.E, -value));
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

    void UpdateAgents(GameObject agent, Genom nn)
    {
        bool[] actions = new bool[Agent.AGENT_ACTION_COUNT];
        float[] neuralNetworkInput = new float[Agent.NEURAL_NETWORK_INPUT];

        int i = 0;
        foreach (var ray in agent.GetComponent<Agent>().Rays())
        {
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, float.MaxValue)) // origin, direction, hitinfo, maxdistance
            {
                if (hit.collider.gameObject.name == "Hider")
                {
                    Debug.LogWarning("Quit");
                    Application.Quit();
                }

                neuralNetworkInput[i++] = GetID(hit.collider.gameObject.tag);
                neuralNetworkInput[i++] = hit.distance;
            }
            else
            {
                neuralNetworkInput[i++] = 0;
                neuralNetworkInput[i++] = 0;
            }
        }

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
    }

    public static int GetID(string tag)
    {
        if (tag == "floor") return 1;
        else if (tag == "wall") return 2;
        else if (tag == "agent") return 3;
        else throw new Exception("Error, tag does not exist : " + tag);
    }

}
