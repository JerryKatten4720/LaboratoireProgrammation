using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LaboratoireProgrammation.Project.ViewModels.Exercices;

public partial class Exo6 : UserControl {
    private readonly Brush[] _bgColors = {
        new SolidColorBrush(Color.FromRgb(0, 0, 128)), Brushes.Black, new SolidColorBrush(Color.FromRgb(15, 15, 20)),
        Brushes.DarkRed
    };

    private readonly Brush[] _lineColors =
        { Brushes.Yellow, Brushes.Cyan, Brushes.Lime, Brushes.Magenta, Brushes.White };

    private Brush _bgColor = new SolidColorBrush(Color.FromRgb(8, 8, 8));
    private int _bgColorIndex;
    private Brush _lineColor = Brushes.Cyan;

    private int _lineColorIndex;

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

        var numVertices = (int)SldVertices.Value;
        var density = SldDensity.Value / 100.0;
        var depth = (int)SldDepth.Value;

        var width = SpirographCanvas.ActualWidth;
        var height = SpirographCanvas.ActualHeight;

        if (width == 0 || height == 0) return;

        var cx = width / 2;
        var cy = height / 2;
        var radius = Math.Min(cx, cy) * 0.9;

        var points = new Point[numVertices];
        for (var i = 0; i < numVertices; i++) {
            var angle = 2 * Math.PI * i / numVertices - Math.PI / 2;
            points[i] = new Point(cx + radius * Math.Cos(angle), cy + radius * Math.Sin(angle));
        }

        for (var d = 0; d < depth; d++) {
            var poly = new Polygon {
                Stroke = _lineColor,
                StrokeThickness = 1,
                Fill = Brushes.Transparent
            };

            foreach (var p in points) poly.Points.Add(p);

            SpirographCanvas.Children.Add(poly);

            var nextPoints = new Point[numVertices];
            for (var i = 0; i < numVertices; i++) {
                var p1 = points[i];
                var p2 = points[(i + 1) % numVertices];

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