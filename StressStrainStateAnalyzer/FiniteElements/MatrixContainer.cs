using StressStrainStateAnalyzer.Extensions;
using StressStrainStateAnalyzer.Nodes;

namespace StressStrainStateAnalyzer.FiniteElements
{
    public class MatrixContainer
    {
        private double[,] _coordinateMatrix;
        private double[,] _inverseCoordinateMatrix;
        private double[,] _elasticityMatrix;
        private double[,] _stress;
        private double[,] _q;
        private double[,] _b;

        public double[,] LocalStiffnessMatrix { get; private set; }
        public double[,] Sig { get; set; }
        public bool IsInitialized { get; private set; }

        public MatrixContainer()
        {
            LocalStiffnessMatrix = new double[6, 6];
            Sig = new double[6, 6];
            _coordinateMatrix = new double[6, 6];
            _inverseCoordinateMatrix = new double[6, 6];
            _elasticityMatrix = new double[3, 3];
            _stress = new double[6, 6];
            _q = new double[3, 6];
            _b = new double[3, 6];
        }

        public void InitializeMatrixes(double depth, double elasticModulus,
            double poissonsRatio, double elementSquare, List<INode> elementNodes)
        {
            InitializeCoordinateMatrixes(elementNodes);
            InitializeElasticityMatrixes(elasticModulus, poissonsRatio);
            InitializeLocalStiffnessMatrix(depth, elementSquare);
            IsInitialized = true;
        }

        private void InitializeCoordinateMatrixes(List<INode> elementNodes)
        {
            var j = 0;
            var counter = 0;
            for (var i = 0; i < _coordinateMatrix.GetLength(0); i++)
            {
                j %= _coordinateMatrix.GetLength(0);
                _coordinateMatrix[i, j] = 1;
                _coordinateMatrix[i, j + 1] = elementNodes[counter].X;
                _coordinateMatrix[i, j + 2] = elementNodes[counter].Y;
                j += _coordinateMatrix.GetLength(0) / 2;
                if (j == _coordinateMatrix.GetLength(0))
                    counter++;
            }
            _inverseCoordinateMatrix = _coordinateMatrix.Inverse();
        }

        private void InitializeElasticityMatrixes(double elasticModulus, double poissonsRatio)
        {
            var ratio = elasticModulus / (1 - Math.Pow(poissonsRatio, 2));
            _elasticityMatrix[0, 0] = 1;
            _elasticityMatrix[0, 1] = poissonsRatio;
            _elasticityMatrix[0, 2] = 0;
            _elasticityMatrix[1, 0] = poissonsRatio;
            _elasticityMatrix[1, 1] = 1;
            _elasticityMatrix[1, 2] = 0;
            _elasticityMatrix[2, 0] = 0;
            _elasticityMatrix[2, 1] = 0;
            _elasticityMatrix[2, 2] = (1 - poissonsRatio) / 2;
            _elasticityMatrix = _elasticityMatrix.Multiply(ratio);
        }

        private void InitializeLocalStiffnessMatrix(double depth, double elementSquare)
        {
            _q[0, 1] = 1;
            _q[1, 5] = 1;
            _q[2, 2] = 1;
            _q[2, 4] = 1;
            _b = _q.Multiply(_inverseCoordinateMatrix);
            var transposeB = _b.Transpose();
            LocalStiffnessMatrix = transposeB.Multiply(_elasticityMatrix);
            LocalStiffnessMatrix = LocalStiffnessMatrix.Multiply(_b);
            LocalStiffnessMatrix = LocalStiffnessMatrix.Multiply(depth);
            LocalStiffnessMatrix = LocalStiffnessMatrix.Multiply(elementSquare);
        }

        public double CalculateStress()
        {
            var bb = _q.Multiply(_inverseCoordinateMatrix);
            var e = bb.Multiply(Sig);
            _stress = _elasticityMatrix.Multiply(e);
            return Math.Sqrt(Math.Pow(_stress[0, 0], 2) + Math.Pow(_stress[1, 0], 2) - _stress[0, 0] * _stress[1, 0] + 3 * Math.Pow(_stress[2, 0], 2));
        }
    }
}
