using StressStrainStateAnalyzer.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressStrainStateAnalyzer.FiniteElements
{
    public class TriangleFiniteElement : IFiniteElement
    {
        public int Index { get; set; }
        public List<INode> Nodes { get; }

        public TriangleFiniteElement(INode firstNode, INode secondNode, INode thirdNode)
        {
            Nodes = new List<INode>() { firstNode, secondNode, thirdNode };
        }

        public double CalculateSquare()
        {
            var square = 1.0;
            var sides = new double[Nodes.Count];
            for (var i = 0; i < Nodes.Count; i++)
            {
                sides[i] = Math.Sqrt(Math.Pow(Nodes[i].X - Nodes[(i + 1) % Nodes.Count].X, 2)
                    + Math.Pow(Nodes[i].Y - Nodes[(i + 1) % Nodes.Count].Y, 2));
            }
            var p = sides.Sum() / 2;
            if (sides.Max() > p)
                return 0;
            for (var i = 0; i < sides.Length; i++)
                square *= p - sides[i];
            return Math.Sqrt(square);
        }

        public double CalculateMinAngleSize()
        {
            var min = double.MaxValue;
            for (var i = 0; i < Nodes.Count; i++)
            {
                var angle = CalculateAngle(Nodes[i % Nodes.Count], Nodes[(i + 1) % Nodes.Count], Nodes[(i + 2) % Nodes.Count]);
                if (angle < min)
                    min = angle;
            }
            return min;
        }

        private double CalculateAngle(INode firstNode, INode secondNode, INode angleNode)
        {
            var numerator = (angleNode.X - firstNode.X) * (angleNode.X - secondNode.X)
                + (angleNode.Y - firstNode.Y) * (angleNode.Y - secondNode.Y);
            var denomenator = Math.Sqrt(Math.Pow(angleNode.X - firstNode.X, 2) + Math.Pow(angleNode.Y - firstNode.Y, 2))
                * Math.Sqrt(Math.Pow(angleNode.X - secondNode.X, 2) + Math.Pow(angleNode.Y - secondNode.Y, 2));
            return Math.Acos(numerator / denomenator);
        }

        public override bool Equals(object? obj)
        {
            return obj is TriangleFiniteElement element
                && Nodes.Any(n => n.CoordinatesEqual(element.Nodes[0]))
                && Nodes.Any(n => n.CoordinatesEqual(element.Nodes[1]))
                && Nodes.Any(n => n.CoordinatesEqual(element.Nodes[2]));
                
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Nodes);
        }
    }
}
