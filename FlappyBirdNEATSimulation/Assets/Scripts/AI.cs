using NEAT;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AI
{

    public const int NUM_OF_INPUTS = 3;
    public const int NUM_OF_OUTPUTS = 1;
    public const int POPULATION_SIZE = 100;


    private readonly System.Random rnd = new System.Random();
    private readonly NEATPopulation neat;

    public int Generation { get { return neat.GenerationNumber; } }
    public Genom Best { get; private set; }

    public AI()
    {

        neat = new NEATPopulation(NUM_OF_INPUTS, NUM_OF_OUTPUTS, POPULATION_SIZE, 0.8, 0.9, 0.03, 0.05, RandomWeight, null, 1, 1, 0.4, 3, 5, 0.25, Chooser, true);
        
    }

    double RandomWeight()
    {
        int sign = rnd.NextDouble() < 0.5 ? -1 : 1;
        return sign * rnd.NextDouble() * double.MaxValue;
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

    public IEnumerable<Genom> Networks()
    {

        foreach (var genom in neat.Population)
            yield return genom;

    }

    public Genom Network(int index)
    {

        if (GameLogic.gameMode == GameLogic.GameMode.LOAD_AI)
            return Best;

        return neat.Population[index];
    }

    public double EvaluateNetwork(int index, double distance, double distanceTop, double distanceBottom)
    {

        var genom = Network(index);
        var input = new Dictionary<int, double>() { 
            { neat.InputNodeIds[0], distance }, 
            { neat.InputNodeIds[1], distanceTop }, 
            { neat.InputNodeIds[2], distanceBottom } 
        };

        NEAT.Algorithms.EvaluateNetwork(genom, input, ActivationFunction, true);

        return genom.Nodes[neat.OutputNodeIds[0]].Value;

    }

    public static double ActivationFunction(double value)
    {
        return 1.0 / (1 + Math.Pow(Math.E, -value));
    }

    public void FitnessCalculated()
    {
        neat.FitnessCalculated();

        var newBest = neat.best;

        if (this.Best == null || this.Best.Fitness < newBest.Fitness)
            this.Best = newBest;

        printStats();
    }

    public void Advance()
    {
        neat.Advance();
    }

    public static void Reset()
    {
        NEAT.ConnectionMarkings.Reset();
        NEAT.NodeMarkings.Reset();
    }

    private void printStats()
    {
        double avgFitness = 0, medianFitness = 0;
        foreach (var g in neat.Population)
        {
            avgFitness += g.Fitness;
        }
        avgFitness /= neat.PopulationSize;

        if (POPULATION_SIZE % 2 == 0)
        {
            medianFitness = (neat.Population[POPULATION_SIZE / 2 - 1].Fitness + neat.Population[POPULATION_SIZE / 2].Fitness) / 2;
        }
        else
        {
            medianFitness = neat.Population[(int)Math.Floor(POPULATION_SIZE / 2.0)].Fitness;
        }

        Debug.Log("Generation " + Generation + " =>\tMax fitness : " + neat.best.Fitness + "\tAvg fintess : " + avgFitness + "\tMedian fitness : " + medianFitness);
    }

}
