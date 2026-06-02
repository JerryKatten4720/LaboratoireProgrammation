using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public class OutlinedTextBlock : FrameworkElement {
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    public string Text {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
        nameof(Fill), typeof(Brush), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush Fill {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize) {
        if (string.IsNullOrEmpty(Text)) return new Size(10, 10);
        var formattedText = new FormattedText(
            Text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Impact"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
            120,
            Brushes.Black,
            VisualTreeHelper.GetDpi(this).PixelsPerDip
        );
        return new Size(formattedText.Width, formattedText.Height);
    }

    protected override void OnRender(DrawingContext dc) {
        if (string.IsNullOrEmpty(Text)) return;
        var formattedText = new FormattedText(
            Text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Impact"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
            120,
            Brushes.Black,
            VisualTreeHelper.GetDpi(this).PixelsPerDip
        );

        var geometry = formattedText.BuildGeometry(new Point(0, 0));
        dc.DrawGeometry(Fill, new Pen(new SolidColorBrush(Color.FromArgb(51, 255, 255, 255)), 4), geometry);
    }
}
