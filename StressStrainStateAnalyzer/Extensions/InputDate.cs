using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressStrainStateAnalyzer.Extensions
{
    public class InputDate
    {
        /// <summary>
        /// Получение из файла узлов.
        /// </summary>
        /// <returns>узлы</returns>
        static public List<INode> Nodes()
        {
            Node node = new Node();
            List<INode> nodes = new List<INode>();
            StreamReader file = new StreamReader("nodes.txt");
            while (!file.EndOfStream)
            {
                List<string> str = file.ReadLine().TrimStart().TrimEnd().Split().ToList();
                for (int i = 0; i < str.Count; i++)
                    if (str[i] == "")
                    {
                        str.RemoveAt(i);
                        i--;
                    }
                node = new Node(Convert.ToDouble(str[1]), Convert.ToDouble(str[2]), Convert.ToInt32(str[0]) - 1);
                nodes.Add(node);
            }
            return (nodes);
        }

        /// <summary>
        /// Получение из файла элементов.
        /// </summary>
        /// <param name="nodes">узлы</param>
        /// <returns>элементы</returns>
        static public List<IFiniteElement> Elements(List<INode> nodes)
        {
            TriangleFiniteElement element = new TriangleFiniteElement();
            List<IFiniteElement> elements = new List<IFiniteElement>();
            StreamReader file = new StreamReader("elements.txt");
            while (!file.EndOfStream)
            {
                List<string> str = file.ReadLine().TrimStart().TrimEnd().Split().ToList();
                for (int i = 0; i < str.Count; i++)
                    if (str[i] == "")
                    {
                        str.RemoveAt(i);
                        i--;
                    }
                if (str.Count == 0)
                    continue;
                element = new TriangleFiniteElement(nodes[Convert.ToInt32(str[1]) - 1], nodes[Convert.ToInt32(str[2]) - 1], nodes[Convert.ToInt32(str[3]) - 1]);
                elements.Add(element);
            }
            return (elements);
        }

    }
}
