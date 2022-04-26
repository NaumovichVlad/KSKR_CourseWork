using StressStrainStateAnalyzer.Nodes;

namespace StressStrainStateAnalyzer.FiniteElements
{
    public interface IFiniteElement
    {
        int Index { get; set; }
        List<INode> Nodes { get; }
        double[,] LocalStiffnessMatrix { get; }
        double[,] Sig { get; set; }
        double Stress { get; }

        void CreateLocalStiffnessMatrix(double depth,
            double elasticModulus, double poissonsRatio);
        void CalculateStress();
        double CalculateSquare();
        double CalculateMinAngleSize();
    }
}
