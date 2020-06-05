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

            // xor 2 inputs
            int numberOfIterations = 100;
            int maxNumberOfGenerations = 100;

            int failedTimes = 0;
            List<int> bestGenomeNodeCount = new List<int>();
            List<int> numberOfGenerations = new List<int>();

            for (int i = 0; i < numberOfIterations; i++)
            {
                NodeMarkings.Reset();
                ConnectionMarkings.Reset();
                Console.WriteLine("-------------------Iteration " + i + "-------------------");

                NEATPopulation neat = new NEATPopulation(2, 1, 150, 0.6, 0.9, 0.025, 0.025, RandomWeight, Fitness, 1, 1, 0.4, 3, 5, 0.5, Chooser, true);
                Console.WriteLine("Gen" + neat.GenerationNumber + " : " + neat.Population[0].Fitness + " Nodes: " + neat.Population[0].Nodes.Count + " Connections: " + neat.Population[0].Connections.Count);

                int counter = 0;
                while (neat.best.Fitness != 4 && counter < maxNumberOfGenerations)
                {
                    neat.Advance();
                    Console.WriteLine("Gen" + neat.GenerationNumber + " : " + neat.best.Fitness + " Nodes: " + neat.best.Nodes.Count + " Connections: " + neat.best.Connections.Count);
                    counter++;
                }
                if (counter == maxNumberOfGenerations && neat.best.Fitness != 4)
                {
                    failedTimes++;
                    Console.WriteLine("FAILED");
                }
                else
                {
                    bestGenomeNodeCount.Add(neat.best.Nodes.Count);
                    numberOfGenerations.Add(neat.GenerationNumber);
                }
            }

            // Print stat
            double averageSolutionNodeCount = 0;
            int minSolutionNodeCount = int.MaxValue;
            int maxSolutionNodeCount = int.MinValue;
            foreach (int n in bestGenomeNodeCount)
            {
                averageSolutionNodeCount += n;
                if (n < minSolutionNodeCount)
                    minSolutionNodeCount = n;
                if (n > maxSolutionNodeCount)
                    maxSolutionNodeCount = n; ;
            }
            averageSolutionNodeCount /= bestGenomeNodeCount.Count;

            double averageSolutionGenerationNumber = 0;
            int minSolutionGenerationNumber = int.MaxValue;
            int maxSolutionGenerationNumber = int.MinValue;
            foreach (int n in numberOfGenerations)
            {
                averageSolutionGenerationNumber += n;
                if (n < minSolutionGenerationNumber)
                    minSolutionGenerationNumber = n;
                if (n > maxSolutionGenerationNumber)
                    maxSolutionGenerationNumber = n; ;
            }
            averageSolutionGenerationNumber /= numberOfGenerations.Count;

            double probabilityOfFindingBestSolution = 0;
            foreach (int n in bestGenomeNodeCount)
            {
                if (n == minSolutionNodeCount)
                    probabilityOfFindingBestSolution++;
            }
            probabilityOfFindingBestSolution /= bestGenomeNodeCount.Count;

            Console.WriteLine("-----------------STAT-----------------");
            Console.WriteLine("Solution node count");
            Console.WriteLine("Min: " + minSolutionNodeCount + " Max: " + maxSolutionNodeCount + " Avg : " + averageSolutionNodeCount);
            Console.WriteLine("Solution generation number");
            Console.WriteLine("Min: " + minSolutionGenerationNumber + " Max: " + maxSolutionGenerationNumber + " Avg : " + averageSolutionGenerationNumber);
            Console.WriteLine("Failed times: " + failedTimes);
            Console.WriteLine("Best solution was found in " + probabilityOfFindingBestSolution * 100 + " % of algorithm run.");
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
            Algorithms.EvaluateNetwork(gCopy, new Dictionary<int, double> { { 0, 0 }, { 1, 0 } }, ActivationFunction, true);
            if (Math.Round(gCopy.GetNode(2).Value, MidpointRounding.ToPositiveInfinity) == 0)
                fitness++;

            gCopy = g.Copy();
            Algorithms.EvaluateNetwork(gCopy, new Dictionary<int, double> { { 0, 0 }, { 1, 1 } }, ActivationFunction, true);
            if (Math.Round(gCopy.GetNode(2).Value, MidpointRounding.ToPositiveInfinity) == 1)
                fitness++;

            gCopy = g.Copy();
            Algorithms.EvaluateNetwork(gCopy, new Dictionary<int, double> { { 0, 1 }, { 1, 0 } }, ActivationFunction, true);
            if (Math.Round(gCopy.GetNode(2).Value, MidpointRounding.ToPositiveInfinity) == 1)
                fitness++;

            gCopy = g.Copy();
            Algorithms.EvaluateNetwork(gCopy, new Dictionary<int, double> { { 0, 1 }, { 1, 1 } }, ActivationFunction, true);
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
