using StressStrainStateAnalyzer.Nodes;

namespace StressStrainStateAnalyzer.FiniteElements
{
    //Конечный элемент
    public interface IFiniteElement
    {
        int Index { get; set; }
        List<INode> Nodes { get; }
        double[,] LocalStiffnessMatrix { get; }
        double DisplacementX { get; set; }
        double DisplacementY { get; set; }
        double[,] Sig { get; set; }
        double Stress { get; }

        void CreateLocalStiffnessMatrix(double depth,
            double elasticModulus, double poissonsRatio);
        void CalculateStress();
        double CalculateSquare();
        double CalculateMinAngleSize();
    }
}
