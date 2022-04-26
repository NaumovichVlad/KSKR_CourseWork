using Microsoft.VisualStudio.TestTools.UnitTesting;
using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.Nodes;
using System;

namespace StressStrainStateAnalyzerTests
{
    [TestClass]
    public class TriangleFiniteElementTests
    {
        [TestMethod]
        public void CalculateSquare_Test1()
        {
            var testTriangle = new TriangleFiniteElement(
                new Node() { X = 3, Y = 2 },
                new Node() { X = 3, Y = 9 },
                new Node() { X = 10, Y = 2 }
            );
            var expected = 24.5;
            var actual = testTriangle.CalculateSquare();
            Assert.IsTrue(Math.Abs(expected - actual) < 0.000001);
        }

        [TestMethod]
        public void CalculateSquare_Test2()
        {
            var testTriangle = new TriangleFiniteElement(
                new Node() { X = 3, Y = 2 },
                new Node() { X = 3, Y = 2 },
                new Node() { X = 10, Y = 2 }
            );
            var expected = 0;
            var actual = testTriangle.CalculateSquare();
            Assert.IsTrue(Math.Abs(expected - actual) < 0.000001);
        }

        [TestMethod]
        public void CalculateSquare_Test3()
        {
            var testTriangle = new TriangleFiniteElement(
                new Node() { X = 2, Y = 10 },
                new Node() { X = 3, Y = 2 },
                new Node() { X = 10, Y = 5 }
            );
            var expected = 29.5;
            var actual = testTriangle.CalculateSquare();
            Assert.IsTrue(Math.Abs(expected - actual) < 0.000001);
        }
    }
}
