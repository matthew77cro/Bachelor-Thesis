using System;
using System.Collections.Generic;

namespace NEAT
{

    class NodeMarkings
    {
        private static readonly Dictionary<int, NodeMarkings> dict = new Dictionary<int, NodeMarkings>();
        private static int nextId = 0;

        public enum NodeType
        {
            SENSOR,
            HIDDEN,
            OUTPUT
        }

        public int ID { get; private set; }
        public NodeType Type { get; private set; }

        public NodeMarkings(NodeType type)
        {
            Type = type;
            ID = nextId++;
            dict.Add(ID, this);
        }

        public override bool Equals(object obj)
        {
            return obj is NodeMarkings markings &&
                   ID == markings.ID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID);
        }

        public static NodeMarkings GetMarkings(int id)
        {
            if (!dict.ContainsKey(id))
                return null;

            return dict[id];
        }
    }

    class Node
    {
        
        public int ID { get; private set; }
        public int DistanceFromSensors { get; set; }
        public double Value { get; set; }
        public bool ValueCalculated { get; set; }

        public Node(int id, int distanceFromSensors)
        {
            ID = id;
            DistanceFromSensors = distanceFromSensors;
        }

        private Node()
        {

        }

        public Node Copy()
        {
            return new Node()
            {
                ID = this.ID,
                DistanceFromSensors = this.DistanceFromSensors,
                Value = this.Value,
                ValueCalculated = this.ValueCalculated
            };
        }

    }

    class ConnectionMarkings
    {
        private static readonly Dictionary<int, ConnectionMarkings> dict = new Dictionary<int, ConnectionMarkings>();
        private static readonly Dictionary<int, Dictionary<int, ConnectionMarkings>> dict2 = new Dictionary<int, Dictionary<int, ConnectionMarkings>>();
        private static int nextId = 0;

        public int Innovation { get; private set; }
        public int In { get; private set; }
        public int Out { get; private set; }

        public ConnectionMarkings(int inNode, int outNode)
        {
            if (dict2.ContainsKey(inNode) && dict2[inNode].ContainsKey(outNode))
                throw new Exception("NodeMarkings already exist!");

            In = inNode;
            Out = outNode;
            Innovation = nextId++;
            dict.Add(Innovation, this);

            if (!dict2.ContainsKey(inNode)) dict2.Add(inNode, new Dictionary<int, ConnectionMarkings>());
            dict2[inNode].Add(outNode, this);
        }

        public override bool Equals(object obj)
        {
            return obj is ConnectionMarkings markings &&
                   Innovation == markings.Innovation;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Innovation);
        }

        public static ConnectionMarkings GetMarkings(int innovation)
        {
            if (!dict.ContainsKey(innovation))
                return null;

            return dict[innovation];
        }

        public static ConnectionMarkings GetMarkings(int inNode, int outNode)
        {
            if (!dict2.ContainsKey(inNode) || !dict2[inNode].ContainsKey(outNode))
                return null;

            return dict2[inNode][outNode];
        }
    }

    class Connection
    {

        public int Innovation { get; private set; }
        public bool Enabled { get; set; }
        public double Weight { get; set; }

        public Connection(int innovation)
        {
            Innovation = innovation;
            Enabled = true;
        }

        private Connection()
        {

        }

        public Connection Copy()
        {
            return new Connection()
            {
                Innovation = this.Innovation,
                Enabled = this.Enabled,
                Weight = this.Weight
            };
        }

    }

    class Genom
    {

        private long version = long.MinValue;

        private readonly SortedDictionary<int, Node> nodes; // all nodes (id -> node)
        private readonly SortedDictionary<int, Node> sensorNodes; // just sensor nodes
        private readonly SortedDictionary<int, Node> hiddenNodes; // just hidden nodes
        private readonly SortedDictionary<int, Node> outputNodes; // just output nodes
        private readonly SortedDictionary<int, Connection> connections; // all connections (innovation -> connection)
        private readonly SortedDictionary<int, Dictionary<int, Connection>> inputs; // inputs : nodeX -> (inputNodesToNodeX -> ConnectionThatConnectsThoseTwoNodes)
        
        public double Fitness { get; set; }
        public bool FitnessCalculated { get; set; }

        public Genom()
        {
            nodes = new SortedDictionary<int, Node>();
            sensorNodes = new SortedDictionary<int, Node>();
            hiddenNodes = new SortedDictionary<int, Node>();
            outputNodes = new SortedDictionary<int, Node>();
            connections = new SortedDictionary<int, Connection>();
            inputs = new SortedDictionary<int, Dictionary<int, Connection>>();
            Fitness = double.MinValue;
            FitnessCalculated = false;
        }

        public void RecalculateNodesDistance()
        {
            foreach (var n in nodes.Values)
            {
                n.DistanceFromSensors = -1;
            }

            foreach (var n in nodes.Values)
            {
                UpdateNodeDistanceRec(n);
            }
        }

        private void UpdateNodeDistanceRec(Node n)
        {

            if (n.DistanceFromSensors != -1)
                return;

            NodeMarkings nm = NodeMarkings.GetMarkings(n.ID);

            if (nm.Type == NodeMarkings.NodeType.SENSOR)
            {
                n.DistanceFromSensors = 0;
                return;
            }
            else if (nm.Type == NodeMarkings.NodeType.OUTPUT)
            {
                n.DistanceFromSensors = int.MaxValue;
                return;
            }
            else if (!inputs.ContainsKey(n.ID) || inputs[n.ID].Count == 0)
            {
                n.DistanceFromSensors = -1;
                return;
            }

            int maxValue = int.MinValue;
            foreach (var input in inputs[n.ID])
            {
                if (!input.Value.Enabled)
                    continue;
                int nodeId = input.Key;
                var node = nodes[nodeId];
                UpdateNodeDistanceRec(node);
                if (node.DistanceFromSensors + 1 > maxValue)
                    maxValue = node.DistanceFromSensors + 1;
            }

            n.DistanceFromSensors = maxValue;

            return;

        }

        public Genom Copy()
        {
            var g = new Genom()
            {
                Fitness = this.Fitness,
                FitnessCalculated = this.FitnessCalculated,
            };

            foreach (var kvp in nodes)
            {
                var nm = NodeMarkings.GetMarkings(kvp.Key);
                var n = kvp.Value.Copy();
                g.nodes.Add(kvp.Key, n);
                switch (nm.Type)
                {
                    case NodeMarkings.NodeType.SENSOR:
                        g.sensorNodes.Add(kvp.Key, n);
                        break;
                    case NodeMarkings.NodeType.HIDDEN:
                        g.hiddenNodes.Add(kvp.Key, n);
                        break;
                    case NodeMarkings.NodeType.OUTPUT:
                        g.outputNodes.Add(kvp.Key, n);
                        break;
                }
            }

            foreach (var kvp in connections)
            {
                var cm = ConnectionMarkings.GetMarkings(kvp.Key);
                var c = kvp.Value.Copy();
                g.connections.Add(kvp.Key, c);
                if (!g.inputs.ContainsKey(cm.Out)) g.inputs.Add(cm.Out, new Dictionary<int, Connection>());
                g.inputs[cm.Out].Add(cm.In, c);
            }

            return g;
        }

        public void AddNode(Node node)
        {
            if (nodes.ContainsKey(node.ID))
                throw new Exception("NodeId already exists in this genom");

            var nm = NodeMarkings.GetMarkings(node.ID);

            if (nm == null)
                throw new Exception("NodeId does not exist");

            int distance = -1;
            if (nm.Type == NodeMarkings.NodeType.SENSOR) distance = 0;
            if (nm.Type == NodeMarkings.NodeType.OUTPUT) distance = int.MaxValue;
            node.DistanceFromSensors = distance;

            nodes.Add(node.ID, node);
            switch (nm.Type)
            {
                case NodeMarkings.NodeType.SENSOR:
                    sensorNodes.Add(node.ID, node);
                    break;
                case NodeMarkings.NodeType.HIDDEN:
                    hiddenNodes.Add(node.ID, node);
                    break;
                case NodeMarkings.NodeType.OUTPUT:
                    outputNodes.Add(node.ID, node);
                    break;
            }

            this.version++;
        }

        public void AddConnection(Connection connection)
        {
            if (connections.ContainsKey(connection.Innovation))
                throw new Exception("Duplicate NodeMarkings");

            var cm = ConnectionMarkings.GetMarkings(connection.Innovation);
            if (cm == null)
                throw new Exception("NodeMarkings innovation does not exist");

            if (!nodes.ContainsKey(cm.In) || !nodes.ContainsKey(cm.Out))
                throw new Exception("Invalid connection : nodes do not exist");

            connections.Add(connection.Innovation, connection);

            if (!inputs.ContainsKey(cm.Out)) inputs.Add(cm.Out, new Dictionary<int, Connection>());
            inputs[cm.Out].Add(cm.In, connection);

            int d;
            if (nodes[cm.Out].DistanceFromSensors < (d = nodes[cm.In].DistanceFromSensors + 1))
                nodes[cm.Out].DistanceFromSensors = d;

            this.version++;
        }

        public Node GetNode(int id)
        {
            if (!nodes.ContainsKey(id))
                return null;
            return nodes[id];
        }

        public Connection GetConnection(int innovation)
        {
            if (!connections.ContainsKey(innovation))
                return null;
            return connections[innovation];
        }

        public void AddNodeMutation(int connectionToSplitInnovationNumber, int newNodeId)
        {

            if (NodeMarkings.GetMarkings(newNodeId) == null || NodeMarkings.GetMarkings(newNodeId).Type != NodeMarkings.NodeType.HIDDEN)
                throw new Exception("newNodeId does not exist as NodeMarkings or type is incorrect");
            if (nodes.ContainsKey(newNodeId))
                throw new Exception("Node with given id already exists");
            if (!connections.ContainsKey(connectionToSplitInnovationNumber))
                throw new Exception("Connection with given innovation does not exist in this genom");

            Connection old = connections[connectionToSplitInnovationNumber];
            old.Enabled = false;
            ConnectionMarkings oldM = ConnectionMarkings.GetMarkings(old.Innovation);

            ConnectionMarkings cm1 = ConnectionMarkings.GetMarkings(oldM.In, newNodeId);
            if (cm1 == null) cm1 = new ConnectionMarkings(oldM.In, newNodeId);
            ConnectionMarkings cm2 = ConnectionMarkings.GetMarkings(newNodeId, oldM.Out);
            if (cm2 == null) cm2 = new ConnectionMarkings(newNodeId, oldM.Out);

            Connection c1 = new Connection(cm1.Innovation)
            {
                Weight = 1
            };
            Connection c2 = new Connection(cm2.Innovation)
            {
                Weight = old.Weight
            };

            Node n = new Node(newNodeId, nodes[oldM.In].DistanceFromSensors + 1);
            NodeMarkings nm = NodeMarkings.GetMarkings(newNodeId);

            nodes.Add(newNodeId, n);
            switch (nm.Type)
            {
                case NodeMarkings.NodeType.SENSOR:
                    sensorNodes.Add(newNodeId, n);
                    break;
                case NodeMarkings.NodeType.HIDDEN:
                    hiddenNodes.Add(newNodeId, n);
                    break;
                case NodeMarkings.NodeType.OUTPUT:
                    outputNodes.Add(newNodeId, n);
                    break;
            }

            connections.Add(cm1.Innovation, c1);
            connections.Add(cm2.Innovation, c2);
            inputs[oldM.Out].Add(newNodeId, c2);
            inputs.Add(newNodeId, new Dictionary<int, Connection>());
            inputs[newNodeId].Add(oldM.In, c1);

            inputs[oldM.Out].Remove(oldM.In);

            this.version++;

            RecalculateNodesDistance();
        }

        public void AddConnectionMutation(int innovation)
        {

            var cm = ConnectionMarkings.GetMarkings(innovation);
            if (cm == null)
                throw new Exception("ConnectionMarkings with given innovation do not exist");
            if (connections.ContainsKey(cm.Innovation))
                throw new Exception("Connection with given innovation already exists");
            if (!nodes.ContainsKey(cm.In) || !nodes.ContainsKey(cm.Out))
                throw new Exception("Nodes that this connections connects do not exist");
            if (nodes[cm.Out].DistanceFromSensors <= nodes[cm.In].DistanceFromSensors)
                throw new Exception("Distance from sensors error -> FFN (feed-froward network) violation");

            Connection c = new Connection(innovation);
            connections.Add(innovation, c);
            if (!inputs.ContainsKey(cm.Out)) inputs.Add(cm.Out, new Dictionary<int, Connection>());
            inputs[cm.Out].Add(cm.In, c);

            int d;
            if (nodes[cm.Out].DistanceFromSensors < (d = nodes[cm.In].DistanceFromSensors + 1))
                nodes[cm.Out].DistanceFromSensors = d;

            this.version++;

            RecalculateNodesDistance();

        }

        public IEnumerable<Node> Nodes()
        {
            long savedVersion = this.version;
            foreach (var kvp in nodes)
            {
                if (savedVersion != this.version)
                    throw new Exception("Concurrent modification");

                yield return kvp.Value;
            }
        }

        public IEnumerable<Node> SensorNodes()
        {
            long savedVersion = this.version;
            foreach (var kvp in sensorNodes)
            {
                if (savedVersion != this.version)
                    throw new Exception("Concurrent modification");

                yield return kvp.Value;
            }
        }

        public IEnumerable<Node> HiddenNodes()
        {
            long savedVersion = this.version;
            foreach (var kvp in hiddenNodes)
            {
                if (savedVersion != this.version)
                    throw new Exception("Concurrent modification");

                yield return kvp.Value;
            }
        }

        public IEnumerable<Node> OutputNodes()
        {
            long savedVersion = this.version;
            foreach (var kvp in outputNodes)
            {
                if (savedVersion != this.version)
                    throw new Exception("Concurrent modification");

                yield return kvp.Value;
            }
        }

        public IEnumerable<Connection> Connections()
        {
            long savedVersion = this.version;
            foreach (var kvp in connections)
            {
                if (savedVersion != this.version)
                    throw new Exception("Concurrent modification");

                yield return kvp.Value;
            }
        }

        public IEnumerable<Connection> Inputs(int nodeId)
        {
            long savedVersion = this.version;

            if (inputs.ContainsKey(nodeId))
                foreach (var kvp in inputs[nodeId])
                {
                    if (savedVersion != this.version)
                        throw new Exception("Concurrent modification");

                    yield return kvp.Value;
                }
        }

    }

    static class Algorithms
    {

        public delegate Connection GeneChooser(Genom parent1, Genom parent2, int innovation);
        public delegate double GetRandomConnectionWeight(double oldWeightValue);
        public delegate double ActivationFunction(double value);

        private static readonly Random rnd = new Random();

        public static Genom Crossover(Genom parent1, Genom parent2, GeneChooser chooser)
        {
            Genom offspring = new Genom();

            var enumerator1 = parent1.Connections().GetEnumerator();
            var enumerator2 = parent2.Connections().GetEnumerator();

            byte moreFitParent = parent1.Fitness > parent2.Fitness ? (byte)1 : (byte)2;
            moreFitParent = parent1.Fitness == parent2.Fitness ? (byte)0 : moreFitParent;

            bool has1 = enumerator1.MoveNext(), has2 = enumerator2.MoveNext();
            while(has1 || has2)
            {
                Connection c = null;

                if (has1 && has2)
                {
                    if (enumerator1.Current.Innovation == enumerator2.Current.Innovation)
                    {
                        c = chooser(parent1, parent2, enumerator1.Current.Innovation);
                        has1 = enumerator1.MoveNext();
                        has2 = enumerator2.MoveNext();
                    }
                    else if (enumerator1.Current.Innovation > enumerator2.Current.Innovation)
                    {
                        if (moreFitParent != (byte)1)
                        {
                            has2 = enumerator2.MoveNext();
                            continue;
                        }

                        c = enumerator2.Current;
                        has2 = enumerator2.MoveNext();
                    }
                    else
                    {
                        if (moreFitParent == (byte)2)
                        {
                            has1 = enumerator1.MoveNext();
                            continue;
                        }

                        c = enumerator1.Current;
                        has1 = enumerator1.MoveNext();
                    }
                }
                else if(has1 && !has2)
                {
                    if (moreFitParent == (byte)2)
                    {
                        break;
                    }

                    c = enumerator1.Current;
                    has1 = enumerator1.MoveNext();
                }
                else if(!has1 && has2)
                {
                    if (moreFitParent == (byte)1)
                    {
                        break;
                    }

                    c = enumerator2.Current;
                    has2 = enumerator2.MoveNext();
                }

                var cm = ConnectionMarkings.GetMarkings(c.Innovation);

                if (offspring.GetNode(cm.In) == null)
                    offspring.AddNode(new Node(cm.In, -1));
                if (offspring.GetNode(cm.Out) == null)
                    offspring.AddNode(new Node(cm.Out, -1));

                Connection nc = new Connection(cm.Innovation);
                offspring.AddConnection(nc);

            }

            offspring.RecalculateNodesDistance();

            return offspring;
        }

        public static void ConnectionSingleWeightMutation(Genom g, double uniformPerturbationRate)
        {
            foreach (var conn in g.Connections())
            {
                if (rnd.NextDouble() < uniformPerturbationRate)
                    conn.Weight += SmallValueNoramalDistr(conn.Weight - 1, 1 - conn.Weight);
                else
                    conn.Weight = rnd.NextDouble();
            }
        }

        private static double SmallValueNoramalDistr(double lowerBond, double upperBond)
        {
            // Box - Muller
            double range = upperBond - lowerBond;

            double value;
            do
            {
                double u1 = rnd.NextDouble();
                double u2 = rnd.NextDouble();

                double z = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);

                value = ((range / 2) / 3) * z + range / 2;
            } while (value < lowerBond || value > upperBond); // 99.7% chance value will fall within bonds

            return value;
        }

        public static void EvaluateNetwork(Genom g, Dictionary<int, double> inputValues, ActivationFunction af) // inputValues : sensorNodeId -> inputValue; return : nodeId -> value (for each node)
        {
            foreach (var node in g.Nodes())
            {
                node.ValueCalculated = false;
            }

            foreach (var outNode in g.OutputNodes())
            {
                EvaluateNodeRec(g, outNode.ID, inputValues, af);
            }
        }

        private static void EvaluateNodeRec(Genom g, int nodeId, Dictionary<int, double> values, ActivationFunction af)
        {

            var node = g.GetNode(nodeId);
            if (node.ValueCalculated)
                return;

            if (NodeMarkings.GetMarkings(nodeId).Type == NodeMarkings.NodeType.SENSOR)
            {
                if (!values.ContainsKey(nodeId))
                    throw new Exception("No value for node id : " + nodeId);
                node.Value = values[nodeId];
                node.ValueCalculated = true;
                return;
            }

            node.Value = 0;
            bool inputsExist = false;
            foreach (var inputConn in g.Inputs(nodeId))
            {
                if (!inputConn.Enabled)
                    continue;
                inputsExist = true;
                var inNode = g.GetNode(ConnectionMarkings.GetMarkings(inputConn.Innovation).In);
                if (!inNode.ValueCalculated)
                    EvaluateNodeRec(g, inNode.ID, values, af);
                node.Value += inNode.Value * inputConn.Weight;
            }

            if(inputsExist)
                node.Value = af(node.Value);
            node.ValueCalculated = true;

        }

    }

}
