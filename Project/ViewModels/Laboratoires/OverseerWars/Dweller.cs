using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Documents;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using FontFamily = System.Windows.Media.FontFamily;

namespace LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars;

public enum DwellerState { Selected, Ally, Enemy }
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

public class Dweller {
    public string FirstName;
    public string LastName;
    public int S, P, E, C, I, A, L;
    public int X, Y;
    public Team Team { get; set; }

    public Outfit Outfit { get; set; }
    public Weapon Weapon { get; set; }
    public Rarity Rarity { get; set; }
    public string Texture { get; set; }
    public DwellerState CurrentState { get; set; }

    public int IndividuelActionPoints { get; set; } = 3;

    public Dweller() { }
    
    public Dweller(string fn, string ln) {
        FirstName = fn;
        LastName = ln;
    }

    public void Move(int x, int y) {
        X = x;
        Y = y;
    }

    public UIElement CreateCardVisual() {
        string rarityFactor = Rarity.ToString().ToLower();
        bool isHolo = false;
        
        if (Rarity == Rarity.Epic || Rarity == Rarity.Legendary) {
            isHolo = true;
        }

        var cardGrid = new Grid {
            Width = 400,
            Height = 600,
            RenderTransformOrigin = new Point(0.5, 0.5)
        };

        var skewX = new SkewTransform();
        var skewY = new SkewTransform();
        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(skewX);
        transformGroup.Children.Add(skewY);
        cardGrid.RenderTransform = transformGroup;

        var cardBorder = new Image {
            Source = new BitmapImage(new Uri($"pack://application:,,,/LaboratoireProgrammation;component/Assets/images/overseerWars/cards/card_border_{rarityFactor}.png", UriKind.Absolute))
        };
        Panel.SetZIndex(cardBorder, 110);
        cardGrid.Children.Add(cardBorder);

        var cardColor = new Image {
            Source = new BitmapImage(new Uri($"pack://application:,,,/LaboratoireProgrammation;component/Assets/images/overseerWars/cards/card_color_{rarityFactor}.png", UriKind.Absolute))
        };
        Panel.SetZIndex(cardColor, 109);
        cardGrid.Children.Add(cardColor);

        var cardTop = new Image {
            Source = new BitmapImage(new Uri($"pack://application:,,,/LaboratoireProgrammation;component/Assets/images/overseerWars/cards/card_top_{rarityFactor}.png", UriKind.Absolute))
        };
        Panel.SetZIndex(cardTop, 108);
        cardGrid.Children.Add(cardTop);

        var cardCircleBorder = new Image {
            Source = new BitmapImage(new Uri("pack://application:,,,/LaboratoireProgrammation;component/Assets/images/overseerWars/cards/card_circleborder.png", UriKind.Absolute))
        };
        Panel.SetZIndex(cardCircleBorder, 107);
        cardGrid.Children.Add(cardCircleBorder);

        var cardCircle = new Image {
            Source = new BitmapImage(new Uri("pack://application:,,,/LaboratoireProgrammation;component/Assets/images/overseerWars/cards/card_circle.png", UriKind.Absolute))
        };
        Panel.SetZIndex(cardCircle, 105);
        cardGrid.Children.Add(cardCircle);

        var dwellerImage = new Image {
            Source = new BitmapImage(new Uri($"pack://application:,,,/LaboratoireProgrammation;component/Assets/images/overseerWars/dwellers/{Texture}", UriKind.Absolute)),
            Stretch = Stretch.None,
            LayoutTransform = new ScaleTransform(1.3, 1.3),
            Margin = new Thickness(15, 200, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Panel.SetZIndex(dwellerImage, 106);
        cardGrid.Children.Add(dwellerImage);

        var nameText = new TextBlock {
            FontFamily = new FontFamily(new Uri("pack://application:,,,/LaboratoireProgrammation;component/Assets/fonts/"), "./#overseer"),
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.LightGray,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 44, 0, 0),
            Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 6, ShadowDepth = 3, Opacity = 0.7 }
        };

        nameText.Inlines.Add(new Run(FirstName.ToUpper()) { FontSize = 34 });
        
        if (!string.IsNullOrWhiteSpace(LastName)) {
            nameText.Inlines.Add(new Run($" {LastName.ToUpper()}") { FontSize = 34 });
        }

        Panel.SetZIndex(nameText, 115);
        cardGrid.Children.Add(nameText);

        var statsContainer = new UniformGrid {
            Rows = 1, Columns = 7, Width = 320, Height = 80,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 40)
        };
        Panel.SetZIndex(statsContainer, 120);

        statsContainer.Children.Add(CreateStatCircle("S", S));
        statsContainer.Children.Add(CreateStatCircle("P", P));
        statsContainer.Children.Add(CreateStatCircle("E", E));
        statsContainer.Children.Add(CreateStatCircle("C", C));
        statsContainer.Children.Add(CreateStatCircle("I", I));
        statsContainer.Children.Add(CreateStatCircle("A", A));
        statsContainer.Children.Add(CreateStatCircle("L", L));
        cardGrid.Children.Add(statsContainer);

        var vignette = new Rectangle {
            IsHitTestVisible = false,
            Fill = new RadialGradientBrush(Colors.Transparent, Color.FromArgb(45, 0, 0, 0)) { RadiusX = 1.1, RadiusY = 1.1 }
        };
        Panel.SetZIndex(vignette, 123);
        cardGrid.Children.Add(vignette);

        Rectangle holoOverlay = null;
        LinearGradientBrush holoBrush = null;

        if (isHolo) {
            holoBrush = new LinearGradientBrush {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection {
                    new GradientStop(Color.FromArgb(0,   255,   0, 128), 0.00),
                    new GradientStop(Color.FromArgb(25,  255,   0, 255), 0.16),
                    new GradientStop(Color.FromArgb(25,    0, 128, 255), 0.32),
                    new GradientStop(Color.FromArgb(25,    0, 255, 200), 0.48),
                    new GradientStop(Color.FromArgb(25,  255, 255,   0), 0.64),
                    new GradientStop(Color.FromArgb(25,  255, 100,   0), 0.80),
                    new GradientStop(Color.FromArgb(0,   255,   0, 128), 1.00)
                },
                MappingMode = BrushMappingMode.RelativeToBoundingBox
            };

            holoOverlay = new Rectangle {
                IsHitTestVisible = false,
                Opacity = 0,
                Fill = holoBrush
            };
            Panel.SetZIndex(holoOverlay, 122);
            cardGrid.Children.Add(holoOverlay);
        }

        cardGrid.MouseMove += (s, e) => {
            var pos = e.GetPosition(cardGrid);
            double xPct = (pos.X / 400.0) - 0.5;
            double yPct = (pos.Y / 600.0) - 0.5;

            skewX.AngleX = -yPct * 8;
            skewY.AngleY = xPct * 8;

            if (isHolo) {
                if (holoOverlay != null) {
                    if (holoBrush != null) {
                        holoOverlay.Opacity = 0.55 + Math.Abs(xPct) * 0.45 + Math.Abs(yPct) * 0.2;
                        
                        double startX = Math.Clamp(xPct + 0.5 - 0.4, 0, 1);
                        double startY = Math.Clamp(yPct + 0.5 - 0.4, 0, 1);
                        holoBrush.StartPoint = new Point(startX, startY);
                        
                        double endX = Math.Clamp(xPct + 0.5 + 0.4, 0, 1);
                        double endY = Math.Clamp(yPct + 0.5 + 0.4, 0, 1);
                        holoBrush.EndPoint = new Point(endX, endY);

                        double hueShift = xPct * 0.18;
                        var stops = holoBrush.GradientStops;
                        stops[1].Color = ShiftHue(Color.FromArgb(25, 255,   0, 255), hueShift);
                        stops[2].Color = ShiftHue(Color.FromArgb(25,   0, 128, 255), hueShift);
                        stops[3].Color = ShiftHue(Color.FromArgb(25,   0, 255, 200), hueShift);
                        stops[4].Color = ShiftHue(Color.FromArgb(25, 255, 255,   0), hueShift);
                        stops[5].Color = ShiftHue(Color.FromArgb(25, 255, 100,   0), hueShift);
                    }
                }
            }
        };

        cardGrid.MouseLeave += (s, e) => {
            skewX.AngleX = 0;
            skewY.AngleY = 0;

            if (isHolo) {
                if (holoOverlay != null) {
                    holoOverlay.Opacity = 0;
                }
            }
        };

        return new Viewbox { Child = cardGrid, Stretch = Stretch.Uniform };
    }

    private static Color ShiftHue(Color color, double shift) {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        if (delta == 0) {
            return color;
        }

        double h = 0;
        
        if (max == r) {
            h = ((g - b) / delta) % 6;
        }
        else if (max == g) {
            h = (b - r) / delta + 2;
        }
        else {
            h = (r - g) / delta + 4;
        }

        h = ((h / 6.0) + shift + 1.0) % 1.0;
        double s = delta / max;
        double v = max;

        int hi = (int)(h * 6);
        double f = h * 6 - hi;
        double p = v * (1 - s);
        double q = v * (1 - f * s);
        double t = v * (1 - (1 - f) * s);

        double nr = 0;
        double ng = 0;
        double nb = 0;

        if (hi == 0) { 
            nr = v; 
            ng = t; 
            nb = p; 
        }
        else if (hi == 1) { 
            nr = q; 
            ng = v; 
            nb = p; 
        }
        else if (hi == 2) { 
            nr = p; 
            ng = v; 
            nb = t; 
        }
        else if (hi == 3) { 
            nr = p; 
            ng = q; 
            nb = v; 
        }
        else if (hi == 4) { 
            nr = t; 
            ng = p; 
            nb = v; 
        }
        else { 
            nr = v; 
            ng = p; 
            nb = q; 
        }

        byte finalR = (byte)(nr * 255);
        byte finalG = (byte)(ng * 255);
        byte finalB = (byte)(nb * 255);

        return Color.FromArgb(color.A, finalR, finalG, finalB);
    }

    public UIElement CreateCharacterVisual() {
        var container = new Grid {
            Width = 200,
            Height = 200,
            RenderTransformOrigin = new Point(0.5, 0.5)
        };
        
        var imageSource = new BitmapImage(new Uri($"pack://application:,,,/LaboratoireProgrammation;component/Assets/images/overseerWars/dwellers/{Texture}", UriKind.Absolute));

        if (CurrentState == DwellerState.Selected) {
            int[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };
            
            foreach (int angle in angles) {
                var outlineLayer = new Image {
                    Source = imageSource,
                    Stretch = Stretch.Uniform,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Effect = new DropShadowEffect {
                        Color = Colors.White,
                        BlurRadius = 0,
                        ShadowDepth = 3,
                        Direction = angle,
                        Opacity = 1
                    }
                };
                container.Children.Add(outlineLayer);
            }
        }
        
        if (CurrentState == DwellerState.Ally) {
            int[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };
            
            foreach (int angle in angles) {
                var outlineLayer = new Image {
                    Source = imageSource,
                    Stretch = Stretch.Uniform,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Effect = new DropShadowEffect {
                        Color = Color.FromRgb(0, 182, 255),
                        BlurRadius = 0,
                        ShadowDepth = 3  ,
                        Direction = angle,
                        Opacity = 1
                    }
                };
                container.Children.Add(outlineLayer);
            }
        }
        
        if (CurrentState == DwellerState.Enemy) {
            int[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };
            
            foreach (int angle in angles) {
                var outlineLayer = new Image {
                    Source = imageSource,
                    Stretch = Stretch.Uniform,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Effect = new DropShadowEffect {
                        Color = Color.FromRgb(211, 0, 24),
                        BlurRadius = 0,
                        ShadowDepth = 3,
                        Direction = angle,
                        Opacity = 1
                    }
                };
                container.Children.Add(outlineLayer);
            }
        }

        var dwellerImage = new Image {
            Source = imageSource,
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        container.Children.Add(dwellerImage);
        
        return new Viewbox { Child = container, Stretch = Stretch.Uniform };
    }

    private UIElement CreateStatCircle(string letter, int value) {
        var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

        var bgBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#4D000000"); 

        var border = new Border {
            Width = 42, 
            Height = 42,
            Background = bgBrush,
            CornerRadius = new CornerRadius(21),
            Child = new TextBlock {
                Text = letter, 
                FontSize = 28, 
                FontWeight = FontWeights.Bold, 
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center, 
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily(new Uri("pack://application:,,,/LaboratoireProgrammation;component/Assets/fonts/"), "./#bebas neue")
            }
        };

        var valBorder = new Border {
            Width = 28, 
            Height = 28,
            Background = bgBrush,
            CornerRadius = new CornerRadius(14), 
            Margin = new Thickness(0, 10, 0, 0),
            Child = new TextBlock {
                Text = value.ToString(), 
                FontSize = 16, 
                FontWeight = FontWeights.Bold, 
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center, 
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        stack.Children.Add(border);
        stack.Children.Add(valBorder);
        return stack;
    }
}

public class Outfit {
    public string Name { get; set; }
    public string Prefix { get; set; }
    public int BonusS, BonusP, BonusE, BonusC, BonusI, BonusA, BonusL;
}

public class Weapon {
    public string Name;
    public string Prefix;
    public int Damages;
    public double Timeout;
}