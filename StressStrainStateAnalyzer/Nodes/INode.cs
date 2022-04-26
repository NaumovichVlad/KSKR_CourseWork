namespace StressStrainStateAnalyzer.Nodes
{
    public interface INode
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Index { get; set; }
        public bool IsFixed { get; set; }

        public bool CoordinatesEqual(INode node);
    }
}
