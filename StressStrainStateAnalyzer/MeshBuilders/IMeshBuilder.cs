using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressStrainStateAnalyzer.MeshBuilders
{
    public interface IMeshBuilder
    {
        public List<IFiniteElement> BuildMesh(List<INode> nodes, double maxElementSquare, double minAngle);
    }
}
