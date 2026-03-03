using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LaboratoireProgrammation.Project.Views.Exercices {
    public partial class Exo6 : UserControl {
        private Brush _lineColor = Brushes.Cyan;
        private Brush _bgColor = new SolidColorBrush(Color.FromRgb(8, 8, 8));

        private Brush[] _lineColors = { Brushes.Yellow, Brushes.Cyan, Brushes.Lime, Brushes.Magenta, Brushes.White };
        private Brush[] _bgColors = { new SolidColorBrush(Color.FromRgb(0, 0, 128)), Brushes.Black, new SolidColorBrush(Color.FromRgb(15, 15, 20)), Brushes.DarkRed };
        
        private int _lineColorIndex = 0;
        private int _bgColorIndex = 0;

        public Exo6() {
            InitializeComponent();
            
            Loaded += (s, e) => DrawSpirograph();
            SizeChanged += (s, e) => DrawSpirograph();
        }

        private void BtnLineColor_Click(object sender, RoutedEventArgs e) {
            _lineColorIndex = (_lineColorIndex + 1) % _lineColors.Length;
            _lineColor = _lineColors[_lineColorIndex];
            DrawSpirograph();
        }

        private void BtnBgColor_Click(object sender, RoutedEventArgs e) {
            _bgColorIndex = (_bgColorIndex + 1) % _bgColors.Length;
            _bgColor = _bgColors[_bgColorIndex];
            DrawSpirograph();
        }

        private void DrawSpirograph() {
            SpirographCanvas.Children.Clear();
            SpirographCanvas.Background = _bgColor;

            int numVertices = (int)SldVertices.Value;
            double density = SldDensity.Value / 100.0; 
            int depth = (int) SldDepth.Value;

            double width = SpirographCanvas.ActualWidth;
            double height = SpirographCanvas.ActualHeight;

            if (width == 0 || height == 0) return;

            double cx = width / 2;
            double cy = height / 2;
            double radius = Math.Min(cx, cy) * 0.9;

            Point[] points = new Point[numVertices];
            for (int i = 0; i < numVertices; i++) {
                double angle = 2 * Math.PI * i / numVertices - Math.PI / 2;
                points[i] = new Point(cx + radius * Math.Cos(angle), cy + radius * Math.Sin(angle));
            }

            for (int d = 0; d < depth; d++) {
                Polygon poly = new Polygon {
                    Stroke = _lineColor,
                    StrokeThickness = 1,
                    Fill = Brushes.Transparent
                };

                foreach (Point p in points) { poly.Points.Add(p); }
                
                SpirographCanvas.Children.Add(poly);

                Point[] nextPoints = new Point[numVertices];
                for (int i = 0; i < numVertices; i++) {
                    Point p1 = points[i];
                    Point p2 = points[(i + 1) % numVertices];
                    
                    nextPoints[i] = new Point(
                        p1.X + (p2.X - p1.X) * density,
                        p1.Y + (p2.Y - p1.Y) * density
                    );
                }
                
                points = nextPoints;
            }
        }


        private void UpdateSpirograph(object sender, MouseButtonEventArgs e) {
            DrawSpirograph();
        }
    }
}