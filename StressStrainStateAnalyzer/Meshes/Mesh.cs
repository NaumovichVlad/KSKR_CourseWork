using StressStrainStateAnalyzer.Extensions;
using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.MeshBuilders;
using StressStrainStateAnalyzer.Nodes;

namespace StressStrainStateAnalyzer.Meshes
{
    public class Mesh
    {
        private List<int> _forcedNodesIndexes = new List<int>();
        private List<INode> _nodes;
        public List<IFiniteElement> FiniteElements { get; set; }
        public double[,] GlobalStiffnessMatrix { get; set; }
        public double MaxStress { get; set; }
        public double MaxXDeformation { get; set; }
        public double MaxYDeformation { get; set; }
        public double MinXDeformation { get; set; }
        public double MinYDeformation { get; set; }


        public Mesh(IMeshBuilder builder, List<INode> nodes, double maxSquare, double angle)
        {
            FiniteElements = builder.BuildMesh(nodes, maxSquare, angle);
            _nodes = new List<INode>();
            FiniteElements.ForEach(f => _nodes.AddRange(f.Nodes));
            _nodes = _nodes.Distinct().OrderBy(n => n.X).ThenBy(n => n.Y).ToList();
            AddIndexes();
            GlobalStiffnessMatrix = new double[_nodes.Count * 2, _nodes.Count * 2 + 1];
        }

        public Mesh(List<IFiniteElement> finiteElements, List<INode> nodes)
        {
            _nodes = nodes;
            FiniteElements = finiteElements;
            GlobalStiffnessMatrix = new double[_nodes.Count * 2, _nodes.Count * 2 + 1];
        }

        private void AddIndexes()
        {
            for (var i = 0; i < _nodes.Count; i++)
                _nodes[i].Index = i;
            foreach (var node in _nodes)
                foreach (var element in FiniteElements)
                    foreach (var elNode in element.Nodes)
                        if (elNode.CoordinatesEqual(node))
                            elNode.Index = node.Index;
        }

        public void AddFixationByXAxis(double x)
        {
            foreach (var node in _nodes)
                if (Math.Abs(node.X - x) < Math.Pow(10, -15))
                    node.IsFixed = true;
            var lastFixedNode = _nodes.OrderBy(n => n.X).First(n => n.X != 0 && n.Y == 0);
            lastFixedNode.IsFixed = true;

        }

        public void ApplyForceByXAxis(double x)
        {
            foreach (var node in _nodes)
                if (node.X == x)
                    _forcedNodesIndexes.Add(node.Index);
        }

        public void MakeCalculations(double depth, double force, double poissonsRatio, double elasticityModulus)
        {
            CalculateGlobalStiffnessMatrix(depth, poissonsRatio, elasticityModulus, force);
            foreach (var element in FiniteElements)
                element.CalculateStress();
            CalculateDeformations();

        }

        public List<double> GetSquares()
        {
            var squares = new List<double>();
            foreach (var element in FiniteElements)
                squares.Add(element.CalculateSquare());
            return squares;
        }

        private void CalculateGlobalStiffnessMatrix(double depth, double poissonsRatio, double elasticityModulus, double force)
        {
            foreach (var element in FiniteElements)
                element.CreateLocalStiffnessMatrix(depth, elasticityModulus, poissonsRatio);
            var indexes = new int[3];
            for (var i = 0; i < FiniteElements.Count; i++)
            {
                indexes[0] = FiniteElements[i].Nodes[0].Index;
                indexes[1] = FiniteElements[i].Nodes[1].Index;
                indexes[2] = FiniteElements[i].Nodes[2].Index;
                for (var j = 0; j < 3; j++)
                    for (var k = 0; k < 3; k++)
                    {
                        GlobalStiffnessMatrix[2 * indexes[j], 2 * indexes[k]] = GlobalStiffnessMatrix[2 * indexes[j], 2 * indexes[k]] + FiniteElements[i].LocalStiffnessMatrix[j * 2, k * 2];
                        GlobalStiffnessMatrix[2 * indexes[j] + 1, 2 * indexes[k]] = GlobalStiffnessMatrix[2 * indexes[j] + 1, 2 * indexes[k]] + FiniteElements[i].LocalStiffnessMatrix[j * 2 + 1, k * 2];
                        GlobalStiffnessMatrix[2 * indexes[j], 2 * indexes[k] + 1] = GlobalStiffnessMatrix[2 * indexes[j], 2 * indexes[k] + 1] + FiniteElements[i].LocalStiffnessMatrix[j * 2, k * 2 + 1];
                        GlobalStiffnessMatrix[2 * indexes[j] + 1, 2 * indexes[k] + 1] = GlobalStiffnessMatrix[2 * indexes[j] + 1, 2 * indexes[k] + 1] + FiniteElements[i].LocalStiffnessMatrix[j * 2 + 1, k * 2 + 1];
                    }
            }

            for (var i = 0; i < _forcedNodesIndexes.Count; i++)
                GlobalStiffnessMatrix[_forcedNodesIndexes[i] * 2, _nodes.Count * 2] = force * 0.076;

            for (var i = 0; i < _nodes.Count; i++)
                if (_nodes[i].IsFixed)
                {
                    for (var j = 0; j < _nodes.Count * 2; j++)
                    {
                        GlobalStiffnessMatrix[2 * i, j] = 0;
                        GlobalStiffnessMatrix[2 * i + 1, j] = 0;
                        GlobalStiffnessMatrix[j, 2 * i] = 0;
                        GlobalStiffnessMatrix[j, 2 * i + 1] = 0;
                    }
                    GlobalStiffnessMatrix[2 * i, 2 * i] = 1;
                    GlobalStiffnessMatrix[2 * i + 1, 2 * i + 1] = 1;
                }
            GlobalStiffnessMatrix = GlobalStiffnessMatrix.CalcGauss(_nodes);
            foreach (var element in FiniteElements)
            {
                element.Sig[0, 0] = GlobalStiffnessMatrix[2 * element.Nodes[0].Index, 2 * _nodes.Count];
                element.Sig[1, 0] = GlobalStiffnessMatrix[2 * element.Nodes[0].Index + 1, 2 * _nodes.Count];
                element.Sig[2, 0] = GlobalStiffnessMatrix[2 * element.Nodes[1].Index, 2 * _nodes.Count];
                element.Sig[3, 0] = GlobalStiffnessMatrix[2 * element.Nodes[1].Index + 1, 2 * _nodes.Count];
                element.Sig[4, 0] = GlobalStiffnessMatrix[2 * element.Nodes[2].Index, 2 * _nodes.Count];
                element.Sig[5, 0] = GlobalStiffnessMatrix[2 * element.Nodes[2].Index + 1, 2 * _nodes.Count];
            }
        }

        private void CalculateDeformations()
        {
            MaxXDeformation = GlobalStiffnessMatrix[0, 2 * _nodes.Count];
            MaxYDeformation = GlobalStiffnessMatrix[0, 2 * _nodes.Count];
            MinXDeformation = GlobalStiffnessMatrix[0, 2 * _nodes.Count];
            MinYDeformation = GlobalStiffnessMatrix[0, 2 * _nodes.Count];

            for (var i = 0; i < FiniteElements.Count; i++)
            {
                FiniteElements[i].DisplacementX = Math.Abs(GlobalStiffnessMatrix[2 * FiniteElements[i].Nodes[0].Index, 2 * _nodes.Count]) 
                    + Math.Abs(GlobalStiffnessMatrix[2 * FiniteElements[i].Nodes[1].Index, 2 * _nodes.Count]) 
                    + Math.Abs(GlobalStiffnessMatrix[2 * FiniteElements[i].Nodes[2].Index, 2 * _nodes.Count]);
                FiniteElements[i].DisplacementY = Math.Abs(GlobalStiffnessMatrix[2 * FiniteElements[i].Nodes[0].Index + 1, 2 * _nodes.Count])
                    + Math.Abs(GlobalStiffnessMatrix[2 * FiniteElements[i].Nodes[1].Index + 1, 2 * _nodes.Count])
                    + Math.Abs(GlobalStiffnessMatrix[2 * FiniteElements[i].Nodes[2].Index + 1, 2 * _nodes.Count]);
            }

            for (int i = 0; i < _nodes.Count; i++)
            {
                if (GlobalStiffnessMatrix[i * 2, 2 * _nodes.Count] > MaxXDeformation)
                    MaxXDeformation = GlobalStiffnessMatrix[i * 2, 2 * _nodes.Count];
                if (GlobalStiffnessMatrix[i * 2 + 1, 2 * _nodes.Count] > MaxYDeformation)
                    MaxYDeformation = GlobalStiffnessMatrix[i * 2 + 1, 2 * _nodes.Count];
                if (GlobalStiffnessMatrix[i * 2, 2 * _nodes.Count] < MinXDeformation)
                    MinXDeformation = GlobalStiffnessMatrix[i * 2, 2 * _nodes.Count];
                if (GlobalStiffnessMatrix[i * 2 + 1, 2 * _nodes.Count] < MinYDeformation)
                    MinYDeformation = GlobalStiffnessMatrix[i * 2 + 1, 2 * _nodes.Count];
            }

            MaxStress = FiniteElements.Max(e => e.Stress);
        }
    }
}

