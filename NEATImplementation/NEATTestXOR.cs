using System;
using System.Collections.Generic;
using System.Text;
using NEAT;

namespace NEATTest
{
    class NEATTestXOR
    {

        private static Random rnd = new Random();

        public static void Main(string[] args)
        {

            // xor
            NEATPopulation neat = new NEATPopulation(2, 1, 1000, 0.8, 0.9, 0.1, 0.1, RandomWeight, Fitness, 1, 1, 1, 3, 5, Chooser);

            int gen = 0;
            while (true)
            {
                Console.WriteLine("Gen" + gen + " : " + neat.Population[0].Fitness + " Nodes: " + neat.Population[0].Nodes.Count);
                Console.ReadKey();
                neat.Advance();
                gen++;
            }

        }

        public static double RandomWeight()
        {
            return rnd.NextDouble();
        }

        public static double Fitness(Genom g)
        {
            double fitness = 0;

            var gCopy = g.Copy();
            Algorithms.EvaluateNetwork(gCopy, new Dictionary<int, double> { { 0, 0 }, { 1, 0 } }, ActivationFunction);
            if (Math.Round(gCopy.GetNode(2).Value, MidpointRounding.ToPositiveInfinity) == 0)
                fitness++;

            gCopy = g.Copy();
            Algorithms.EvaluateNetwork(gCopy, new Dictionary<int, double> { { 0, 0 }, { 1, 1 } }, ActivationFunction);
            if (Math.Round(gCopy.GetNode(2).Value, MidpointRounding.ToPositiveInfinity) == 1)
                fitness++;

            gCopy = g.Copy();
            Algorithms.EvaluateNetwork(gCopy, new Dictionary<int, double> { { 0, 1 }, { 1, 0 } }, ActivationFunction);
            if (Math.Round(gCopy.GetNode(2).Value, MidpointRounding.ToPositiveInfinity) == 1)
                fitness++;

            gCopy = g.Copy();
            Algorithms.EvaluateNetwork(gCopy, new Dictionary<int, double> { { 0, 1 }, { 1, 1 } }, ActivationFunction);
            if (Math.Round(gCopy.GetNode(2).Value, MidpointRounding.ToPositiveInfinity) == 0)
                fitness++;

            return fitness;
        }

        public static double ActivationFunction(double value)
        {
            return 1.0 / (1 + Math.Pow(Math.E, -value));
        }

        public static Connection Chooser(Genom parent1, Genom parent2, int innovation)
        {

            if (rnd.NextDouble() < 0.5)
                return new Connection(innovation)
                {
                    Enabled = parent1.GetConnection(innovation).Enabled,
                    Weight = parent1.GetConnection(innovation).Weight
                };
            else
                return new Connection(innovation)
                {
                    Enabled = parent2.GetConnection(innovation).Enabled,
                    Weight = parent2.GetConnection(innovation).Weight
                };

        }

    }
}
