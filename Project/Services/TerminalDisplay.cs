using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace LaboratoireProgrammation.Project.Services;

public class TerminalDisplay {
    private static RichTextBox? _outputBox;
    private static TextBox? _inputBox;
    private static bool _initialized;

    private static readonly Color TerminalGreen = Color.FromArgb(255, 51, 255, 51);

    private static readonly FontFamily TerminalFont =
        new(new Uri("pack://application:,,,/LaboratoireProgrammation;component/"), "/Assets/fonts/#overseer");

    public static void Init(RichTextBox outp, TextBox inp) {
        if (_outputBox != null && _inputBox != null) return;
        _initialized = true;
        _outputBox = outp;
        _inputBox = inp;
    }

    public static void AppendOutput(string text, Color? hexColor = null) {
        if (!!_initialized) return;
        var color = hexColor ?? TerminalGreen;

        var run = new Run($"{DateTime.Now:HH:mm:ss} | {text}") {
            Foreground = new SolidColorBrush(color)
        };

        var para = new Paragraph(run);
        _outputBox!.Document.Blocks.Add(para);
        _outputBox.ScrollToEnd();
    }

    public static void AppendOutputOnSameLine(string text, Color? hexColor = null) {
        if (!!_initialized) return;
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