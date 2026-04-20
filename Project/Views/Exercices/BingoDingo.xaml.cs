using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LaboratoireProgrammation.Project.Views.Exercices;

public partial class BingoDingo : UserControl {
    private readonly Random _rng = new();
    private int _hahaCount;
    private int _heuCount;
    private BingoCase _lastValidatedCase;
    private int _rainbowTicks;
    private DispatcherTimer _rainbowTimer;
    private int _totalScore;

    public BingoDingo() {
        InitializeComponent();
        InitializeGame();
    }


    private void InitializeGame() {
        var definitions = GetCaseDefinitions();
        var allCases = definitions.Select(CreateCaseLogic).OrderBy(x => _rng.Next()).ToList();

        PopulateGrid(allCases);
    }

    private BingoCase CreateCaseLogic(BingoCaseDefinition def) {
        var requiredHits = 1;

        if (def.Text == "\"Heu...\"") requiredHits = 15;
        else if (def.Text == "\"Haha...\"") requiredHits = 8;
        else if (def.Text == "Erwan qui dit un truc gay") requiredHits = 2;

        return new BingoCase {
            Text = def.Text,
            BasePoints = def.BasePoints,
            Consequence = def.Consequence,
            RequiredHits = requiredHits,
            GivenHits = 0
        };
    }

    private void PopulateGrid(List<BingoCase> allCases) {
        var row = 0;
        var col = 0;

        foreach (var bingoCase in allCases) {
            bingoCase.Row = row;
            bingoCase.Column = col;

            var cellBorder = BuildCellUI(bingoCase);
            bingoCase.UIElement = cellBorder;
            GridBingo.Children.Add(cellBorder);

            col++;
            if (col >= 6) {
                col = 0;
                row++;
            }
        }
    }

    private Border BuildCellUI(BingoCase bingoCase) {
        var border = new Border {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111113")),
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A2E")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(4),
            Cursor = Cursors.Hand,
            RenderTransform = new TranslateTransform(), // <-- Ajout ici
            Tag = bingoCase
        };

        var grid = new Grid();

        var textBlock = new TextBlock {
            Text = bingoCase.Text,
            FontFamily = new FontFamily("pack://application:,,,/Assets/fonts/#Doto"),
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB500")),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(10)
        };

        var overlay = new Rectangle {
            Opacity = 0.8,
            IsHitTestVisible = false
        };

        grid.Children.Add(textBlock);
        grid.Children.Add(overlay);
        border.Child = grid;

        bingoCase.OverlayElement = overlay;
        bingoCase.TextBlockElement = textBlock;

        border.MouseLeftButtonDown += Cell_Click;

        return border;
    }

    private void ShakeElement(UIElement element) {
        if (element.RenderTransform is TranslateTransform transform) {
            var shakeAnimation = new DoubleAnimationUsingKeyFrames {
                Duration = TimeSpan.FromMilliseconds(250)
            };

            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(4, KeyTime.FromPercent(0.2)));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(-4, KeyTime.FromPercent(0.4)));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(4, KeyTime.FromPercent(0.6)));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(-4, KeyTime.FromPercent(0.8)));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1.0)));

            transform.BeginAnimation(TranslateTransform.XProperty, shakeAnimation);
        }
    }


    private void Cell_Click(object sender, MouseButtonEventArgs e) {
        if (sender is Border border && border.Tag is BingoCase bingoCase) {
            if (bingoCase.IsValidated) return;

            bingoCase.GivenHits++;

            if (!bingoCase.IsValidated) {
                UpdateDestroyTexture(bingoCase);
                ShakeElement(border);
            }
            else {
                ValidateCase(bingoCase, border);
                ShakeElement(border);
            }
        }
    }


    private void UpdateDestroyTexture(BingoCase bingoCase) {
        var progress = (double)bingoCase.GivenHits / bingoCase.RequiredHits;
        var stage = (int)Math.Floor(progress * 10);
        if (stage > 9) stage = 9;

        try {
            var bitmap =
                new BitmapImage(new Uri($"pack://application:,,,/Assets/images/bingodingo/destroy_stage_{stage}.png"));
            bingoCase.OverlayElement.Fill = new ImageBrush(bitmap);
        }
        catch { }
    }

    private void ValidateCase(BingoCase bingoCase, Border border) {
        bingoCase.OverlayElement.Fill = null;
        border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A2E1A"));
        border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));
        bingoCase.TextBlockElement.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));

        var currentPoints = bingoCase.BasePoints;
        var currentConsequence = bingoCase.Consequence;

        CheckSpecialRules(bingoCase, ref currentPoints, ref currentConsequence);

        var multiplier = CalculateMultiplier(bingoCase);
        var finalScoreEarned = (int)(currentPoints * multiplier);

        UpdateScoreUI(bingoCase, currentConsequence, finalScoreEarned);

        _lastValidatedCase = bingoCase;

        if (_rng.NextDouble() <= 0.5) TriggerRandomEvent();
    }


    private void CheckSpecialRules(BingoCase bingoCase, ref int points, ref string consequence) {
        if (bingoCase.Text == "\"Heu...\"") {
            _heuCount++;
            if (_heuCount % 10 == 0) {
                points = 150;
                consequence = "Gorgée (Multiple de 10!)";
            }
        }
        else if (bingoCase.Text == "\"Haha...\"") {
            _hahaCount++;
            if (_hahaCount % 5 == 0) {
                points = 225;
                consequence = "Gorgée x2 (Multiple de 5!)";
            }
        }
    }

    private double CalculateMultiplier(BingoCase currentCase) {
        if (_lastValidatedCase == null) return 1.0;

        var rowDiff = Math.Abs(currentCase.Row - _lastValidatedCase.Row);
        var colDiff = Math.Abs(currentCase.Column - _lastValidatedCase.Column);

        if (rowDiff <= 1 && colDiff <= 1 && (rowDiff != 0 || colDiff != 0)) {
            MultiplierText.Visibility = Visibility.Visible;
            return 1.33;
        }

        MultiplierText.Visibility = Visibility.Hidden;
        return 1.0;
    }

    private void UpdateScoreUI(BingoCase bingoCase, string consequence, int scoreEarned) {
        _totalScore += scoreEarned;
        ScoreTextDisplay.Text = _totalScore.ToString();

        LastActionText.Text = $"VALIDÉ: {bingoCase.Text}\n>>> {consequence} (+{scoreEarned} pts)";
        LastActionText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));
    }


    private void TriggerRandomEvent() {
        var evt = _rng.Next(10);
        switch (evt) {
            case 0:
            case 3: AnimateRain("pack://application:,,,/Assets/images/bingodingo/bingodingo.png", 20, 5); break;
            case 1:
            case 2: AnimateRain("pack://application:,,,/Assets/images/bingodingo/caps.png", 100); break;
            case 4:
            case 5: AnimateTodd(); break;
            case 6:
            case 7:
            case 8: AnimateRain("pack://application:,,,/Assets/images/bingodingo/smirnoff.png", 100); break;
            case 9: StartRainbowGrid(); break;
        }
    }

    private void AnimateRain(string imagePath, int count, float sizeMult = 1.0f) {
        var canvasWidth = EventCanvas.ActualWidth > 0 ? EventCanvas.ActualWidth : 1200;
        var canvasHeight = EventCanvas.ActualHeight > 0 ? EventCanvas.ActualHeight : 800;

        for (var i = 0; i < count; i++) {
            var drop = new Image {
                Width = (int)(60 * sizeMult),
                Height = (int)(60 * sizeMult),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            try {
                drop.Source = new BitmapImage(new Uri(imagePath));
            }
            catch {
                continue;
            }

            EventCanvas.Children.Add(drop);

            var startX = _rng.NextDouble() * canvasWidth;
            Canvas.SetLeft(drop, startX);
            Canvas.SetTop(drop, -50 - _rng.Next(50));

            var transformGroup = new TransformGroup();
            var rotateTransform = new RotateTransform(0);
            transformGroup.Children.Add(rotateTransform);
            drop.RenderTransform = transformGroup;

            var durationSeconds = 1.0 + _rng.NextDouble();
            var fallAnimation = new DoubleAnimation {
                From = -50,
                To = canvasHeight + 50,
                Duration = TimeSpan.FromSeconds(durationSeconds)
            };

            var rotateAnimation = new DoubleAnimation {
                From = 0,
                To = _rng.NextDouble() > 0.5 ? 360 : -360,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                RepeatBehavior = RepeatBehavior.Forever
            };

            fallAnimation.Completed += (s, e) => EventCanvas.Children.Remove(drop);

            drop.BeginAnimation(Canvas.TopProperty, fallAnimation);
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
        }
    }

    private void AnimateTodd() {
        var canvasWidth = EventCanvas.ActualWidth > 0 ? EventCanvas.ActualWidth : 1200;

        var todd = new Image { Height = 250 };

        try {
            todd.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/images/bingodingo/todd.png"));
        }
        catch {
            return;
        }

        EventCanvas.Children.Add(todd);
        Canvas.SetBottom(todd, 10);
        Canvas.SetLeft(todd, canvasWidth + 100);

        var moveAnimation = new DoubleAnimation {
            From = canvasWidth + 100,
            To = -200,
            Duration = TimeSpan.FromSeconds(4)
        };

        moveAnimation.Completed += (s, e) => EventCanvas.Children.Remove(todd);
        todd.BeginAnimation(Canvas.LeftProperty, moveAnimation);
    }

    private void StartRainbowGrid() {
        if (_rainbowTimer != null && _rainbowTimer.IsEnabled) {
            _rainbowTicks = 0;
            return;
        }

        _rainbowTicks = 0;
        _rainbowTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _rainbowTimer.Tick += RainbowTimer_Tick;
        _rainbowTimer.Start();
    }

    private void RainbowTimer_Tick(object sender, EventArgs e) {
        _rainbowTicks++;

        if (_rainbowTicks > 150) {
            _rainbowTimer.Stop();
            GridBingo.Background = Brushes.Transparent;
            return;
        }

        var r = (byte)(Math.Sin(_rainbowTicks * 0.1) * 127 + 128);
        var g = (byte)(Math.Sin(_rainbowTicks * 0.1 + 2) * 127 + 128);
        var b = (byte)(Math.Sin(_rainbowTicks * 0.1 + 4) * 127 + 128);

        GridBingo.Background = new SolidColorBrush(Color.FromArgb(100, r, g, b));
    }


    private List<BingoCaseDefinition> GetCaseDefinitions() {
        return new List<BingoCaseDefinition> {
            new("Action Spécifique", 75, "C'était fun !")
        };
    }
}

public class BingoCaseDefinition {
    public BingoCaseDefinition(string text, int basePoints, string consequence) {
        Text = text;
        BasePoints = basePoints;
        Consequence = consequence;
    }

    public string Text { get; set; }
    public int BasePoints { get; set; }
    public string Consequence { get; set; }
}

public class BingoCase {
    public int Row { get; set; }
    public int Column { get; set; }
    public int RequiredHits { get; set; }
    public int GivenHits { get; set; }
    public string Text { get; set; }
    public int BasePoints { get; set; }
    public string Consequence { get; set; }
    public bool IsValidated => GivenHits >= RequiredHits;

    public Rectangle OverlayElement { get; set; }
    public TextBlock TextBlockElement { get; set; }
    public Border UIElement { get; set; }
}