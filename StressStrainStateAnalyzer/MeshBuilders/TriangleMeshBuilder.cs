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
            var newNodes = new List<INode>();
            _finiteElements.ForEach(e => newNodes.AddRange(e.Nodes));
            newNodes = newNodes.Distinct().ToList();
            foreach (var node in newNodes)
            {
                node.X /= 1000;
                node.Y /= 1000;
            };
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
                    secondPointIndex = nodes.IndexOf(segments[i].Last);
                    break;
                }
            }
            var vertexPointIndex = FindThirdVertexIndex(nodes[firstPointIndex], nodes[secondPointIndex], nodes);
            var test = CreateElements(firstPointIndex, secondPointIndex, vertexPointIndex, nodes, new List<IFiniteElement>(), segments).Distinct().ToList();
            return test;
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

        private List<IFiniteElement> ChangeDiagonal(IFiniteElement firstTriangle, IFiniteElement secondTriangle, Segment segment)
        {
            var minAngle = double.MaxValue;
            var square = firstTriangle.CalculateSquare() + secondTriangle.CalculateSquare();
            for (int i = 0; i < firstTriangle.Nodes.Count; i++)
            {
                var angle = CalculateAngle(firstTriangle.Nodes[i], firstTriangle.Nodes[(i + 1) % 3], firstTriangle.Nodes[(i + 2) % 3]);
                if (angle < minAngle)
                    minAngle = angle;
            }
            for (int i = 0; i < secondTriangle.Nodes.Count; i++)
            {
                var angle = CalculateAngle(secondTriangle.Nodes[i], secondTriangle.Nodes[(i + 1) % 3], secondTriangle.Nodes[(i + 2) % 3]);
                if (angle < minAngle)
                    minAngle = angle;
            }
            var newNodes = new List<INode>();
            newNodes.Add(firstTriangle.Nodes.Find(n => (!n.CoordinatesEqual(segment.First)) && (!n.CoordinatesEqual(segment.Last))));
            newNodes.Add(secondTriangle.Nodes.Find(n => (!n.CoordinatesEqual(segment.First)) && (!n.CoordinatesEqual(segment.Last))));
            var newElements = new List<IFiniteElement>()
            {
                new TriangleFiniteElement(newNodes[0], newNodes[1], segment.First),
                new TriangleFiniteElement(newNodes[0], newNodes[1], segment.Last)
            };
            var newSquare = newElements[0].CalculateSquare() + newElements[1].CalculateSquare();
            if (Math.Abs(square - newSquare) > 0.0000001)
                return new List<IFiniteElement>()
                {
                    firstTriangle, secondTriangle
                };
            var flag = false;
            foreach (var element in newElements)
            {
                for (int i = 0; i < element.Nodes.Count; i++)
                {
                    var angle = CalculateAngle(element.Nodes[i], element.Nodes[(i + 1) % 3], element.Nodes[(i + 2) % 3]);
                    if (angle == 0)
                    {
                        return new List<IFiniteElement>()
                        {
                            firstTriangle, secondTriangle
                        };
                    }
                    if (angle <= minAngle)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                    break;
            }
            if (!flag)
                return newElements;
            else
                return new List<IFiniteElement>()
                {
                    firstTriangle, secondTriangle
                };
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
            var flag = true;
            while (flag)
            {
                SplitSegments(maxSquare, angle);
                if (SplitMinAngle(angle))
                    continue;
                if (SplitBigSquare(maxSquare))
                    continue;
                flag = false;
            }


        }

        private void SplitSegments(double maxSquare, double minAngle)
        {
            for (var i = 0; i < _finiteElements.Count; i++)
            {
                var flag = false;
                if (_finiteElements[i].CalculateSquare() < maxSquare)
                    continue;
                for (var j = 0; j < _finiteElements[i].Nodes.Count; j++)
                {
                    var rad = GetLineLength(_finiteElements[i].Nodes[j], _finiteElements[i].Nodes[(j + 1) % _finiteElements[i].Nodes.Count]);
                    var circleCenter = GetLineCenter(_finiteElements[i].Nodes[j], _finiteElements[i].Nodes[(j + 1) % _finiteElements[i].Nodes.Count]);
                    circleCenter.X += Math.Pow(10, -2);
                    circleCenter.Y += Math.Pow(10, -2);
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
                        CheckDiagonals();
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
            _finiteElements.AddRange(BuildInitialPartition(newNodes, segments));
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
            neighbors = neighbors.Distinct().ToList();
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
                if (top == 3 || bottom == 3)
                {
                    belonging = true;
                    break;
                }
            }
            return belonging;
        }


        /*private double CalculateProjectionSize(INode lastNode, INode vertexNode, double length)
        {
            var tmpNode = new Node() { X = lastNode.X, Y = vertexNode.Y };
            var corner = CalculateAngle(lastNode, tmpNode, vertexNode);
            var projection = Math.Cos(corner / 180 * Math.PI) * length;
            if (vertexNode.X < lastNode.X)
                return projection;
            else
                return -projection;
        }*/

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
                        start = segment.Last;
            }
            if (start == null || end == null)
                return segments;
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

        private bool SplitMinAngle(double minAngle)
        {
            var flag = false;
            for (var i = 0; i < _finiteElements.Count; i++)
            {

                for (var j = 0; j < _finiteElements[i].Nodes.Count; j++)
                {
                    if (CalculateAngle(_finiteElements[i].Nodes[j], _finiteElements[i].Nodes[(j + 1) % _finiteElements[i].Nodes.Count],
                        _finiteElements[i].Nodes[(j + 2) % _finiteElements[i].Nodes.Count]) < minAngle)
                    {
                        var center = FindCircleCenter(_finiteElements[i]);
                        var deletedTriangles = FindTrianglesBySegment(_finiteElements[i].Nodes[j], _finiteElements[i].Nodes[(j + 2) % _finiteElements[i].Nodes.Count]);
                        deletedTriangles.AddRange(FindTrianglesBySegment(_finiteElements[i].Nodes[(j + 1) % _finiteElements[i].Nodes.Count],
                            _finiteElements[i].Nodes[(j + 2) % _finiteElements[i].Nodes.Count]));
                        deletedTriangles = deletedTriangles.Distinct().ToList();
                        deletedTriangles.Remove(_finiteElements[i]);
                        var belonging = false;
                        foreach (var triangle in deletedTriangles)
                            if (CheckBelonging(triangle, center))
                                belonging = true;
                        if (!belonging)
                            break;
                        flag = true;
                        var newNodes = new List<INode>();
                        var segments = new List<Segment>();
                        for (int k = 0; k < deletedTriangles.Count; k++)
                        {
                            for (var m = 0; m < deletedTriangles[k].Nodes.Count; m++)
                            {
                                if (_finiteElements[i].Nodes.Contains(deletedTriangles[k].Nodes[m])
                                    && _finiteElements[i].Nodes.Contains(deletedTriangles[k].Nodes[(m + 1) % deletedTriangles[k].Nodes.Count]))
                                {
                                    segments.Add(new Segment(deletedTriangles[k].Nodes[m],
                                        deletedTriangles[k].Nodes[(m + 2) % deletedTriangles[k].Nodes.Count]));
                                    segments.Add(new Segment(deletedTriangles[k].Nodes[(m + 1) % deletedTriangles[k].Nodes.Count],
                                        deletedTriangles[k].Nodes[(m + 2) % deletedTriangles[k].Nodes.Count]));
                                }
                            }

                        }
                        if (deletedTriangles.Count == 2)
                            segments = CloseContour(segments);
                        if (deletedTriangles.Count == 1)
                            segments = CloseContour(segments, _finiteElements[i]);
                        _finiteElements.Remove(_finiteElements[i]);
                        for (int k = 0; k < deletedTriangles.Count; k++)
                        {
                            _finiteElements.Remove(deletedTriangles[k]);
                            for (int m = 0; m < segments.Count; m++)
                            {
                                if (!newNodes.Contains(segments[m].Last))
                                    newNodes.Add(segments[m].Last);
                                if (!newNodes.Contains(segments[m].First))
                                    newNodes.Add(segments[m].First);
                            }
                        }
                        newNodes.Add(center);
                        _finiteElements.AddRange(BuildInitialPartition(newNodes, segments));
                        break;
                    }
                }
                if (flag)
                    break;
            }
            return flag;
        }

        private bool SplitBigSquare(double maxSquare)
        {
            var flag = false;
            for (var i = 0; i < _finiteElements.Count; i++)
            {
                for (var j = 0; j < _finiteElements[i].Nodes.Count; j++)
                {
                    if (_finiteElements[i].CalculateSquare() > maxSquare)
                    {
                        var center = FindCircleCenter(_finiteElements[i]);
                        var deletedTriangles = FindTrianglesBySegment(_finiteElements[i].Nodes[j], _finiteElements[i].Nodes[(j + 2) % _finiteElements[i].Nodes.Count]);
                        deletedTriangles.AddRange(FindTrianglesBySegment(_finiteElements[i].Nodes[(j + 1) % _finiteElements[i].Nodes.Count],
                            _finiteElements[i].Nodes[(j + 2) % _finiteElements[i].Nodes.Count]));
                        deletedTriangles.AddRange(FindTrianglesBySegment(_finiteElements[i].Nodes[j], _finiteElements[i].Nodes[(j + 1) % _finiteElements[i].Nodes.Count]));
                        deletedTriangles = deletedTriangles.Distinct().ToList();
                        deletedTriangles.Remove(_finiteElements[i]);
                        var belonging = false;
                        foreach (var triangle in deletedTriangles)
                            if (CheckBelonging(triangle, center))
                                belonging = true;
                        if (!belonging)
                            break;
                        flag = true;
                        var newNodes = new List<INode>();
                        var segments = new List<Segment>();
                        for (int k = 0; k < deletedTriangles.Count; k++)
                        {
                            for (var m = 0; m < deletedTriangles[k].Nodes.Count; m++)
                            {
                                if (_finiteElements[i].Nodes.Contains(deletedTriangles[k].Nodes[m])
                                    && _finiteElements[i].Nodes.Contains(deletedTriangles[k].Nodes[(m + 1) % deletedTriangles[k].Nodes.Count]))
                                {
                                    segments.Add(new Segment(deletedTriangles[k].Nodes[m],
                                        deletedTriangles[k].Nodes[(m + 2) % deletedTriangles[k].Nodes.Count]));
                                    segments.Add(new Segment(deletedTriangles[k].Nodes[(m + 1) % deletedTriangles[k].Nodes.Count],
                                        deletedTriangles[k].Nodes[(m + 2) % deletedTriangles[k].Nodes.Count]));
                                }
                            }

                        }
                        if (deletedTriangles.Count == 2)
                            segments = CloseContour(segments);
                        if (deletedTriangles.Count == 1)
                            segments = CloseContour(segments, _finiteElements[i]);
                        _finiteElements.Remove(_finiteElements[i]);
                        for (int k = 0; k < deletedTriangles.Count; k++)
                        {
                            _finiteElements.Remove(deletedTriangles[k]);
                            for (int m = 0; m < segments.Count; m++)
                            {
                                if (!newNodes.Contains(segments[m].Last))
                                    newNodes.Add(segments[m].Last);
                                if (!newNodes.Contains(segments[m].First))
                                    newNodes.Add(segments[m].First);
                            }
                        }
                        newNodes.Add(center);
                        _finiteElements.AddRange(BuildInitialPartition(newNodes, segments));
                        break;
                    }
                }
                if (flag)
                    break;
            }
            return flag;
        }

        private void CheckDiagonals()
        {
            for (var j = 0; j < _finiteElements.Count; j++)
            {
                var flag = false;
                for (var i = 0; i < _finiteElements[j].Nodes.Count; i++)
                {
                    var deletedElements = FindTrianglesBySegment(_finiteElements[j].Nodes[i], _finiteElements[j].Nodes[(i + 1) % _finiteElements[j].Nodes.Count]).Distinct().ToList();
                    deletedElements.Remove(_finiteElements[j]);
                    if (deletedElements.Count != 0)
                    {
                        var newElements = ChangeDiagonal(deletedElements[0], _finiteElements[j],
                            new Segment(_finiteElements[j].Nodes[i], _finiteElements[j].Nodes[(i + 1) % _finiteElements[j].Nodes.Count]));
                        if (!newElements.Contains(_finiteElements[j]))
                        {
                            flag = true;
                            _finiteElements.Remove(_finiteElements[j]);
                            _finiteElements.Remove(deletedElements[0]);
                            _finiteElements.AddRange(newElements);
                        }
                    }
                    if (flag)
                    {
                        j = 0;
                        break;
                    }
                }
            }
        }

        private INode FindCircleCenter(IFiniteElement element)
        {
            var x12 = element.Nodes[0].X - element.Nodes[1].X;
            var x23 = element.Nodes[1].X - element.Nodes[2].X;
            var x31 = element.Nodes[2].X - element.Nodes[0].X;
            var y12 = element.Nodes[0].Y - element.Nodes[1].Y;
            var y23 = element.Nodes[1].Y - element.Nodes[2].Y;
            var y31 = element.Nodes[2].Y - element.Nodes[0].Y;
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

        public class Segment
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