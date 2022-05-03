using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressStrainStateAnalyzer.DataManagers
{
    public interface IDataManager
    {
        List<INode> GetNodes(string path);
        List<IFiniteElement> GetElements(string path, List<INode> nodes);
        List<INode> GetNodesWithStress(string path);
        void SaveReport(List<INode> nodes, List<INode> verifyNodes, string path);
    }
}
