using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using FontFamily = System.Windows.Media.FontFamily;

namespace LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars.DwellerComponent;

public enum DwellerState {
    Selected,
    Ally,
    Enemy
}

public enum Rarity {
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public class Dweller {
    // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
    // [ - Constants - ]

    private const string AssetBase =
        "pack://application:,,,/LaboratoireProgrammation;component/Assets/images/overseerWars";

    private const string FontBase = "pack://application:,,,/LaboratoireProgrammation;component/Assets/fonts/";
    private static readonly int[] OutlineAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };
    private static readonly Brush StatBg = (SolidColorBrush)new BrushConverter().ConvertFrom("#4D000000");

    // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
    // [ - Fields - ]

    public string FirstName;
    public string LastName;
    public int S, P, E, C, I, A, L;
    public int X, Y;

    // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
    // [ - Constructors - ]

    public Dweller() { }

    public Dweller(string fn, string ln) {
        FirstName = fn;
        LastName = ln;
    }

    public Team Team { get; set; }
    public Outfit Outfit { get; set; }
    public Weapon Weapon { get; set; }
    public Rarity Rarity { get; set; }
    public string Texture { get; set; }
    public DwellerState CurrentState { get; set; }
    public int IndividuelActionPoints { get; set; } = 3;

    public void Move(int x, int y) {
        X = x;
        Y = y;
    }

    // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
    // [ - Visuals - ]

    public UIElement CreateCardVisual() {
        var rarity = Rarity.ToString().ToLower();
        var isHolo = Rarity == Rarity.Epic || Rarity == Rarity.Legendary;

        var skewX = new SkewTransform();
        var skewY = new SkewTransform();
        var tg = new TransformGroup();
        tg.Children.Add(skewX);
        tg.Children.Add(skewY);

        var cardGrid = new Grid {
            Width = 400, Height = 600,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = tg
        };

        AddCardImage(cardGrid, $"cards/card_border_{rarity}.png", 110);
        AddCardImage(cardGrid, $"cards/card_color_{rarity}.png", 109);
        AddCardImage(cardGrid, $"cards/card_top_{rarity}.png", 108);
        AddCardImage(cardGrid, "cards/card_circleborder.png", 107);
        AddCardImage(cardGrid, "cards/card_circle.png", 105);

        var dwellerImg = new Image {
            Source = LoadImage($"dwellers/{Texture}"),
            Stretch = Stretch.None,
            LayoutTransform = new ScaleTransform(1.3, 1.3),
            Margin = new Thickness(15, 200, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        AddToGrid(cardGrid, dwellerImg, 106);

        var nameText = new TextBlock {
            FontFamily = new FontFamily(new Uri(FontBase), "./#overseer"),
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.LightGray,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 44, 0, 0),
            Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 6, ShadowDepth = 3, Opacity = 0.7 }
        };
        nameText.Inlines.Add(new Run(FirstName.ToUpper()) { FontSize = 34 });
        if (!string.IsNullOrWhiteSpace(LastName))
            nameText.Inlines.Add(new Run($" {LastName.ToUpper()}") { FontSize = 34 });
        AddToGrid(cardGrid, nameText, 115);

        var statsGrid = new UniformGrid {
            Rows = 1, Columns = 7, Width = 320, Height = 80,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 40)
        };
        foreach (var (letter, val) in new[] { ("S", S), ("P", P), ("E", E), ("C", C), ("I", I), ("A", A), ("L", L) })
            statsGrid.Children.Add(CreateStatCircle(letter, val));
        AddToGrid(cardGrid, statsGrid, 120);

        AddToGrid(cardGrid, new Rectangle {
            IsHitTestVisible = false,
            Fill = new RadialGradientBrush(Colors.Transparent, Color.FromArgb(45, 0, 0, 0))
                { RadiusX = 1.1, RadiusY = 1.1 }
        }, 123);

        Rectangle? holoOverlay = null;
        LinearGradientBrush? holoBrush = null;

        if (isHolo) {
            holoBrush = new LinearGradientBrush {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                MappingMode = BrushMappingMode.RelativeToBoundingBox,
                GradientStops = new GradientStopCollection {
                    new GradientStop(Color.FromArgb(0, 255, 0, 128), 0.00),
                    new GradientStop(Color.FromArgb(25, 255, 0, 255), 0.16),
                    new GradientStop(Color.FromArgb(25, 0, 128, 255), 0.32),
                    new GradientStop(Color.FromArgb(25, 0, 255, 200), 0.48),
                    new GradientStop(Color.FromArgb(25, 255, 255, 0), 0.64),
                    new GradientStop(Color.FromArgb(25, 255, 100, 0), 0.80),
                    new GradientStop(Color.FromArgb(0, 255, 0, 128), 1.00)
                }
            };
            holoOverlay = new Rectangle { IsHitTestVisible = false, Opacity = 0, Fill = holoBrush };
            AddToGrid(cardGrid, holoOverlay, 122);
        }

        cardGrid.MouseMove += (s, e) => {
            var pos = e.GetPosition(cardGrid);
            var xp = pos.X / 400.0 - 0.5;
            var yp = pos.Y / 600.0 - 0.5;
            skewX.AngleX = -yp * 8;
            skewY.AngleY = xp * 8;

            if (!isHolo || holoOverlay == null || holoBrush == null) return;

            holoOverlay.Opacity = 0.55 + Math.Abs(xp) * 0.45 + Math.Abs(yp) * 0.2;
            holoBrush.StartPoint = new Point(Math.Clamp(xp + 0.1, 0, 1), Math.Clamp(yp + 0.1, 0, 1));
            holoBrush.EndPoint = new Point(Math.Clamp(xp + 0.9, 0, 1), Math.Clamp(yp + 0.9, 0, 1));

            var hue = xp * 0.18;
            var stops = holoBrush.GradientStops;
            stops[1].Color = ShiftHue(Color.FromArgb(25, 255, 0, 255), hue);
            stops[2].Color = ShiftHue(Color.FromArgb(25, 0, 128, 255), hue);
            stops[3].Color = ShiftHue(Color.FromArgb(25, 0, 255, 200), hue);
            stops[4].Color = ShiftHue(Color.FromArgb(25, 255, 255, 0), hue);
            stops[5].Color = ShiftHue(Color.FromArgb(25, 255, 100, 0), hue);
        };

        cardGrid.MouseLeave += (s, e) => {
            skewX.AngleX = 0;
            skewY.AngleY = 0;
            if (holoOverlay != null) holoOverlay.Opacity = 0;
        };

        return new Viewbox { Child = cardGrid, Stretch = Stretch.Uniform };
    }

    public UIElement CreateCharacterVisual() {
        var imageSource = LoadImage($"dwellers/{Texture}");
        var container = new Grid {
            Width = 200, Height = 200,
            RenderTransformOrigin = new Point(0.5, 0.5)
        };

        var outlineColor = CurrentState switch {
            DwellerState.Selected => Colors.White,
            DwellerState.Enemy => Color.FromRgb(211, 0, 24),
            _ => Color.FromRgb(0, 182, 255)
        };

        foreach (var angle in OutlineAngles)
            container.Children.Add(new Image {
                Source = imageSource,
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Effect = new DropShadowEffect
                    { Color = outlineColor, BlurRadius = 0, ShadowDepth = 3, Direction = angle, Opacity = 1 }
            });

        container.Children.Add(new Image {
            Source = imageSource,
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        return new Viewbox { Child = container, Stretch = Stretch.Uniform };
    }

    // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
    // [ - Helpers - ]

    private BitmapImage LoadImage(string path) {
        return new BitmapImage(new Uri($"{AssetBase}/{path}", UriKind.Absolute));
    }

    private void AddCardImage(Grid grid, string path, int zIndex) {
        AddToGrid(grid, new Image { Source = LoadImage(path) }, zIndex);
    }

    private static void AddToGrid(Grid grid, UIElement el, int zIndex) {
        Panel.SetZIndex(el, zIndex);
        grid.Children.Add(el);
    }

    private UIElement CreateStatCircle(string letter, int value) {
        var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

        stack.Children.Add(new Border {
            Width = 42, Height = 42,
            Background = StatBg,
            CornerRadius = new CornerRadius(21),
            Child = new TextBlock {
                Text = letter, FontSize = 28, FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily(new Uri(FontBase), "./#bebas neue")
            }
        });

        stack.Children.Add(new Border {
            Width = 28, Height = 28,
            Background = StatBg,
            CornerRadius = new CornerRadius(14),
            Margin = new Thickness(0, 10, 0, 0),
            Child = new TextBlock {
                Text = value.ToString(), FontSize = 16, FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        });

        return stack;
    }

    private static Color ShiftHue(Color color, double shift) {
        double r = color.R / 255.0, g = color.G / 255.0, b = color.B / 255.0;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        if (delta == 0) return color;

        double h;
        if (max == r) h = (g - b) / delta % 6;
        else if (max == g) h = (b - r) / delta + 2;
        else h = (r - g) / delta + 4;

        h = (h / 6.0 + shift + 1.0) % 1.0;
        double s = delta / max, v = max;

        var hi = (int)(h * 6);
        var f = h * 6 - hi;
        double p = v * (1 - s), q = v * (1 - f * s), t = v * (1 - (1 - f) * s);

        var (nr, ng, nb) = hi switch {
            0 => (v, t, p),
            1 => (q, v, p),
            2 => (p, v, t),
            3 => (p, q, v),
            4 => (t, p, v),
            _ => (v, p, q)
        };

        return Color.FromArgb(color.A, (byte)(nr * 255), (byte)(ng * 255), (byte)(nb * 255));
    }
}