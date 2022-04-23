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
            _finiteElements = BuildInitialPartition(_nodes, InitializeSegments(_nodes));
            OptimizeWithRappertAlgorithm(maxElementSquare, minAngle);
            return _finiteElements;
        }

        private List<Segment> InitializeSegments(List<INode> nodes)
        {
            List<Segment> segments = new List<Segment>();
            for (int i = 0; i < nodes.Count; i++)
                segments.Add(new Segment(nodes[i], nodes[(i + 1) % nodes.Count]));
            return segments;
        }

        private List<IFiniteElement> BuildInitialPartition(List<INode> nodes, List<Segment> segments)
        {
            var firstPointIndex = nodes.IndexOf(segments[0].First);
            var secondPointIndex = nodes.IndexOf(segments[0].Last);
            for (var i = 0; i < segments.Count; i++)
            {
                firstPointIndex = nodes.IndexOf(segments[i].First);
                secondPointIndex = nodes.IndexOf(segments[i].Last);
                List<int> sides = new List<int>();
                var flag = false;
                foreach (var node in nodes)
                {
                    var side = FindPointSide(segments[i].First, segments[i].Last, node);
                    if (side == 0)
                        continue;
                    sides.Add(side);
                    if (sides.Distinct().Count() != 1)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    firstPointIndex = nodes.IndexOf(segments[i].First);
                    secondPointIndex= nodes.IndexOf(segments[i].Last);
                    break;
                }
            }
            var vertexPointIndex = FindThirdVertexIndex(nodes[firstPointIndex], nodes[secondPointIndex], nodes);
            return CreateElements(firstPointIndex, secondPointIndex, vertexPointIndex, nodes, new List<IFiniteElement>(), segments);

        }

        private List<IFiniteElement> CreateElements(int firstPointIndex, int secondPointIndex, 
            int vertexPointIndex, List<INode> points, List<IFiniteElement> finiteElements, List<Segment> segments)
        {
            var finiteElement = new TriangleFiniteElement(points[firstPointIndex], points[secondPointIndex], points[vertexPointIndex]);
            if (finiteElements.Contains(finiteElement))
                return finiteElements;
            else 
                finiteElements.Add(finiteElement);
            var firstVertexPointIndex =
                FindThirdVertexIndex(points[firstPointIndex], points[vertexPointIndex], points[secondPointIndex], points);
            var secondVertexPointIndex =
                FindThirdVertexIndex(points[secondPointIndex], points[vertexPointIndex], points[firstPointIndex], points);
            if (firstVertexPointIndex != -1 && !SegmentCheck(points[firstPointIndex], points[vertexPointIndex], segments))
                finiteElements = CreateElements(firstPointIndex, vertexPointIndex, firstVertexPointIndex, points, finiteElements, segments);
            if (secondVertexPointIndex != -1 && !SegmentCheck(points[secondPointIndex], points[vertexPointIndex], segments))
                finiteElements = CreateElements(secondPointIndex, vertexPointIndex, secondVertexPointIndex, points, finiteElements, segments);
            return finiteElements;
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
           /* var numerator = (vertexNode.X - firstNode.X) * (vertexNode.X - secondNode.X)
                + (vertexNode.Y - firstNode.Y) * (vertexNode.Y - secondNode.Y);
            var denomenator = Math.Sqrt(Math.Pow(vertexNode.X - firstNode.X, 2) + Math.Pow(vertexNode.Y - firstNode.Y, 2))
                * Math.Sqrt(Math.Pow(vertexNode.X - secondNode.X, 2) + Math.Pow(vertexNode.Y - secondNode.Y, 2));*/
            var a = Math.Sqrt(Math.Pow(firstNode.X - vertexNode.X, 2) + Math.Pow(firstNode.Y - vertexNode.Y, 2));
            var b = Math.Sqrt(Math.Pow(secondNode.X - vertexNode.X, 2) + Math.Pow(secondNode.Y - vertexNode.Y, 2));
            var c = Math.Sqrt(Math.Pow(firstNode.X - secondNode.X, 2) + Math.Pow(firstNode.Y - secondNode.Y, 2));
            var angle = Math.Acos((Math.Pow(a, 2) + Math.Pow(b, 2) - Math.Pow(c, 2)) / (2 * a * b)) * 180 / Math.PI;
            return angle;
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

        private bool SegmentCheck(INode first, INode last, List<Segment> segments)
        {
            var flag = false;
            foreach (var segment in segments)
            {
                if ((segment.Last.CoordinatesEqual(last) && segment.First.CoordinatesEqual(first)) ||
                    (segment.Last.CoordinatesEqual(first) && segment.First.CoordinatesEqual(last)))
                {
                    flag = true;
                    break;
                }    
            }
            return flag;
        }

        private void OptimizeWithRappertAlgorithm(double maxSquare, double angle)
        {
            SplitSegments(maxSquare, angle);
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

        private void SplitSegments(double maxSquare, double minAngle)
        {
            for (var i = 0; i < _finiteElements.Count; i++)
            {
                var flag = false;
                /*if (_finiteElements[i].CalculateSquare() < maxSquare)
                    if (CheckSmallCorner(_finiteElements[i], minAngle))
                        continue;*/
                for (var j = 0; j < _finiteElements[i].Nodes.Count; j++)
                {
                    var rad = GetLineLength(_finiteElements[i].Nodes[j], _finiteElements[i].Nodes[(j + 1) % _finiteElements[i].Nodes.Count]);
                    var circleCenter = GetLineCenter(_finiteElements[i].Nodes[j], _finiteElements[i].Nodes[(j + 1) % _finiteElements[i].Nodes.Count]);
                    circleCenter.X += Math.Pow(10, -50);
                    circleCenter.Y += Math.Pow(10, -50);
                    if (!CheckBelonging(_finiteElements[i], circleCenter))
                        continue;
                    for (var k = 0; k < _finiteElements.Count; k++)
                    {
                        foreach (var node in _finiteElements[k].Nodes)
                        {
                            if (GetLineLength(circleCenter, node) <= rad)
                            {
                                if (node.CoordinatesEqual(_finiteElements[i].Nodes[j]) ||
                                    node.CoordinatesEqual(_finiteElements[i].Nodes[(j + 1) % _finiteElements[i].Nodes.Count]))
                                    continue;
                                BuildNewTriangles(_finiteElements[i], circleCenter);
                                flag = true;
                                break;
                            }
                        }
                        if (flag)
                            break;
                    }
                    if (flag)
                    {
                        i = 0;
                        break;
                    }
                }
            }
        }

        private double GetLineLength(INode firstNode, INode secondNode)
        {
            return Math.Sqrt(Math.Pow(firstNode.X - secondNode.X, 2) + Math.Pow(firstNode.Y - secondNode.Y, 2));
        }

        private INode GetLineCenter(INode firstNode, INode secondNode)
        {
            var centerX = firstNode.X - (firstNode.X - secondNode.X) / 2;
            var centerY = firstNode.Y - (firstNode.Y - secondNode.Y) / 2;
            return new Node { X = centerX, Y = centerY };
        }

        private void BuildNewTriangles(IFiniteElement triangle, INode newNode)
        {
            var deletedTriangles = new List<IFiniteElement>();
            for (var i = 0; i < triangle.Nodes.Count; i++)
                deletedTriangles.AddRange(FindTrianglesBySegment(triangle.Nodes[i], triangle.Nodes[(i + 1) % triangle.Nodes.Count]));
            deletedTriangles = deletedTriangles.Distinct().ToList();
            while (deletedTriangles.Contains(triangle))
                deletedTriangles.Remove(triangle);
            var newNodes = new List<INode>();
            var segments = new List<Segment>();
            for (int i = 0; i < deletedTriangles.Count; i++)
            {
                for (var j = 0; j < deletedTriangles[i].Nodes.Count; j++)
                {
                    if (triangle.Nodes.Contains(deletedTriangles[i].Nodes[j]) 
                        && triangle.Nodes.Contains(deletedTriangles[i].Nodes[(j + 1) % deletedTriangles[i].Nodes.Count]))
                    {
                        segments.Add(new Segment(deletedTriangles[i].Nodes[j],
                            deletedTriangles[i].Nodes[(j + 2) % deletedTriangles[i].Nodes.Count]));
                        segments.Add(new Segment(deletedTriangles[i].Nodes[(j + 1) % deletedTriangles[i].Nodes.Count],
                            deletedTriangles[i].Nodes[(j + 2) % deletedTriangles[i].Nodes.Count]));
                    }
                }
                
            }
            if (deletedTriangles.Count == 2)
                segments = CloseContour(segments);
            if (deletedTriangles.Count == 1)
                segments = CloseContour(segments, triangle);
            _finiteElements.Remove(triangle);
            for (int i = 0; i < deletedTriangles.Count; i++)
            {
                _finiteElements.Remove(deletedTriangles[i]);
                for (int j = 0; j < segments.Count; j++)
                {
                    if (!newNodes.Contains(segments[j].Last))
                        newNodes.Add(segments[j].Last);
                    if (!newNodes.Contains(segments[j].First))
                        newNodes.Add(segments[j].First);
                }
            }
            newNodes.Add(newNode);
            _finiteElements.AddRange(BuildInitialPartition(newNodes,segments));
        }

        private List<IFiniteElement> FindTrianglesBySegment(INode firstNode, INode secondNode)
        {
            List<IFiniteElement> triangles = new List<IFiniteElement>();
            foreach (var element in _finiteElements)
            {
                var counter = 0;
                foreach (var node in element.Nodes)
                    if (node.CoordinatesEqual(firstNode) || node.CoordinatesEqual(secondNode))
                        counter++;
                if (counter == 2)
                    triangles.Add(element);
            }
            return triangles;
        }

        private bool CheckBelonging(IFiniteElement element, INode center)
        {
            var neighbors = new List<IFiniteElement>();
            for (var i = 0; i < element.Nodes.Count; i++)
                neighbors.AddRange(FindTrianglesBySegment(element.Nodes[i], element.Nodes[(i + 1) % element.Nodes.Count]));
            neighbors.Remove(element);
            var belonging = false;
            foreach (var neighbor in neighbors)
            {
                var top = 0;
                var bottom = 0;
                for (var i = 0; i < neighbor.Nodes.Count; i++)
                {
                    var side = FindPointSide(neighbor.Nodes[i], neighbor.Nodes[(i + 1) % neighbor.Nodes.Count], center);
                    if (side > 0)
                        top++;
                    if (side < 0)
                        bottom++;
                }
                if (top == 1 && bottom == 2)
                {
                    belonging = true;
                    break;
                }
            }
            return belonging;
        }

        private bool CheckSmallCorner(IFiniteElement element, double minAngle)
        {
            var smallAngle = false;
            for (var i = 0; i < element.Nodes.Count; i++)
            {
                var test_tmp = CalculateAngle(element.Nodes[i], element.Nodes[(i + 1) % element.Nodes.Count],
                    element.Nodes[(i + 2) % element.Nodes.Count]);
                if (test_tmp < minAngle)
                {
                    smallAngle = true;
                    /*var deletedTriangles = FindTrianglesBySegment(element.Nodes[i], element.Nodes[(i + 2) % element.Nodes.Count]);
                    deletedTriangles.AddRange(FindTrianglesBySegment(element.Nodes[(i + 1) % element.Nodes.Count], element.Nodes[(i + 2) % element.Nodes.Count]));
                    deletedTriangles = deletedTriangles.Distinct().ToList();
                    deletedTriangles.Remove(element);
                    if (deletedTriangles.Count == 2)
                    {
                        var newNodes = new List<INode>();
                        var segments = new List<Segment>();
                        for (int k = 0; k < deletedTriangles.Count; k++)
                        {
                            for (var j = 0; j < deletedTriangles[k].Nodes.Count; j++)
                            {
                                if (element.Nodes.Contains(deletedTriangles[k].Nodes[j])
                                    && element.Nodes.Contains(deletedTriangles[k].Nodes[(j + 1) % deletedTriangles[k].Nodes.Count]))
                                {
                                    segments.Add(new Segment(deletedTriangles[k].Nodes[j],
                                        deletedTriangles[k].Nodes[(j + 2) % deletedTriangles[k].Nodes.Count]));
                                    segments.Add(new Segment(deletedTriangles[k].Nodes[(j + 1) % deletedTriangles[k].Nodes.Count],
                                        deletedTriangles[k].Nodes[(j + 2) % deletedTriangles[k].Nodes.Count]));
                                    segments = CloseContour(segments);
                                }
                            }

                        }
                        _finiteElements.Remove(element);
                        for (int k = 0; k < deletedTriangles.Count; k++)
                        {
                            _finiteElements.Remove(deletedTriangles[k]);
                            for (int j = 0; j < segments.Count; j++)
                            {
                                if (!newNodes.Contains(segments[j].Last))
                                    newNodes.Add(segments[j].Last);
                                if (!newNodes.Contains(segments[j].First))
                                    newNodes.Add(segments[j].First);
                            }
                        }
                        for (var r = 0.8; r < GetLineLength(element.Nodes[i], element.Nodes[(i + 2) % element.Nodes.Count]); r *= 2)
                        {
                            var projection = CalculateProjectionSize(element.Nodes[(i + 2) % element.Nodes.Count], element.Nodes[i], r);
                            var newX = element.Nodes[(i + 2) % element.Nodes.Count].X + projection;
                            var newY = (newX - element.Nodes[(i + 2) % element.Nodes.Count].X) / (element.Nodes[i].X - element.Nodes[(i + 2) % element.Nodes.Count].X)
                                * (element.Nodes[i].Y - element.Nodes[(i + 2) % element.Nodes.Count].Y) + element.Nodes[(i + 2) % element.Nodes.Count].Y;
                            newNodes.Add(new Node() { X = newX, Y = newY });
                        }
                        for (var r = 0.8; r < GetLineLength(element.Nodes[(i + 1) % element.Nodes.Count], element.Nodes[(i + 2) % element.Nodes.Count]); r *= 2)
                        {
                            var projection = CalculateProjectionSize(element.Nodes[(i + 2) % element.Nodes.Count], element.Nodes[(i + 1) % element.Nodes.Count], r);
                            var newX = element.Nodes[(i + 2) % element.Nodes.Count].X + projection;
                            var newY = (newX - element.Nodes[(i + 2) % element.Nodes.Count].X) / (element.Nodes[(i + 1) % element.Nodes.Count].X - element.Nodes[(i + 2) % element.Nodes.Count].X)
                                * (element.Nodes[(i + 1) % element.Nodes.Count].Y - element.Nodes[(i + 2) % element.Nodes.Count].Y) + element.Nodes[(i + 2) % element.Nodes.Count].Y;
                            newNodes.Add(new Node() { X = newX, Y = newY });
                        }
                        _finiteElements.AddRange(BuildInitialPartition(newNodes, segments));
                    }*/
                    break;
                }
            }
            return smallAngle;
        }

        private double CalculateProjectionSize(INode lastNode, INode vertexNode, double length)
        {
            var tmpNode = new Node() { X = lastNode.X, Y = vertexNode.Y };
            var corner = CalculateAngle(lastNode, tmpNode, vertexNode);
            var projection = Math.Cos(corner / 180 * Math.PI) * length;
            if (vertexNode.X < lastNode.X)
                return projection;
            else
                return -projection;
        }

        private List<Segment> CloseContour(List<Segment> segments)
        {
            INode start = null;
            INode end = null;
            foreach (var segment in segments)
            {
                var firstConter = 0;
                var secondConter = 0;
                for (var i = 0; i < segments.Count; i++)
                {
                    if (segments[i].First.CoordinatesEqual(segment.First) || segments[i].Last.CoordinatesEqual(segment.First))
                        firstConter++;
                    if (segments[i].First.CoordinatesEqual(segment.Last) || segments[i].Last.CoordinatesEqual(segment.Last))
                        secondConter++;
                }
                if (firstConter != 2)
                    if (start == null)
                        start = segment.First;
                    else
                        end = segment.First;
                if (secondConter != 2)
                    if (end == null)
                        end = segment.Last;
                    else
                        start = segment.First;
            }
            if (start == null || end == null)
                throw new Exception();
            segments.Add(new Segment(start, end));
            return segments;
        }

        private List<Segment> CloseContour(List<Segment> segments, IFiniteElement triangle)
        {
            for (var i = 0; i < triangle.Nodes.Count; i++)
            {
                var flag = false;
                foreach (var segment in segments)
                    if (segment.First.CoordinatesEqual(triangle.Nodes[i]) || segment.Last.CoordinatesEqual(triangle.Nodes[i]))
                    {
                        flag = true;
                        break;
                    }
                if (!flag)
                {
                    segments.Add(new Segment(triangle.Nodes[i], triangle.Nodes[(i + 1) % triangle.Nodes.Count]));
                    segments.Add(new Segment(triangle.Nodes[i], triangle.Nodes[(i + 2) % triangle.Nodes.Count]));
                    break;
                }
            }
            return segments;
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

        private class Segment
        {
            public INode First { get; set; }
            public INode Last { get; set; }

            public Segment(INode firstNode, INode secondNode)
            {
                First = firstNode;
                Last = secondNode;
            }

            public override bool Equals(object? obj)
            {
                return obj is Segment segment && First.CoordinatesEqual(segment.First) &&
                       Last.CoordinatesEqual(segment.Last);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(First, Last);
            }
        }
            
    }
}