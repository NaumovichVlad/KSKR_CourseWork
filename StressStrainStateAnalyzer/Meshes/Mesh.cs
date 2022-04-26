using StressStrainStateAnalyzer.Extensions;
using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.MeshBuilders;
using StressStrainStateAnalyzer.Nodes;

namespace StressStrainStateAnalyzer.Meshes
{
    public class Mesh
    {
        private List<INode> _nodes;
        public List<IFiniteElement> FiniteElements { get; set; }
        public double[,] GlobalStiffnessMatrix { get; set; }
        public double MaxStress { get; set; }
        public double MaxXDeformation { get; set; }
        public double MaxYDeformation { get; set; }


        public Mesh(IMeshBuilder builder, List<INode> nodes, double maxSquare, double angle)
        {
            FiniteElements = builder.BuildMesh(nodes, maxSquare, angle);
            _nodes = new List<INode>();
            FiniteElements.ForEach(f => _nodes.AddRange(f.Nodes));
            GlobalStiffnessMatrix = new double[_nodes.Count * 2, _nodes.Count * 2 + 1];
        }

        public void MakeCalculations(double depth, double force, double poissonsRatio, double elasticityModulus)
        {
            CalculateGlobalStiffnessMatrix(depth, poissonsRatio, elasticityModulus, force);
            foreach (var element in FiniteElements)
                element.CalculateStress();
            CalculateMaxDeformations();

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
            var fixedNodes = _nodes.Where(n => n.IsFixed).Select(n => n.Index).ToList();

            for (var i = 0; i < fixedNodes.Count; i++)
                GlobalStiffnessMatrix[fixedNodes[i] * 2, _nodes.Count * 2] = force;

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

        private void CalculateMaxDeformations()
        {
            MaxXDeformation = GlobalStiffnessMatrix[0, 2 * _nodes.Count];
            MaxYDeformation = GlobalStiffnessMatrix[0, 2 * _nodes.Count];

            for (int i = 0; i < _nodes.Count; i++)
            {
                if (GlobalStiffnessMatrix[i * 2, 2 * _nodes.Count] > MaxXDeformation)
                    MaxXDeformation = GlobalStiffnessMatrix[i * 2, 2 * _nodes.Count];
                if (GlobalStiffnessMatrix[i * 2 + 1, 2 * _nodes.Count] > MaxYDeformation)
                    MaxYDeformation = GlobalStiffnessMatrix[i * 2 + 1, 2 * _nodes.Count];
            }

            MaxStress = FiniteElements[0].Stress;
            for (int i = 0; i < FiniteElements.Count; i++)
                if (FiniteElements[i].Stress > MaxStress)
                    MaxStress = FiniteElements[i].Stress;
        }
    }
}

