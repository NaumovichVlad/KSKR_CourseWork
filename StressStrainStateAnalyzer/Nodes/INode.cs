namespace StressStrainStateAnalyzer.Nodes
{
    //Узел элемента
    public interface INode
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Index { get; set; }
        public double Stress { get; set; }
        public bool IsFixed { get; set; }

        public bool CoordinatesEqual(INode node);
    }
}
