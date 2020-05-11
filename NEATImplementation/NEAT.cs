﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace NEAT
{

    class NodeMarkings
    {
        private static readonly Dictionary<int, NodeMarkings> dict = new Dictionary<int, NodeMarkings>();
        private static readonly List<int> lst = new List<int>();
        private static int nextId = 0;

        public static IReadOnlyDictionary<int, NodeMarkings> NM
        {
            get
            {
                return (IReadOnlyDictionary<int, NodeMarkings>)dict;
            }
        }
        public static IList<int> IDS
        {
            get
            {
                return lst.AsReadOnly();
            }
        }

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
            lst.Add(ID);
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
        private static readonly List<int> lst = new List<int>();
        private static int nextId = 0;

        public static IReadOnlyDictionary<int, ConnectionMarkings> CM
        {
            get
            {
                return (IReadOnlyDictionary<int, ConnectionMarkings>)dict;
            }
        }
        public static IList<int> Innovations
        {
            get
            {
                return lst.AsReadOnly();
            }
        }

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

            lst.Add(Innovation);
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

        private static Random rnd = new Random();

        private long version = long.MinValue;

        private readonly SortedDictionary<int, Node> nodes; // all nodes (id -> node)
        private readonly SortedDictionary<int, Node> sensorNodes; // just sensor nodes
        private readonly SortedDictionary<int, Node> hiddenNodes; // just hidden nodes
        private readonly SortedDictionary<int, Node> outputNodes; // just output nodes
        private readonly SortedDictionary<int, Connection> connections; // all connections (innovation -> connection)
        private readonly SortedDictionary<int, Dictionary<int, Connection>> inputs; // inputs : nodeX -> (inputNodesToNodeX -> ConnectionThatConnectsThoseTwoNodes)

        public IReadOnlyDictionary<int, Node> Nodes { get { return nodes; } }
        public IReadOnlyDictionary<int, Node> SensorNodes { get { return sensorNodes; } }
        public IReadOnlyDictionary<int, Node> HiddenNodes { get { return hiddenNodes; } }
        public IReadOnlyDictionary<int, Node> OutputNodes { get { return outputNodes; } }
        public IReadOnlyDictionary<int, Connection> Connections { get { return connections; } }

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

            this.version++;

            RecalculateNodesDistance();
        }

        public void AddConnectionMutation(int innovation, double weight)
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

            Connection c = new Connection(innovation)
            {
                Weight = weight
            };
            connections.Add(innovation, c);
            if (!inputs.ContainsKey(cm.Out)) inputs.Add(cm.Out, new Dictionary<int, Connection>());
            inputs[cm.Out].Add(cm.In, c);

            this.version++;

            RecalculateNodesDistance();

        }

        public void ConnectionWeightMutation(double uniformPerturbationRate)
        {
            foreach (var conn in connections.Values)
            {
                if (rnd.NextDouble() < uniformPerturbationRate)
                    conn.Weight += StdNormalDistr();
                else
                    conn.Weight = rnd.NextDouble();
            }
        }

        private static double StdNormalDistr()
        {
            // Box - Muller
            double value;
            double u1 = rnd.NextDouble();
            double u2 = rnd.NextDouble();

            double z = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);

            return z;
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

    class NEATPopulation
    {

        private static Random rnd = new Random();

        public delegate double GetRandomConnectionWeight();
        public delegate double Fitness(Genom g);

        private readonly List<int> inputNodeIds = new List<int>();
        private readonly List<int> outputNodeIds = new List<int>();
        private List<Genom> population = new List<Genom>();

        public int NumberOfInputs { get; }
        public int NumberOfOutputs { get; }
        public int PopulationSize { get; }
        public double WeightMutationRate { get; }
        public double WeightMutationPerturbationRate { get; }
        public double AddNodeMutationRate { get; }
        public double AddConnectionMutationRate { get; }
        public GetRandomConnectionWeight RW { get; }
        public Fitness Calculator { get; }
        public double C1 { get; }
        public double C2 { get; }
        public double C3 { get; }
        public double CompatibilityDistanceThreshold { get; }
        public int GenomsInSpeciesChampionCopyThreshold { get; } // The champion of each species with more than GenomsInSpeciesChampionCopyThreshold networks is copied into the next generation unchanged
        public Algorithms.GeneChooser Chooser { get; }
        public IList<int> InputNodeIds
        {
            get
            {
                return inputNodeIds.AsReadOnly();
            }
        }
        public IList<int> OutputNodeIds
        {
            get
            {
                return outputNodeIds.AsReadOnly();
            }
        }
        public IList<Genom> Population
        {
            get
            {
                return population.AsReadOnly();
            }
        }

        public NEATPopulation(int numOfInputs, int numOfOutputs, int populationSize, double weightMutationRate, double weightMutationPerturbationRate, double addNodeMutationRate, double addConnectionMutationRate, GetRandomConnectionWeight rw, Fitness calculator, double c1, double c2, double c3, double compatibilityDistanceThreshold, int genomsInSpeciesChampionCopyThreshold, Algorithms.GeneChooser chooser)
        {
            NumberOfInputs = numOfInputs;
            NumberOfOutputs = numOfOutputs;
            PopulationSize = populationSize;
            WeightMutationRate = weightMutationRate;
            WeightMutationPerturbationRate = weightMutationPerturbationRate;
            AddNodeMutationRate = addNodeMutationRate;
            AddConnectionMutationRate = addConnectionMutationRate;
            RW = rw;
            Calculator = calculator;
            C1 = c1;
            C2 = c2;
            C3 = c3;
            CompatibilityDistanceThreshold = compatibilityDistanceThreshold;
            GenomsInSpeciesChampionCopyThreshold = genomsInSpeciesChampionCopyThreshold;
            Chooser = chooser;

            for (int i = 0; i < numOfInputs; i++)
                inputNodeIds.Add(new NodeMarkings(NodeMarkings.NodeType.SENSOR).ID);

            for (int i = 0; i < numOfOutputs; i++)
                outputNodeIds.Add(new NodeMarkings(NodeMarkings.NodeType.OUTPUT).ID);

            for (int i = 0; i < populationSize; i++)
            {
                var g = new Genom();

                foreach (int id in inputNodeIds)
                {
                    g.AddNode(new Node(id, 0));
                }

                foreach (int id in outputNodeIds)
                {
                    g.AddNode(new Node(id, int.MaxValue));
                }

                foreach (int inId in inputNodeIds)
                {
                    foreach (int outId in outputNodeIds)
                    {
                        var cm = ConnectionMarkings.GetMarkings(inId, outId);
                        if (cm == null) cm = new ConnectionMarkings(inId, outId);

                        g.AddConnection(new Connection(cm.Innovation)
                        {
                            Weight = rw()
                        });
                    }
                }

                population.Add(g);
            }

            CalculateFitnessAndSortDescending();
        }

        public void Advance()
        {

            List<Genom> newPopulation = new List<Genom>();

            Dictionary<List<Genom>, double> species = SpeciateAndFitnessSum();
            double allSpeciesFitnessSum = 0;

            // The champion of each species with more than GenomsInSpeciesChampionCopyThreshold networks is copied into the next generation unchanged
            // Also calculating allSpeciesFitnessSum

            foreach (var kvp in species)
            {
                var spec = kvp.Key;
                allSpeciesFitnessSum += kvp.Value;

                if (spec.Count > GenomsInSpeciesChampionCopyThreshold)
                {
                    Genom champion = spec[0];
                    double championFitness = double.MinValue;

                    foreach (var genom in spec)
                    {
                        if (genom.Fitness > championFitness)
                        {
                            champion = genom;
                            championFitness = genom.Fitness;
                        }
                    }

                    newPopulation.Add(champion);
                }
            }

            while (newPopulation.Count < PopulationSize)
            {
                var g1 = SelectProportionally(species, 0);
                var g2 = SelectProportionally(species, 0);
                var offspring = Algorithms.Crossover(g1, g2, Chooser);
                Mutate(offspring);
                newPopulation.Add(offspring);
            }

            population = newPopulation;

            CalculateFitnessAndSortDescending();
        }

        private Dictionary<List<Genom>, double> SpeciateAndFitnessSum()
        {
            var species = new List<List<Genom>>();
            var toReturn = new Dictionary<List<Genom>, double>();

            foreach (var genom in population)
            {
                bool foundCompatibility = false;
                foreach (var spec in species)
                {
                    if (CompatibilityDistance(genom, spec[0]) < CompatibilityDistanceThreshold)
                    {
                        foundCompatibility = true;
                        spec.Add(genom);
                        break;
                    }
                }

                if (!foundCompatibility)
                {
                    species.Add(new List<Genom>() { genom });
                }
            }

            // calculate the adjusted fitness

            foreach (var spec in species)
            {
                int sh = spec.Count;
                double specSum = 0;
                foreach (var genom in spec)
                {
                    genom.Fitness /= sh;
                    specSum += genom.Fitness;
                }
                toReturn.Add(spec, specSum);
            }

            return toReturn;
        }

        private double CompatibilityDistance(Genom g1, Genom g2)
        {

            int e = 0, d = 0, n = 0;
            double w = 0;

            var enum1 = g1.Connections.GetEnumerator();
            var enum2 = g2.Connections.GetEnumerator();
            double weightDifferenceOfMatchingGenesSum = 0;
            int numOfMatchingGenes = 0;
            int numOfGenes1 = 0, numOfGenes2 = 0;

            bool has1 = enum1.MoveNext(), has2 = enum2.MoveNext();
            while(has1 || has2)
            {
                if (has1 && has2)
                {
                    if(enum1.Current.Value.Innovation < enum2.Current.Value.Innovation)
                    {
                        d++;
                        has1 = enum1.MoveNext();
                        numOfGenes1++;
                    }
                    else if(enum1.Current.Value.Innovation > enum2.Current.Value.Innovation)
                    {
                        d++;
                        has2 = enum2.MoveNext();
                        numOfGenes2++;
                    }
                    else
                    {
                        weightDifferenceOfMatchingGenesSum += Math.Abs(enum1.Current.Value.Weight - enum2.Current.Value.Weight);
                        numOfMatchingGenes++;
                        has1 = enum1.MoveNext();
                        has2 = enum2.MoveNext();
                        numOfGenes1++;
                        numOfGenes2++;
                    }
                    continue;
                }

                e++;

                if (has1)
                {
                    has1 = enum1.MoveNext();
                    numOfGenes1++;
                }
                else if (has2)
                {
                    has2 = enum2.MoveNext();
                    numOfGenes2++;
                }

            }

            w = weightDifferenceOfMatchingGenesSum / numOfMatchingGenes;
            n = numOfGenes1 > numOfGenes2 ? numOfGenes1 : numOfGenes2;

            return C1 * e / n + C2 * d / n + C3 * w;

        }

        private Genom SelectProportionally(Dictionary<List<Genom>, double> speciesAndFitnessSums, double allSpeciesFitnessSum)
        {
            double random = rnd.NextDouble() * allSpeciesFitnessSum;

            List<Genom> pickedSpecies = null;
            double pickedSpeciesFitness = 0;

            double fitnessSumCount = 0;

            foreach (var kvp in speciesAndFitnessSums)
            {
                fitnessSumCount += kvp.Value;
                pickedSpecies = kvp.Key;
                pickedSpeciesFitness = kvp.Value;

                if (fitnessSumCount > random)
                    break;
            }

            random = rnd.NextDouble() * pickedSpeciesFitness;

            Genom pickedGenom = null;

            fitnessSumCount = 0;

            foreach (var g in pickedSpecies)
            {
                fitnessSumCount += g.Fitness;
                pickedGenom = g;

                if (fitnessSumCount > random)
                    break;
            }

            return pickedGenom;
        }

        private void Mutate(Genom g)
        {
            if (rnd.NextDouble() < WeightMutationRate)
            {
                g.ConnectionWeightMutation(WeightMutationPerturbationRate);
            }

            if (rnd.NextDouble() < AddNodeMutationRate)
            {
                int nmId = -1;

                if (NodeMarkings.IDS.Count == g.Nodes.Count)
                {
                    nmId = new NodeMarkings(NodeMarkings.NodeType.HIDDEN).ID;
                }
                else
                {
                    foreach (int id in NodeMarkings.IDS)
                    {
                        if (g.GetNode(id) == null)
                        {
                            nmId = id;
                            break;
                        }
                    }
                }

                g.AddNodeMutation(g.Connections.ElementAt(rnd.Next(0, g.Connections.Count)).Key, nmId);
            }

            if (rnd.NextDouble() < AddConnectionMutationRate)
            {

                int innovation = -1;
                bool found = false;

                foreach (var node1 in g.Nodes.Values)
                {

                    if (NodeMarkings.GetMarkings(node1.ID).Type == NodeMarkings.NodeType.OUTPUT)
                        continue;

                    foreach (var node2 in g.Nodes.Values)
                    {
                        ConnectionMarkings cm = ConnectionMarkings.GetMarkings(node1.ID, node2.ID);
                        if (node1.ID == node2.ID || 
                            NodeMarkings.GetMarkings(node2.ID).Type == NodeMarkings.NodeType.SENSOR ||
                            (cm != null && g.Connections.ContainsKey(cm.Innovation)) ||
                            node1.DistanceFromSensors >= node2.DistanceFromSensors)
                            continue;

                        found = true;
                        innovation = cm == null ? new ConnectionMarkings(node1.ID, node2.ID).Innovation : cm.Innovation;

                    }

                    if (found) break;
                }

                if (found)
                    g.AddConnectionMutation(innovation, RW());

            }

            g.RecalculateNodesDistance();
        }

        private void CalculateFitnessAndSortDescending()
        {

            foreach (var g in population)
            {
                g.Fitness = Calculator(g);
                g.FitnessCalculated = true;
            }

            population.Sort((g1, g2) => g2.Fitness.CompareTo(g1.Fitness));

        }

    }

    static class Algorithms
    {

        public delegate Connection GeneChooser(Genom parent1, Genom parent2, int innovation);
        public delegate double ActivationFunction(double value);

        private static readonly Random rnd = new Random();

        public static Genom Crossover(Genom parent1, Genom parent2, GeneChooser chooser)
        {
            Genom offspring = new Genom();

            var enumerator1 = parent1.Connections.Values.GetEnumerator();
            var enumerator2 = parent2.Connections.Values.GetEnumerator();

            byte moreFitParent = parent1.Fitness > parent2.Fitness ? (byte)1 : (byte)2;
            moreFitParent = parent1.Fitness == parent2.Fitness ? (byte)rnd.Next(1, 3) : moreFitParent;

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
                        if (moreFitParent == (byte)1)
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

                Connection nc = new Connection(cm.Innovation)
                {
                    Enabled = c.Enabled,
                    Weight = c.Weight
                };
                offspring.AddConnection(nc);

            }

            offspring.RecalculateNodesDistance();

            return offspring;
        }        

        public static void EvaluateNetwork(Genom g, Dictionary<int, double> inputValues, ActivationFunction af) // inputValues : sensorNodeId -> inputValue; return : nodeId -> value (for each node)
        {
            foreach (var node in g.Nodes.Values)
            {
                node.ValueCalculated = false;
            }

            foreach (var outNode in g.OutputNodes.Values)
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
