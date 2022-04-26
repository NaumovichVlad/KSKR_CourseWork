namespace StressStrainStateAnalyzer.Nodes
{
    public class Node : INode
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Index { get; set; }
        public bool IsFixed { get; set; }

        public Node()
        { }

        public Node(double x, double y, int index, bool isFixed)
        {
            X = x;
            Y = y;
            Index = index;
            IsFixed = isFixed;
        }

        public bool CoordinatesEqual(INode node) => X == node.X && Y == node.Y;

        public override bool Equals(object? obj)
        {
            return obj is Node node &&
                   X == node.X &&
                   Y == node.Y &&
                   Index == node.Index &&
                   IsFixed == node.IsFixed;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Index, IsFixed);
        }
    }
}
