using System;
using System.Collections.Generic;
using NEAT;

namespace NEATTest
{
    class NEATTestXOR
    {

        private static readonly Random rnd = new Random();

        public static void Main()
        {

            // xor 2 inputs + 1 bias
            NEATPopulation neat = new NEATPopulation(3, 1, 150, 0.8, 0.9, 0.03, 0.05, RandomWeight, Fitness, 1, 1, 0.4, 3, 5, 0.25, Chooser);
            Console.WriteLine("Gen" + neat.GenerationNumber + " : " + neat.Population[0].Fitness + " Nodes: " + neat.Population[0].Nodes.Count + " Connections: " + neat.Population[0].Connections.Count);
            
            while (neat.Population[0].Fitness != 4)
            {
                neat.Advance();
                Console.WriteLine("Gen" + neat.GenerationNumber + " : " + neat.Population[0].Fitness + " Nodes: " + neat.Population[0].Nodes.Count + " Connections: " + neat.Population[0].Connections.Count);
            }

            Console.WriteLine(neat.Population[0]);

        }

        public static double RandomWeight()
        {
            int sign = rnd.NextDouble() < 0.5 ? -1 : 1;
            return sign * rnd.NextDouble() * double.MaxValue;
        }

        public static double Fitness(Genom g)
        {
            double fitness = 0;

            var gCopy = g.Copy();
            Algorithms.EvaluateNetwork(gCopy, new Dictionary<int, double> { { 0, 1 }, { 1, 0 }, { 2, 0 } }, ActivationFunction, true);
            if (Math.Round(gCopy.GetNode(3).Value, MidpointRounding.ToPositiveInfinity) == 0)
                fitness++;

            gCopy = g.Copy();
            Algorithms.EvaluateNetwork(gCopy, new Dictionary<int, double> { { 0, 1 }, { 1, 0 }, { 2, 1 } }, ActivationFunction, true);
            if (Math.Round(gCopy.GetNode(3).Value, MidpointRounding.ToPositiveInfinity) == 1)
                fitness++;

            gCopy = g.Copy();
            Algorithms.EvaluateNetwork(gCopy, new Dictionary<int, double> { { 0, 1 }, { 1, 1 }, { 2, 0 } }, ActivationFunction, true);
            if (Math.Round(gCopy.GetNode(3).Value, MidpointRounding.ToPositiveInfinity) == 1)
                fitness++;

            gCopy = g.Copy();
            Algorithms.EvaluateNetwork(gCopy, new Dictionary<int, double> { { 0, 1 }, { 1, 1 }, { 2, 1 } }, ActivationFunction, true);
            if (Math.Round(gCopy.GetNode(3).Value, MidpointRounding.ToPositiveInfinity) == 0)
                fitness++;

            return fitness;
        }

        public static double ActivationFunction(double value)
        {
            return 1.0 / (1 + Math.Pow(Math.E, -value));
        }

        public static Connection Chooser(Genom parent1, Genom parent2, int innovation)
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

    }
}
