using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.MeshBuilders;
using StressStrainStateAnalyzer.Nodes;

namespace StressStrainStateAnalyzer.MeshBulders
{
    public class TriangleMeshBuilder : IMeshBuilder
    {
        private List<IFiniteElement> _finiteElements = new List<IFiniteElement>();
        private List<INode> _nodes = new List<INode>();

        public List<IFiniteElement> BuildMesh(List<INode> nodes, double maxElementSquare, double minAngle)
        {
            _nodes = nodes;
            BuildInitialPartition(_nodes);
            OptimizeWithRappertAlgorithm(maxElementSquare, minAngle);
            return _finiteElements;
        }

        private void BuildInitialPartition(List<INode> nodes)
        {
            var firstPointIndex = 0;
            var secondPointIndex = 1;
            var vertexPointIndex = FindThirdVertexIndex(nodes[firstPointIndex], nodes[secondPointIndex], nodes);
            CreateElements(firstPointIndex, secondPointIndex, vertexPointIndex, nodes);

        }

        private void CreateElements(int firstPointIndex, int secondPointIndex, int vertexPointIndex, List<INode> points)
        {
            var finiteElement = new TriangleFiniteElement(points[firstPointIndex], points[secondPointIndex], points[vertexPointIndex]);
            if (_finiteElements.Contains(finiteElement))
                return;
            else
                _finiteElements.Add(finiteElement);
            var firstVertexPointIndex =
                FindThirdVertexIndex(_nodes[firstPointIndex], _nodes[vertexPointIndex], _nodes[secondPointIndex], _nodes);
            var secondVertexPointIndex =
                FindThirdVertexIndex(_nodes[secondPointIndex], _nodes[vertexPointIndex], _nodes[firstPointIndex], _nodes);
            if (firstVertexPointIndex != -1 && !SegmentCheck(firstPointIndex, vertexPointIndex, points.Count))
                CreateElements(firstPointIndex, vertexPointIndex, firstVertexPointIndex, _nodes);
            if (secondVertexPointIndex != -1 && !SegmentCheck(secondPointIndex, vertexPointIndex, points.Count))
                CreateElements(secondPointIndex, vertexPointIndex, secondVertexPointIndex, _nodes);
        }



        private int FindThirdVertexIndex(INode firstNode, INode secondNode, List<INode> nodes)
        {
            var max = 0.0;
            var maxIndex = 0;
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].CoordinatesEqual(firstNode) || nodes[i].CoordinatesEqual(secondNode))
                    continue;
                var angle = CalculateAngle(firstNode, secondNode, nodes[i]);
                if (angle > max)
                {
                    max = angle;
                    maxIndex = i;
                }
            }
            return maxIndex;
        }

        private int FindThirdVertexIndex(INode firstNode, INode secondNode, INode checkPoint, List<INode> nodes)
        {
            var max = 0.0;
            var maxIndex = 0;
            var flag = false;
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].CoordinatesEqual(firstNode) || nodes[i].CoordinatesEqual(secondNode))
                    continue;
                if (FindPointSide(firstNode, secondNode, nodes[i]) == FindPointSide(firstNode, secondNode, checkPoint)
                    || FindPointSide(firstNode, secondNode, nodes[i]) == 0)
                {
                    continue;
                }
                var angle = CalculateAngle(firstNode, secondNode, nodes[i]);
                if (angle > max)
                {
                    flag = true;
                    max = angle;
                    maxIndex = i;
                }
            }
            if (flag)
                return maxIndex;
            else return -1;
        }

        private double CalculateAngle(INode firstNode, INode secondNode, INode vertexNode)
        {
            var numerator = (vertexNode.X - firstNode.X) * (vertexNode.X - secondNode.X)
                + (vertexNode.Y - firstNode.Y) * (vertexNode.Y - secondNode.Y);
            var denomenator = Math.Sqrt(Math.Pow(vertexNode.X - firstNode.X, 2) + Math.Pow(vertexNode.Y - firstNode.Y, 2))
                * Math.Sqrt(Math.Pow(vertexNode.X - secondNode.X, 2) + Math.Pow(vertexNode.Y - secondNode.Y, 2));
            return Math.Acos(numerator / denomenator);
        }

        private int FindPointSide(INode firstLinePoint, INode secondLinePoint, INode point)
        {
            var d = (point.X - firstLinePoint.X) * (secondLinePoint.Y - firstLinePoint.Y)
                - (point.Y - firstLinePoint.Y) * (secondLinePoint.X - firstLinePoint.X);
            if (d < 0)
                return -1;
            if (d > 0)
                return 1;
            return 0;
        }

        private bool SegmentCheck(int firstIndex, int secondIndex, int pointsCount)
        {
            return Math.Abs(firstIndex - secondIndex) % (pointsCount - 2) == 1;
        }

        private void OptimizeWithRappertAlgorithm(double maxSquare, double angle)
        {
            /*for (var i = 0; i < _finiteElements.Count; i++)
            {
                if (_finiteElements[i].CalculateSquare() > maxSquare)
                    *//*|| _finiteElements[i].CalculateMinAngleSize() < angle)*//*
                {
                    var newNode = FindCircleCenter(_finiteElements[i]);
                    var deletedFiniteElements = _finiteElements.Where(e =>
                        (e.Nodes.Contains(_finiteElements[i].Nodes[0]) && e.Nodes.Contains(_finiteElements[i].Nodes[1])) ||
                        (e.Nodes.Contains(_finiteElements[i].Nodes[1]) && e.Nodes.Contains(_finiteElements[i].Nodes[2])) ||
                        (e.Nodes.Contains(_finiteElements[i].Nodes[2]) && e.Nodes.Contains(_finiteElements[i].Nodes[0]))
                    ).ToList();
                    var freeNodes = new List<INode>();
                    for (var j = 0; j < deletedFiniteElements.Count(); j++)
                    {
                        freeNodes = freeNodes.Concat(deletedFiniteElements[j].Nodes).ToList();
                        _finiteElements.Remove(deletedFiniteElements[j]);
                    }
                    for (var j = 0; j < freeNodes.Count; j++)
                    {
                        var counter = 0;
                        for (var k = 0; k < freeNodes.Count; k++)
                        {
                            var flag = true;
                            if (k == j)
                                continue;
                            for (var m = 0; m < freeNodes.Count; m++)
                            {
                                if (m == j || m == k)
                                    continue;
                                if (FindPointSide(freeNodes[j], freeNodes[k], freeNodes[m]) != FindPointSide(freeNodes[j], freeNodes[k], newNode))
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                _finiteElements.Add(new TriangleFiniteElement(freeNodes[j], freeNodes[k], newNode));
                                counter++;
                            }
                            if(counter == 2)
                            {
                                freeNodes.Remove(freeNodes[j]);
                                j--;
                                break;
                            }
                        }
                    }
                    i = 0;
                }
            }*/
        }

        private INode FindCircleCenter(IFiniteElement element)
        {
            var x12 = element.Nodes[0].X - element.Nodes[1].X;
            var x23 = element.Nodes[1].X - element.Nodes[2].X;
            var x31 = element.Nodes[2].X - element.Nodes[1].X;
            var y12 = element.Nodes[0].Y - element.Nodes[1].Y;
            var y23 = element.Nodes[1].Y - element.Nodes[2].Y;
            var y31 = element.Nodes[2].Y - element.Nodes[1].Y;
            var z = x12 * y31 - y12 * x31;
            var z1 = Math.Pow(element.Nodes[0].X, 2) + Math.Pow(element.Nodes[0].Y, 2);
            var z2 = Math.Pow(element.Nodes[1].X, 2) + Math.Pow(element.Nodes[1].Y, 2);
            var z3 = Math.Pow(element.Nodes[2].X, 2) + Math.Pow(element.Nodes[2].Y, 2);
            var zx = y12 * z3 + y23 * z1 + y31 * z2;
            var zy = x12 * z3 + x23 * z1 + x31 * z2;
            var rx = -.5 * zx / z;
            var ry = .5 * zy / z;
            return new Node { X = rx, Y = ry };
        }
    }
}