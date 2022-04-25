using Microsoft.VisualStudio.TestTools.UnitTesting;
using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.MeshBulders;
using StressStrainStateAnalyzer.Nodes;
using System;
using System.Collections.Generic;
using System.Reflection;
using static StressStrainStateAnalyzer.MeshBulders.TriangleMeshBuilder;

namespace StressStrainStateAnalyzerTests
{
    [TestClass]
    public class TriangleMeshBuilderTests
    {
        [TestMethod]
        public void FindCircleCenter_Test1()
        {
            var testTriangle = new TriangleFiniteElement(
                new Node() { X = 1, Y = 1 },
                new Node() { X = 3, Y = 5 },
                new Node() { X = 7, Y = 2 }
            );
            var parameters = new object[1] { testTriangle };
            var expected = new Node { X = 3.90909, Y = 2.04545 };
            var actual = Invoke<INode>("FindCircleCenter", parameters);
            Assert.IsTrue(Math.Abs(expected.X - actual.X) < 0.00001);
            Assert.IsTrue(Math.Abs(expected.Y - actual.Y) < 0.00001);
        }

        [TestMethod]
        public void CalculateAngle_Test1()
        {
            var firstTestNode = new Node() { X = 3, Y = 9 };
            var secondTestNode = new Node() { X = 10, Y = 2 };
            var vertexNode = new Node() { X = 3, Y = 2 };
            var parameters = new object[] { firstTestNode, secondTestNode, vertexNode };
            var expected = 90;
            var actual = (double)Invoke<object>("CalculateAngle", parameters);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CalculateAngle_Test2()
        {
            var firstTestNode = new Node() { X = 12, Y = 5 };
            var secondTestNode = new Node() { X = 10, Y = 2 };
            var vertexNode = new Node() { X = 3, Y = 2 };
            var parameters = new object[] { firstTestNode, secondTestNode, vertexNode };
            var expected = 18.434948822922017;
            var actual = (double)Invoke<object>("CalculateAngle", parameters);
            Assert.IsTrue(Math.Abs(expected - actual) < 0.000001);
        }

        [TestMethod]
        public void CalculateAngle_Test3()
        {
            var firstTestNode = new Node() { X = 0, Y = 1 };
            var secondTestNode = new Node() { X = 10, Y = 2 };
            var vertexNode = new Node() { X = 3, Y = 2 };
            var parameters = new object[] { firstTestNode, secondTestNode, vertexNode };
            var expected = 161.56505117707798;
            var actual = (double)Invoke<object>("CalculateAngle", parameters);
            Assert.IsTrue(Math.Abs(expected - actual) < 0.000001);
        }

        [TestMethod]
        public void CalculateAngle_Test4()
        {
            var firstTestNode = new Node() { X = 2, Y = 1 };
            var secondTestNode = new Node() { X = 1, Y = 0 };
            var vertexNode = new Node() { X = 3, Y = 2 };
            var parameters = new object[] { firstTestNode, secondTestNode, vertexNode };
            var expected = 0;
            var actual = (double)Invoke<object>("CalculateAngle", parameters);
            Assert.IsTrue(Math.Abs(expected - actual) < 0.000001);
        }

        [TestMethod]
        public void ChangeDiagonal_Test1()
        {
            var firstTestTraingle = new TriangleFiniteElement(
                new Node() { X = 1, Y = 2 },
                new Node() { X = 3, Y = 1 },
                new Node() { X = 4, Y = 5 }
            );
            var secondTestTraingle = new TriangleFiniteElement(
                new Node() { X = 3, Y = 1 },
                new Node() { X = 4, Y = 3 },
                new Node() { X = 4, Y = 5 }
            );
            var testSegment = new TriangleMeshBuilder.Segment(new Node() { X = 3, Y = 1 }, new Node() { X = 4, Y = 5 });
            var parameters = new object[] { firstTestTraingle, secondTestTraingle, testSegment };
            var expected = new List<IFiniteElement>()
            {
                new TriangleFiniteElement(new Node() { X = 1, Y = 2 }, new Node() { X = 3, Y = 1 }, new Node() { X = 4, Y = 3 }),
                new TriangleFiniteElement(new Node() { X = 1, Y = 2 }, new Node() { X = 4, Y = 5 }, new Node() { X = 4, Y = 3 })
            };
            var actual = Invoke<List<IFiniteElement>>("ChangeDiagonal", parameters);
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.IsTrue(expected.Contains(actual[0]));
            Assert.IsTrue(expected.Contains(actual[1]));
        }

        [TestMethod]
        public void CloseContour_Test1()
        {
            var expectedCount = 4;
            var testSegments = new List<Segment>()
            {
                new Segment(new Node() { X = 3, Y = 2 }, new Node() {X = 2, Y = 1}),
                new Segment(new Node() { X = 3, Y = 2 }, new Node() {X = 2, Y = 4}),
                new Segment(new Node() { X = 2, Y = 1 }, new Node() {X = 5, Y = 2})
            };
            var parameters = new object[] { testSegments };
            var expectedSegment = new Segment(new Node() { X = 2, Y = 4 }, new Node() { X = 5, Y = 2 });
            var actual = Invoke<List<Segment>>("CloseContour", new Type[] { typeof(List<Segment>) }, parameters);
            Assert.AreEqual(expectedCount, actual.Count);
            var actualSegment = actual.Find(s =>
            (s.First.CoordinatesEqual(expectedSegment.Last) && s.Last.CoordinatesEqual(expectedSegment.First)) ||
            (s.First.CoordinatesEqual(expectedSegment.First) && s.Last.CoordinatesEqual(expectedSegment.Last)));
            Assert.IsNotNull(actualSegment);
        }

        [TestMethod]
        public void CloseContour_Test2()
        {
            var expectedCount = 4;
            var testSegments = new List<Segment>()
            {
                new Segment(new Node() { X = 4, Y = 3 }, new Node() {X = 5, Y = 5}),
                new Segment(new Node() { X = 7, Y = 5 }, new Node() {X = 5, Y = 5})
            };
            IFiniteElement testTriangle = new TriangleFiniteElement(
                new Node() { X = 4, Y = 3 },
                new Node() { X = 7, Y = 5 },
                new Node() { X = 5, Y = 3 }
            );
            var parameters = new object[] { testSegments, testTriangle };
            var expectedSegments = new List<Segment>() {
                new Segment(new Node() { X = 4, Y = 3 }, new Node() { X = 5, Y = 3 }),
                new Segment(new Node() { X = 5, Y = 3 }, new Node() { X = 7, Y = 5 })
                };
            var actual = Invoke<List<Segment>>("CloseContour", new Type[] { typeof(List<Segment>), typeof(IFiniteElement) }, parameters);
            Assert.AreEqual(expectedCount, actual.Count);
            var actualSegment = new List<Segment>();
            foreach (var expectedSegment in expectedSegments)
            {
                actualSegment.AddRange(actual.FindAll(s =>
                (s.First.CoordinatesEqual(expectedSegment.Last) && s.Last.CoordinatesEqual(expectedSegment.First)) ||
                (s.First.CoordinatesEqual(expectedSegment.First) && s.Last.CoordinatesEqual(expectedSegment.Last))));
            }
            Assert.AreEqual(expectedSegments.Count, actualSegment.Count);
        }

        [TestMethod]
        public void FindThirdVertexIndex_Test1()
        {
            var testNodes = new List<INode>()
            {
                new Node(){X = 8, Y = 5},
                new Node(){X = 10, Y = 2},
                new Node(){X = 11, Y = 7},
                new Node(){X = 11, Y = 4},
                new Node(){X = 9, Y = 7},
                new Node(){X = 10, Y = 5},
                new Node(){X = 9, Y = 3},
            };
            var firstTestNode = new Node() { X = 9, Y = 3 };
            var secondTestNode = new Node() { X = 8, Y = 5 };
            var parameters = new object[] { firstTestNode, secondTestNode, testNodes };
            var expected = 5;
            var actual = Invoke<object>("FindThirdVertexIndex", new Type[] {typeof(INode), typeof(INode), typeof(List<INode>)}, parameters);
            Assert.AreEqual(expected, (int)actual);
        }

        private T? Invoke<T>(string methodName, object[] parameters) where T : class
        {
            var builder = new TriangleMeshBuilder();
            var testType = builder.GetType();
            var testMethod = testType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return testMethod.Invoke(builder, parameters) as T;
        }

        private T? Invoke<T>(string methodName, Type[] parametersTypes, object[] parameters) where T : class
        {
            var builder = new TriangleMeshBuilder();
            var testType = builder.GetType();
            var testMethod = testType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance,
                null, parametersTypes, null);
            return testMethod.Invoke(builder, parameters) as T;
        }
    }
}