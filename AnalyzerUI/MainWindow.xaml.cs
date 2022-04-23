using AnalyzerUI.ModalWindows;
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
        private double _maxSize = 40;
        private double _minAngle = 25;
        public MainWindow()
        {
            InitializeComponent();
            InitializeInputs();
        }

        private void InitializeInputs()
        {
            HeightTextBox.Text = "100";
            WidthTextBox.Text = "200";
            DepthTextBox.Text = "1";
            L1TextBox.Text = "80";
            R1TextBox.Text = "40";
            L2TextBox.Text = "40";
            R2TextBox.Text = "20";
            PressTextBox.Text = "100";

        }

        private void BuildMeshBtn_Click(object sender, RoutedEventArgs e)
        {
            var modalWin = new MeshModalWindow();
            modalWin.MaxSquareTextBox.Text = "10";
            modalWin.MinAngleTextBox.Text = "25";
            modalWin.ShowDialog();
            if (!modalWin.IsCancel)
            {
                Canvas.Children.Clear();
                _maxSize = double.Parse(modalWin.MaxSquareTextBox.Text);
                _minAngle = double.Parse(modalWin.MinAngleTextBox.Text);
                var mesh = BuildMesh(double.Parse(WidthTextBox.Text), double.Parse(HeightTextBox.Text), double.Parse(L1TextBox.Text),
                    double.Parse(R1TextBox.Text), double.Parse(L2TextBox.Text), double.Parse(R2TextBox.Text));
                DrawMesh(mesh, double.Parse(WidthTextBox.Text), double.Parse(HeightTextBox.Text));
            }
            
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
            var sectors = AddSector(l1, r1, startLength, a, b, CalculateSplitsCount(CalculateArcLength(r1, l1), _maxSize));
            startLength *= 2;
            startLength += l1;
            sectors.AddRange(AddSector(l2, r2, startLength, a, b, CalculateSplitsCount(CalculateArcLength(r2, l2), _maxSize)));
            nodes.AddRange(sectors.OrderByDescending(n => n.X));

            return nodes;
        }


        private List<INode> AddSector(double l, double r, double startLength, double a, double b, int nodesCount)
        {
            var nodes = new List<INode>();
            INode firstPoint = new Node() { X = startLength, Y = b };
            double centerY = Math.Sqrt(Math.Pow(r, 2) - Math.Pow(l / 2, 2));
            nodes.Add(firstPoint);
            for ( var i = 0; i < nodesCount; i++)
            {
                firstPoint = CalculateProjectionSize(r, l, b, centerY, startLength, firstPoint);
                nodes.Add(firstPoint);
            }
            nodes.RemoveAll(n => n.X > startLength + l);
            nodes.Add(new Node() { X = startLength + l, Y = b });
            return nodes;
        }

        private INode CalculateProjectionSize(double r, double l, double b, double centerY, double startLength, INode firstPoint)
        {
            var splitCount = CalculateSplitsCount(CalculateArcLength(r, l), _maxSize);
            var length = CalculateArcLength(r, l) / splitCount;
            var newPoint = new Node() { X = firstPoint.X + 0.1 };
            double newLength;
            do
            {
                newPoint.X += 0.1;
                newPoint.Y = b - Math.Abs(Math.Sqrt(Math.Abs(Math.Pow(r, 2) - Math.Pow(newPoint.X - (startLength + l / 2), 2))) - centerY);
                newLength = Math.Sqrt(Math.Pow(firstPoint.X - newPoint.X, 2) + Math.Pow(firstPoint.Y - newPoint.Y, 2));
            } while (newLength < length);
            return newPoint;
        }

        private double CalculateArcLength(double r, double l)
        {
            return CalculateSegmentAngle(r, l) * r;
        }

        private double CalculateSegmentAngle(double r, double l)
        {
            return Math.Acos((2 * Math.Pow(r, 2) - Math.Pow(l, 2)) / 2 / Math.Pow(r, 2));
        }

        


        private int CalculateSplitsCount(double length, double elementSquare)
        {
            var side = Math.Sqrt(4 * elementSquare);
            return (int)Math.Round(length / side);
        }

        private void DrawMesh(List<INode> nodes, double a, double b)
        {
            var mesh = MeshFactory.GetMesh(FiniteElementsTypes.Triangular, nodes, _maxSize, _minAngle);
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
