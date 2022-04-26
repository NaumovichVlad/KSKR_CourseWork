using AnalyzerUI.ModalWindows;
using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.Meshes;
using StressStrainStateAnalyzer.Meshes.Factory;
using StressStrainStateAnalyzer.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
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
        private Mesh? _mesh;
        public MainWindow()
        {
            InitializeComponent();
            InitializeInputs();
            InitializeAxis(double.Parse(WidthTextBox.Text) * 1000, double.Parse(HeightTextBox.Text) * 1000, 10);
        }

        private void InitializeInputs()
        {
            HeightTextBox.Text = "0,1";
            WidthTextBox.Text = "0,2";
            DepthTextBox.Text = "0,0001";
            L1TextBox.Text = "0,08";
            R1TextBox.Text = "0,04";
            L2TextBox.Text = "0,04";
            R2TextBox.Text = "0,02";
            PressTextBox.Text = "100";
        }

        private void BuildMeshBtn_Click(object sender, RoutedEventArgs e)
        {
            var modalWin = new MeshModalWindow();
            modalWin.MaxSquareTextBox.Text = "0,0001";
            modalWin.MinAngleTextBox.Text = "20";
            modalWin.ShowDialog();
            if (!modalWin.IsCancel)
            {
                Canvas.Children.Clear();
                _maxSize = double.Parse(modalWin.MaxSquareTextBox.Text) * 1000000;
                _minAngle = double.Parse(modalWin.MinAngleTextBox.Text);
                var nodes = BuildMesh(double.Parse(WidthTextBox.Text) * 1000, double.Parse(HeightTextBox.Text) * 1000, double.Parse(L1TextBox.Text) * 1000,
                    double.Parse(R1TextBox.Text) * 1000, double.Parse(L2TextBox.Text) * 1000, double.Parse(R2TextBox.Text) * 1000);
                _mesh = MeshFactory.GetMesh(FiniteElementsTypes.Triangular, nodes, _maxSize, _minAngle);
                DrawMesh(_mesh.FiniteElements, double.Parse(WidthTextBox.Text) * 1000, double.Parse(HeightTextBox.Text) * 1000);
                InitializeSquareLabels(_mesh);
                InitializeAxis(double.Parse(WidthTextBox.Text) * 1000, double.Parse(HeightTextBox.Text) * 1000, 10);
                Canvas.Children.Add(XLabel);
                Canvas.Children.Add(YLabel);
                Canvas.Children.Add(X10Label);
                Canvas.Children.Add(Y10Label);
                MakeCalculationsBtn.IsEnabled = true;
            }

        }

        private void InitializeSquareLabels(Mesh mesh)
        {
            var squares = mesh.GetSquares();
            CountLabel.Content = "Number of elements: " + squares.Count;
            AvSquareLabel.Content = "Average square: " + Math.Round(squares.Average(), 8) + " m^2";
            MaxSquareLabel.Content = "Max square: " + Math.Round(squares.Max(), 8) + " m^2";
            MinSquareLabel.Content = "Min square: " + Math.Round(squares.Min(), 8) + " m^2";
            Canvas.Children.Add(AvSquareLabel);
            Canvas.Children.Add(MaxSquareLabel);
            Canvas.Children.Add(MinSquareLabel);
            Canvas.Children.Add(CountLabel);
        }

        private void InitializeAxis(double a, double b, double segmentSize)
        {
            var yAxis = new Line();
            yAxis.X1 = 45;
            yAxis.Y1 = FormatY(0, b);
            yAxis.X2 = 45;
            yAxis.Y2 = FormatY(0, b) - 50;
            yAxis.Stroke = Brushes.Blue;
            yAxis.StrokeThickness = 2;
            var yPoint = new Line();
            yPoint.X1 = yAxis.X1 + 5;
            yPoint.Y1 = FormatY(segmentSize, b);
            yPoint.X2 = yAxis.X1 - 5;
            yPoint.Y2 = FormatY(segmentSize, b);
            yPoint.Stroke = Brushes.Blue;
            yPoint.StrokeThickness = 2;
            var yPointZero = new Line();
            yPointZero.X1 = yAxis.X1 + 5;
            yPointZero.Y1 = FormatY(0, b);
            yPointZero.X2 = yAxis.X1 - 5;
            yPointZero.Y2 = FormatY(0, b);
            yPointZero.Stroke = Brushes.Blue;
            yPointZero.StrokeThickness = 2;
            var yLeftArrow = new Line();
            yLeftArrow.X1 = 45;
            yLeftArrow.Y1 = FormatY(0, b) - 50;
            yLeftArrow.X2 = 40;
            yLeftArrow.Y2 = FormatY(0, b) - 40;
            yLeftArrow.Stroke = Brushes.Blue;
            yLeftArrow.StrokeThickness = 2;
            var yRightArrow = new Line();
            yRightArrow.X1 = 45;
            yRightArrow.Y1 = FormatY(0, b) - 50;
            yRightArrow.X2 = 50;
            yRightArrow.Y2 = FormatY(0, b) - 40;
            yRightArrow.Stroke = Brushes.Blue;
            yRightArrow.StrokeThickness = 2;

            Canvas.Children.Add(yAxis);
            Canvas.Children.Add(yPoint);
            Canvas.Children.Add(yPointZero);
            Canvas.Children.Add(yLeftArrow);
            Canvas.Children.Add(yRightArrow);

            var xAxis = new Line();
            xAxis.X1 = FormatX(0, a);
            xAxis.Y1 = FormatY(0, b) + 35;
            xAxis.X2 = FormatX(0, a) + 50;
            xAxis.Y2 = FormatY(0, b) + 35;
            xAxis.Stroke = Brushes.Red;
            xAxis.StrokeThickness = 2;
            var xPoint = new Line();
            xPoint.X1 = FormatX(segmentSize, a);
            xPoint.Y1 = xAxis.Y1 + 5;
            xPoint.X2 = FormatX(segmentSize, a);
            xPoint.Y2 = xAxis.Y1 - 5;
            xPoint.Stroke = Brushes.Red;
            xPoint.StrokeThickness = 2;
            var xPointZero = new Line();
            xPointZero.X1 = FormatX(0, a);
            xPointZero.Y1 = xAxis.Y1 + 5;
            xPointZero.X2 = FormatX(0, a);
            xPointZero.Y2 = xAxis.Y1 - 5;
            xPointZero.Stroke = Brushes.Red;
            xPointZero.StrokeThickness = 2;
            var xLeftArrow = new Line();
            xLeftArrow.X1 = FormatX(0, a) + 50;
            xLeftArrow.Y1 = FormatY(0, b) + 35;
            xLeftArrow.X2 = FormatX(0, a) + 45;
            xLeftArrow.Y2 = FormatY(0, b) + 40;
            xLeftArrow.Stroke = Brushes.Red;
            xLeftArrow.StrokeThickness = 2;
            var xRightArrow = new Line();
            xRightArrow.X1 = FormatX(0, a) + 50;
            xRightArrow.Y1 = FormatY(0, b) + 35;
            xRightArrow.X2 = FormatX(0, a) + 45;
            xRightArrow.Y2 = FormatY(0, b) + 30;
            xRightArrow.Stroke = Brushes.Red;
            xRightArrow.StrokeThickness = 2;
            Canvas.Children.Add(xAxis);
            Canvas.Children.Add(xPoint);
            Canvas.Children.Add(xPointZero);
            Canvas.Children.Add(xRightArrow);
            Canvas.Children.Add(xLeftArrow);
            X10Label.Content = "10 mm";
            XLabel.Content = "X";
            YLabel.Content = "Y";
            Y10Label.Content = "10 mm";
            X10Label.Visibility = Visibility.Visible;

        }

        private List<INode> BuildMesh(double a, double b, double l1, double r1, double l2, double r2)
        {
            var nodes = new List<INode>();
            var lengthA = a / CalculateSplitsCount(a, _maxSize);
            var lengthB = b / CalculateSplitsCount(b, _maxSize);
            for (var i = b; i > 0; i -= lengthB)
                nodes.Add(new Node() { X = 0, Y = i });
            nodes.Add(new Node() { X = 0, Y = 0 });
            for (var i = lengthA; i < a; i += lengthA)
                nodes.Add(new Node() { X = i, Y = 0 });
            nodes.Add(new Node() { X = a, Y = 0 });
            for (var i = lengthB; i < b; i += lengthB)
                nodes.Add(new Node() { X = a, Y = i });
            nodes.Add(new Node() { X = a, Y = b });
            var startLength = (a - l1 - l2) / 4;
            var lengthS = startLength / CalculateSplitsCount(startLength, _maxSize);
            var splits = new List<INode>();
            for (var i = lengthS; i < startLength - lengthS / 4; i += lengthS)
                splits.Add(new Node() { X = i, Y = b });
            var sectors = AddSector(l1, r1, startLength, a, b, CalculateSplitsCount(CalculateArcLength(r1, l1), _maxSize));
            startLength *= 2;
            startLength += l1;
            lengthS = (startLength - sectors[sectors.Count - 1].X) / CalculateSplitsCount((startLength - sectors[sectors.Count - 1].X), _maxSize);
            for (var i = sectors[sectors.Count - 1].X + lengthS; i < startLength - lengthS / 4; i += lengthS)
                splits.Add(new Node() { X = i, Y = b });
            sectors.AddRange(AddSector(l2, r2, startLength, a, b, CalculateSplitsCount(CalculateArcLength(r2, l2), _maxSize)));
            lengthS = (a - sectors[sectors.Count - 1].X) / CalculateSplitsCount((a - sectors[sectors.Count - 1].X), _maxSize);
            for (var i = sectors[sectors.Count - 1].X + lengthS; i < a - lengthS / 4; i += lengthS)
                splits.Add(new Node() { X = i, Y = b });
            sectors.AddRange(splits);
            nodes.AddRange(sectors.OrderByDescending(n => n.X));

            return nodes;
        }


        private List<INode> AddSector(double l, double r, double startLength, double a, double b, int nodesCount)
        {
            nodesCount *= 2;
            var nodes = new List<INode>();
            INode firstPoint = new Node() { X = startLength, Y = b };
            double centerY = Math.Sqrt(Math.Pow(r, 2) - Math.Pow(l / 2, 2));
            nodes.Add(firstPoint);
            for (var i = 0; i < nodesCount; i++)
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
            var splitCount = 2 * CalculateSplitsCount(CalculateArcLength(r, l), _maxSize);
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
            var side = Math.Sqrt(4 * elementSquare / Math.Sqrt(3));
            return (int)Math.Round(length / side);
        }

        private void DrawMesh(List<IFiniteElement> elements, double a, double b)
        {
            Canvas.Children.Clear();
            foreach (var element in elements)
            {
                var points = element.Nodes.Select(n =>
                    new Point(FormatX(n.X * 1000, a), FormatY(n.Y * 1000, b))).ToList();
                var triangle = new Polygon();
                triangle.Points = new PointCollection(points);
                triangle.Stroke = Brushes.Black;
                Canvas.Children.Add(triangle);
            }
        }

        private double FormatX(double coordinate, double figureSideSize)
        {
            var ratio = Canvas.Width / Canvas.Height * 0.8;
            return coordinate / ratio * Canvas.Width / figureSideSize + Canvas.Width * 0.13;
        }

        private double FormatY(double coordinate, double figureSideSize)
        {
            var ratio = Canvas.Width / Canvas.Height * 0.8;
            return -coordinate / ratio * Canvas.Height / figureSideSize + Canvas.Height * 0.8;
        }

        private void MakeCalculationsBtn_Click(object sender, RoutedEventArgs e)
        {
            _mesh.MakeCalculations(double.Parse(DepthTextBox.Text), double.Parse(PressTextBox.Text), 
                double.Parse(PuassonKoefTextBox.Text), double.Parse(JungKoefTextBox.Text));
        }
    }
}
