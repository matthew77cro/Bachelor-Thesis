using NEAT;
using System;
using System.Collections.Generic;

namespace TestNN
{
    class TestNN
    {
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
                g.AddNode(new Node(nmIds[i], -1));
            }

            for (int i = 0; i < cmInnovations.Length; i++)
            {
                g.AddConnection(new Connection(cmInnovations[i])
                {
                    Weight = 0.01 * (i + 1)
                });
            }

            NEAT.Algorithms.EvaluateNetwork(g, 
                new Dictionary<int, double>() { { nmIds[0], 5 },
                                                { nmIds[1], 6 },
                                                { nmIds[2], 7 },
                                                { nmIds[3], 8 },}, (x) => x);

            var newnm = new NodeMarkings(NodeMarkings.NodeType.HIDDEN);
            g.AddNodeMutation(cmInnovations[11], newnm.ID);

        }
    }
}
