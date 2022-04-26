using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.Nodes;

namespace StressStrainStateAnalyzer.MeshBuilders
{
    public interface IMeshBuilder
    {
        public List<IFiniteElement> BuildMesh(List<INode> nodes, double maxElementSquare, double minAngle);
    }
}
