using NEAT;
using System;
using System.Collections.Generic;

namespace NEATTest
{
    class NEATTestAlgorithms
    {

        private static Random rnd = new Random();

        static void Main(string[] args)
        {

            int[] nmIds = new int[17];

            for (int i = 0; i < 4; i++)
                nmIds[i] = new NodeMarkings(NodeMarkings.NodeType.SENSOR).ID;
            for (int i = 4; i < 15; i++)
                nmIds[i] = new NodeMarkings(NodeMarkings.NodeType.HIDDEN).ID;
            for (int i = 15; i < 17; i++)
                nmIds[i] = new NodeMarkings(NodeMarkings.NodeType.OUTPUT).ID;

            int[] cmInnovations = new int[21];
            cmInnovations[0] = new ConnectionMarkings(nmIds[0], nmIds[9]).Innovation;
            cmInnovations[1] = new ConnectionMarkings(nmIds[0], nmIds[4]).Innovation;
            cmInnovations[2] = new ConnectionMarkings(nmIds[1], nmIds[14]).Innovation;
            cmInnovations[3] = new ConnectionMarkings(nmIds[1], nmIds[5]).Innovation;
            cmInnovations[4] = new ConnectionMarkings(nmIds[2], nmIds[6]).Innovation;
            cmInnovations[5] = new ConnectionMarkings(nmIds[2], nmIds[7]).Innovation;
            cmInnovations[6] = new ConnectionMarkings(nmIds[3], nmIds[8]).Innovation;
            cmInnovations[7] = new ConnectionMarkings(nmIds[4], nmIds[9]).Innovation;
            cmInnovations[8] = new ConnectionMarkings(nmIds[5], nmIds[10]).Innovation;
            cmInnovations[9] = new ConnectionMarkings(nmIds[6], nmIds[10]).Innovation;
            cmInnovations[10] = new ConnectionMarkings(nmIds[6], nmIds[11]).Innovation;
            cmInnovations[11] = new ConnectionMarkings(nmIds[7], nmIds[12]).Innovation;
            cmInnovations[12] = new ConnectionMarkings(nmIds[8], nmIds[13]).Innovation;
            cmInnovations[13] = new ConnectionMarkings(nmIds[9], nmIds[16]).Innovation;
            cmInnovations[14] = new ConnectionMarkings(nmIds[9], nmIds[14]).Innovation;
            cmInnovations[15] = new ConnectionMarkings(nmIds[10], nmIds[14]).Innovation;
            cmInnovations[16] = new ConnectionMarkings(nmIds[11], nmIds[14]).Innovation;
            cmInnovations[17] = new ConnectionMarkings(nmIds[11], nmIds[15]).Innovation;
            cmInnovations[18] = new ConnectionMarkings(nmIds[12], nmIds[15]).Innovation;
            cmInnovations[19] = new ConnectionMarkings(nmIds[13], nmIds[15]).Innovation;
            cmInnovations[20] = new ConnectionMarkings(nmIds[14], nmIds[16]).Innovation;

            Genom g = new Genom();

            for (int i = 0; i < nmIds.Length; i++)
            {
                g.AddNode(new Node(nmIds[i]));
            }

            for (int i = 0; i < cmInnovations.Length; i++)
            {
                g.AddConnection(new Connection(cmInnovations[i])
                {
                    Weight = 0.01 * (i + 1)
                });
            }

            Test1(nmIds, cmInnovations, g.Copy());
            Test2(nmIds, cmInnovations, g.Copy());
            Test3(nmIds, cmInnovations, g.Copy());
            Test4();

        }

        private static void Test1(int[] nmIds, int[] cmInnovations, Genom g)
        {
            Console.WriteLine("------------------------Test1 started------------------------");

            bool passed = true;

            NEAT.Algorithms.EvaluateNetwork(g,
                new Dictionary<int, double>() { { nmIds[0], 5 },
                                                { nmIds[1], 6 },
                                                { nmIds[2], 7 },
                                                { nmIds[3], 8 },}, (x) => x, true);

            passed = passed && Check("Value", nmIds[15], g.GetNode(nmIds[15]).Value, 0.031066);
            passed = passed && Check("Value", nmIds[16], g.GetNode(nmIds[16]).Value, 0.05102321);

            Console.WriteLine("Test " + (passed ? "passed" : "failed"));
            Console.WriteLine("------------------------Test1 end------------------------");
        }
        
        private static void Test2(int[] nmIds, int[] cmInnovations, Genom g)
        {
            Console.WriteLine("------------------------Test2 started------------------------");

            bool passed = true;

            var newnm = new NodeMarkings(NodeMarkings.NodeType.HIDDEN);
            g.AddNodeMutation(cmInnovations[11], newnm.ID);

            NEAT.Algorithms.EvaluateNetwork(g,
                new Dictionary<int, double>() { { nmIds[0], 5 },
                                                { nmIds[1], 6 },
                                                { nmIds[2], 7 },
                                                { nmIds[3], 8 },}, (x) => x, true);

            passed = passed && Check("Value", nmIds[15], g.GetNode(nmIds[15]).Value, 0.031066);
            passed = passed && Check("Value", nmIds[16], g.GetNode(nmIds[16]).Value, 0.05102321);

            Console.WriteLine("Test " + (passed ? "passed" : "failed"));
            Console.WriteLine("------------------------Test2 end------------------------");
        }

        private static void Test3(int[] nmIds, int[] cmInnovations, Genom g)
        {
            Console.WriteLine("------------------------Test3 started------------------------");

            bool passed = true;

            var newcm = new ConnectionMarkings(nmIds[5], nmIds[9]);
            g.AddConnectionMutation(newcm.Innovation, 0.7);

            NEAT.Algorithms.EvaluateNetwork(g,
                new Dictionary<int, double>() { { nmIds[0], 5 },
                                                { nmIds[1], 6 },
                                                { nmIds[2], 7 },
                                                { nmIds[3], 8 },}, (x) => x, true);

            passed = passed && Check("Value", nmIds[15], g.GetNode(nmIds[15]).Value, 0.031066);
            passed = passed && Check("Value", nmIds[16], g.GetNode(nmIds[16]).Value, 0.07983521);

            Console.WriteLine("Test " + (passed ? "passed" : "failed"));
            Console.WriteLine("------------------------Test3 end------------------------");
        }

        private static void Test4()
        {

            Console.WriteLine("------------------------Test4 started------------------------");

            bool passed = true;

            Console.WriteLine("Crossover testing");

            int[] nodeIds = new int[6];
            nodeIds[0] = new NodeMarkings(NodeMarkings.NodeType.SENSOR).ID;
            nodeIds[1] = new NodeMarkings(NodeMarkings.NodeType.SENSOR).ID;
            nodeIds[2] = new NodeMarkings(NodeMarkings.NodeType.SENSOR).ID;
            nodeIds[3] = new NodeMarkings(NodeMarkings.NodeType.OUTPUT).ID;
            nodeIds[4] = new NodeMarkings(NodeMarkings.NodeType.HIDDEN).ID;
            nodeIds[5] = new NodeMarkings(NodeMarkings.NodeType.HIDDEN).ID;

            int[] connIds = new int[10];
            connIds[0] = new ConnectionMarkings(nodeIds[0], nodeIds[3]).Innovation;
            connIds[1] = new ConnectionMarkings(nodeIds[0], nodeIds[4]).Innovation;
            connIds[2] = new ConnectionMarkings(nodeIds[1], nodeIds[4]).Innovation;
            connIds[3] = new ConnectionMarkings(nodeIds[2], nodeIds[3]).Innovation;
            connIds[4] = new ConnectionMarkings(nodeIds[4], nodeIds[3]).Innovation;
            connIds[5] = new ConnectionMarkings(nodeIds[0], nodeIds[5]).Innovation;
            connIds[6] = new ConnectionMarkings(nodeIds[2], nodeIds[4]).Innovation;
            connIds[7] = new ConnectionMarkings(nodeIds[4], nodeIds[5]).Innovation;
            connIds[8] = new ConnectionMarkings(nodeIds[5], nodeIds[3]).Innovation;
            connIds[9] = new ConnectionMarkings(nodeIds[1], nodeIds[3]).Innovation;

            Genom p1 = new Genom();
            p1.AddNode(new Node(nodeIds[0]));
            p1.AddNode(new Node(nodeIds[1]));
            p1.AddNode(new Node(nodeIds[2]));
            p1.AddNode(new Node(nodeIds[3]));
            p1.AddNode(new Node(nodeIds[4]));
            p1.AddConnection(new Connection(connIds[0]));
            p1.AddConnection(new Connection(connIds[1]));
            p1.AddConnection(new Connection(connIds[2]));
            p1.AddConnection(new Connection(connIds[3]));
            p1.AddConnection(new Connection(connIds[4]));
            p1.AddConnection(new Connection(connIds[9]));

            Genom p2 = new Genom();
            p2.AddNode(new Node(nodeIds[0]));
            p2.AddNode(new Node(nodeIds[1]));
            p2.AddNode(new Node(nodeIds[2]));
            p2.AddNode(new Node(nodeIds[3]));
            p2.AddNode(new Node(nodeIds[4]));
            p2.AddNode(new Node(nodeIds[5]));
            p2.AddConnection(new Connection(connIds[0]));
            p2.AddConnection(new Connection(connIds[5]));
            p2.AddConnection(new Connection(connIds[2]));
            p2.AddConnection(new Connection(connIds[6]));
            p2.AddConnection(new Connection(connIds[3]));
            p2.AddConnection(new Connection(connIds[7]));
            p2.AddConnection(new Connection(connIds[8]));
            p2.AddConnection(new Connection(connIds[9]));
            p2.AddConnection(new Connection(connIds[4]));

            Genom offspring = Algorithms.Crossover(p1, p2, Chooser);

            if (offspring.Connections.Count != 10)
                passed = false;

            Console.WriteLine("Test " + (passed ? "passed" : "failed"));
            Console.WriteLine("------------------------Test4 end------------------------");

        }

        private static bool Check(string functionName, int nodeId, double value, double expected)
        {
            Console.Write(functionName + "(node" + nodeId + ") = " + value + "; Expected = " + expected + " => ");
            if (Math.Abs(value-expected) < 10e-7)
            {
                Console.WriteLine("PASS");
                return true;
            }
            else
            {
                Console.WriteLine("FAIL");
                return false;
            }
        }

        public static Connection Chooser(Genom parent1, Genom parent2, int innovation)
        {

            if (rnd.NextDouble() < 0.5)
                return parent1.GetConnection(innovation);
            else
                return parent2.GetConnection(innovation);
        }

    }
}
