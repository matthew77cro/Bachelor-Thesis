using NEAT;
using System;

namespace NEATTest
{
    class NEATTestRecurrentEvaluation
    {

        public static void Main()
        {

            int[] nmIds = new int[6];

            for (int i = 0; i < 2; i++)
                nmIds[i] = new NodeMarkings(NodeMarkings.NodeType.SENSOR).ID;
            for (int i = 2; i < 5; i++)
                nmIds[i] = new NodeMarkings(NodeMarkings.NodeType.HIDDEN).ID;
            for (int i = 5; i < 6; i++)
                nmIds[i] = new NodeMarkings(NodeMarkings.NodeType.OUTPUT).ID;

            int[] cmInnovations = new int[6];
            cmInnovations[0] = new ConnectionMarkings(nmIds[0], nmIds[2]).Innovation;
            cmInnovations[1] = new ConnectionMarkings(nmIds[1], nmIds[2]).Innovation;
            cmInnovations[2] = new ConnectionMarkings(nmIds[2], nmIds[3]).Innovation;
            cmInnovations[3] = new ConnectionMarkings(nmIds[2], nmIds[4]).Innovation;
            cmInnovations[4] = new ConnectionMarkings(nmIds[3], nmIds[2]).Innovation;
            cmInnovations[5] = new ConnectionMarkings(nmIds[4], nmIds[5]).Innovation;

            double[] weights = new double[] { 0.5, 0.4, 0.1, 0.2, 0.2, 0.6 };

            Genom g = new Genom();

            for (int i = 0; i < nmIds.Length; i++)
            {
                g.AddNode(new Node(nmIds[i]));
            }

            for (int i = 0; i < cmInnovations.Length; i++)
            {
                g.AddConnection(new Connection(cmInnovations[i])
                {
                    Weight = weights[i]
                });
            }

            Algorithms.EvaluateNetwork(g, new System.Collections.Generic.Dictionary<int, double> { { 0, 1 }, { 1, 1 } }, x => x, true);
            if (Math.Abs(g.GetNode(nmIds[5]).Value - 0.108) > 1E-7)
            {
                Console.WriteLine("Pass1 FAILED");
            }
            else
            {
                Console.WriteLine("Pass1 SUCCESS");
            }

            Algorithms.EvaluateNetwork(g, new System.Collections.Generic.Dictionary<int, double> { { 0, 1 }, { 1, 1 } }, x => x, false);
            if (Math.Abs(g.GetNode(nmIds[5]).Value - 0.11016) > 1E-7)
            {
                Console.WriteLine("Pass2 FAILED");
            }
            else
            {
                Console.WriteLine("Pass2 SUCCESS");
            }

        }

    }
}
