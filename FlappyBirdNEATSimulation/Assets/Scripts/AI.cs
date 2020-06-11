using NEAT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class AI
{

    public const int POPULATION_SIZE = 100;

    private System.Random rnd = new System.Random();
    private NEATPopulation neat;

    public AI()
    {

        neat = new NEATPopulation(3, 1, POPULATION_SIZE, 0.8, 0.9, 0.03, 0.05, RandomWeight, null, 1, 1, 0.4, 3, 5, 0.25, Chooser, true);

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
        return neat.Population[index];
    }

    public double EvaluateNetwork(int index, double distance, double distanceTop, double distanceBottom)
    {

        var genom = neat.Population[index];
        var input = new Dictionary<int, double>() { { 0, distance }, { 1, distanceTop }, { 2, distanceBottom } };

        NEAT.Algorithms.EvaluateNetwork(genom, input, ActivationFunction, true);

        return genom.Nodes[3].Value;

    }

    public static double ActivationFunction(double value)
    {
        return 1.0 / (1 + Math.Pow(Math.E, -value));
    }

    public void Advance()
    {
        neat.FitnessCalculated();
        neat.Advance();
    }

    public static void Reset()
    {
        NEAT.ConnectionMarkings.Reset();
        NEAT.NodeMarkings.Reset();
    }

}
