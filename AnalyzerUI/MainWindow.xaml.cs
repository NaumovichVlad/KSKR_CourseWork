using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.Meshes.Factory;
using StressStrainStateAnalyzer.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnalyzerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double maxSize = 200;
        private const double minAngle = 25;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BuildMeshBtn_Click(object sender, RoutedEventArgs e)
        {
            var mesh = BuildMesh(150, 75, 40, 25, 25, 15);
            DrawMesh(mesh, 150, 75);
        }

        private List<INode> BuildMesh(double a, double b, double l1, double r1, double l2, double r2)
        {
            var nodes = new List<INode>()
            {
                new Node() {X = 0, Y = b},
                new Node() {X = 0, Y = 0},
                new Node() {X = a, Y = 0},
                new Node() {X = a, Y = b},
            };
            var startLength = (a - l1 - l2) / 4;
            var sectors = AddSector(l1, r1, startLength, a, b, CalculateSplitsCount(l1, 10));
            startLength *= 2;
            startLength += l1;
            sectors.AddRange(AddSector(l2, r2, startLength, a, b, CalculateSplitsCount(l2, 5)));
            nodes.AddRange(sectors.OrderByDescending(n => n.X));

            return nodes;
        }

        private List<INode> AddSector(double l, double r, double startLength, double a, double b, int nodesCount)
        {
            var nodes = new List<INode>();
            for (int i = 0; i < nodesCount + 1; i++)
                nodes.Add(new Node() { X = startLength + l / nodesCount * i, Y = b });
            double centerY = Math.Sqrt(Math.Pow(r, 2) - Math.Pow(l / 2, 2));
            for (var i = 1; i < nodes.Count - 1; i++)
                nodes[i].Y -= Math.Sqrt(Math.Abs(Math.Pow(r, 2) - Math.Pow(nodes[i].X - (startLength + l / 2), 2))) - centerY;
            return nodes;
        }

        private int CalculateSplitsCount(double length, double elementSquare)
        {
            var side = Math.Sqrt(4 * elementSquare);
            return (int)Math.Round(length / side);
        }

        private void DrawMesh(List<INode> nodes, double a, double b)
        {
            var mesh = MeshFactory.GetMesh(FiniteElementsTypes.Triangular, nodes, maxSize, minAngle);
            var elements = mesh.FiniteElements;
            foreach (var element in elements)
            {
                if (element != null)
                    for (var i = 0; i < element.Nodes.Count; i++)
                    {
                        var line = new Line();
                        line.X1 = FormatX(element.Nodes[i].X, a);
                        line.Y1 = FormatY(element.Nodes[i].Y, b);
                        line.X2 = FormatX(element.Nodes[(i + 1) % element.Nodes.Count].X, a);
                        line.Y2 = FormatY(element.Nodes[(i + 1) % element.Nodes.Count].Y, b);
                        line.Stroke = Brushes.Black;
                        Canvas.Children.Add(line);
                    }
            }
        }

        private double FormatX(double coordinate, double figureSideSize)
        {
            var ratio = Canvas.Width / Canvas.Height * 0.8;
            return coordinate / ratio * Canvas.Width / figureSideSize + Canvas.Width * 0.2;
        }

        private double FormatY(double coordinate, double figureSideSize)
        {
            var ratio = Canvas.Width / Canvas.Height * 0.8;
            return -coordinate / ratio * Canvas.Height / figureSideSize + Canvas.Height * 0.8;
        }
    }
}
