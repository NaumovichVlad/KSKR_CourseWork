using StressStrainStateAnalyzer.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressStrainStateAnalyzer.FiniteElements
{
    public interface IFiniteElement
    {
        public int Index { get; set; }
        public List<INode> Nodes { get; }

        double CalculateSquare();

        double CalculateMinAngleSize();
    }
}
