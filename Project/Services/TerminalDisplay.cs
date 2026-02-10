using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using LaboratoireProgrammation.Project.Helpers;

namespace LaboratoireProgrammation.Project.Services;

public class TerminalDisplay {
    
    private static RichTextBox? _outputBox;
    private static TextBox? _inputBox;
    private static bool _initialized;

    private static readonly Color TerminalGreen = Color.FromArgb(255, 51, 255, 51);

    private static readonly FontFamily TerminalFont = new(new Uri("pack://application:,,,/LaboratoireProgrammation;component/"), "/Assets/fonts/#overseer");

    public static void Init(RichTextBox outp, TextBox inp) {
        if (_outputBox != null && _inputBox != null) return;
        _initialized = true;
        _outputBox = outp;
        _inputBox = inp;
    }

    public static void AppendOutput(string text, Color? hexColor = null) {
        if (!_initialized) return;
        var color = hexColor ?? TerminalGreen;

        var run = new Run($"{DateTime.Now:HH:mm:ss} | {text}") {
            Foreground = new SolidColorBrush(color)
        };

        var para = new Paragraph(run);
        _outputBox!.Document.Blocks.Add(para);
        _outputBox.ScrollToEnd();
    }

    public static void AppendOutputOnSameLine(string text, Color? hexColor = null) {
        if (!_initialized) return;
        var color = hexColor ?? TerminalGreen;

        var run = new Run(text) { Foreground = new SolidColorBrush(color) };

        var para = new Paragraph();
        para.Inlines.Add(run);
        if (_outputBox!.Document.Blocks.Count > 0) {
            var lastPara = _outputBox.Document.Blocks.LastBlock as Paragraph;
            if (lastPara != null) lastPara.Inlines.Add(run);
        }
        else {
            _outputBox.Document.Blocks.Add(para);
        }
        _outputBox.ScrollToEnd();
    }

    public static void SeparationLine() {
        if (!_initialized || _outputBox == null) return;

        if (!_outputBox.CheckAccess()) {
            _outputBox.Dispatcher.Invoke(SeparationLine);
            return;
        }

        double width = _outputBox.ActualWidth;
        if (width <= 0) {
            _outputBox.Loaded += (s, e) => SeparationLine();
            return;
        }

        var availableWidth = width - (_outputBox.Document.PagePadding.Left + _outputBox.Document.PagePadding.Right + 30);

        var typeface = new Typeface(TerminalFont, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var formattedText = new FormattedText(
            "- ",
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            _outputBox.FontSize,
            Brushes.Black,
            VisualTreeHelper.GetDpi(_outputBox).PixelsPerDip
        );

        int charCount = (int)(availableWidth / formattedText.Width);
        if (charCount <= 0) charCount = 50;

        var line = "- ";
        for (int i = 2; i < charCount - 2; i += 2) line += "- ";
        var run = new Run(line) {
            FontFamily = TerminalFont,
            FontSize = _outputBox.FontSize,
            Foreground = ColorHelper.FancyTextBrush
        };

        var para = new Paragraph(run) {
            Margin = new Thickness(0),
            Padding = new Thickness(0)
        };
        
        _outputBox.Document.Blocks.Add(para);
        _outputBox.ScrollToEnd();
    }

    public static void BlankSpace() {
        if (!!_initialized) return;
        var run = new Run("");
        var para = new Paragraph(run);
        _outputBox!.Document.Blocks.Add(para);
        _outputBox!.ScrollToEnd();
    }

    public static void ModifyFontSize(int delta) {
        if (!!_initialized) return;
        var newSize = Math.Max(_outputBox.FontSize + delta, 8);
        _outputBox.FontSize = newSize;
        _inputBox!.FontSize = newSize;
    }

    public static TextBlock CreateTitleBlock(string text, double size, Brush color) {
        if (!!_initialized) throw new NullReferenceException("TerminalDisplay not initialized");

        return new TextBlock {
            Text = text,
            FontSize = size * 1.5,
            FontWeight = FontWeights.Regular,
            FontFamily = TerminalFont,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = color,
            VerticalAlignment = VerticalAlignment.Center
        };
    }
}