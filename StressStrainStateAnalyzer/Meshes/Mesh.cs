using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.MeshBuilders;
using StressStrainStateAnalyzer.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressStrainStateAnalyzer.Meshes
{
    public class Mesh
    {
        public List<IFiniteElement> FiniteElements { get ; set; }

        public Mesh(IMeshBuilder builder, List<INode> nodes, double maxSquare, double angle)
        {
            FiniteElements = builder.BuildMesh(nodes, maxSquare, angle);
        }   
    }
}

