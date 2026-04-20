using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LaboratoireProgrammation.Project.Views.Exercices;

public class FindReplaceDialog : Window {
    private static readonly SolidColorBrush YellowBrush = new(Color.FromRgb(0xFF, 0x7D, 0x0F));
    private static readonly SolidColorBrush DarkBrush = new(Color.FromRgb(9, 9, 11));
    private static readonly SolidColorBrush PanelBrush = new(Color.FromRgb(17, 17, 19));
    private static readonly SolidColorBrush BorderBrush2 = new(Color.FromRgb(42, 42, 46));
    private static readonly FontFamily Mono = new("Consolas");

    private readonly RichTextBox _editor;
    private TextBox _findBox = null!;
    private TextBox _replaceBox = null!;
    private TextBlock _resultText = null!;

    public FindReplaceDialog(RichTextBox editor) {
        _editor = editor;
        Title = "Rechercher / Remplacer";
        Background = DarkBrush;
        Foreground = YellowBrush;
        Width = 420;
        Height = 220;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        FontFamily = Mono;
        WindowStyle = WindowStyle.ToolWindow;
        BuildUI();
    }

    private void BuildUI() {
        var grid = new Grid { Margin = new Thickness(16) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        _findBox = CreateTextBox();
        _replaceBox = CreateTextBox();
        _resultText = new TextBlock {
            Foreground = new SolidColorBrush(Colors.Gray), FontFamily = Mono, FontSize = 12,
            Margin = new Thickness(0, 4, 0, 0)
        };

        AddRow(grid, 0, "[ FIND ]", _findBox);
        AddRow(grid, 1, "[ REPLACE ]", _replaceBox);

        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
        btnPanel.Children.Add(CreateBtn("FIND NEXT", FindNext));
        btnPanel.Children.Add(CreateBtn("REPLACE", ReplaceOne));
        btnPanel.Children.Add(CreateBtn("REPLACE ALL", ReplaceAll));
        Grid.SetRow(btnPanel, 2);
        Grid.SetColumnSpan(btnPanel, 2);
        grid.Children.Add(btnPanel);

        Grid.SetRow(_resultText, 3);
        Grid.SetColumnSpan(_resultText, 2);
        grid.Children.Add(_resultText);

        Content = grid;
    }

    private TextBox CreateTextBox() {
        return new TextBox {
            Background = PanelBrush,
            Foreground = YellowBrush,
            CaretBrush = YellowBrush,
            BorderBrush = BorderBrush2,
            BorderThickness = new Thickness(1),
            FontFamily = Mono,
            FontSize = 13,
            Padding = new Thickness(4, 2, 4, 2),
            Margin = new Thickness(0, 4, 0, 4)
        };
    }

    private Button CreateBtn(string label, RoutedEventHandler click) {
        var btn = new Button {
            Content = $"[ {label} ]",
            Background = Brushes.Transparent,
            Foreground = YellowBrush,
            BorderBrush = BorderBrush2,
            BorderThickness = new Thickness(1),
            FontFamily = Mono,
            FontSize = 12,
            Margin = new Thickness(0, 0, 6, 0),
            Padding = new Thickness(6, 3, 6, 3),
            Cursor = Cursors.Hand
        };
        btn.Click += click;
        return btn;
    }

    private void AddRow(Grid grid, int row, string label, UIElement control) {
        var lbl = new TextBlock {
            Text = label, Foreground = YellowBrush, FontFamily = Mono, FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(lbl, row);
        Grid.SetColumn(lbl, 0);
        Grid.SetRow(control, row);
        Grid.SetColumn(control, 1);
        grid.Children.Add(lbl);
        grid.Children.Add(control);
    }

    private void FindNext(object sender, RoutedEventArgs e) {
        var term = _findBox.Text;
        if (string.IsNullOrEmpty(term)) return;
        var start = _editor.Selection.IsEmpty ? _editor.Document.ContentStart : _editor.Selection.End;
        var pos = FindText(start, term);
        if (pos != null) {
            _editor.Selection.Select(pos, pos.GetPositionAtOffset(term.Length, LogicalDirection.Forward));
            _editor.Focus();
            _resultText.Text = "Trouvé.";
        }
        else {
            _resultText.Text = "Non trouvé.";
        }
    }

    private void ReplaceOne(object sender, RoutedEventArgs e) {
        if (!_editor.Selection.IsEmpty && _editor.Selection.Text == _findBox.Text)
            _editor.Selection.Text = _replaceBox.Text;
        FindNext(sender, e);
    }

    private void ReplaceAll(object sender, RoutedEventArgs e) {
        var term = _findBox.Text;
        var repl = _replaceBox.Text;
        if (string.IsNullOrEmpty(term)) return;
        var count = 0;
        var pos = _editor.Document.ContentStart;
        TextPointer? found;
        while ((found = FindText(pos, term)) != null) {
            var end = found.GetPositionAtOffset(term.Length, LogicalDirection.Forward);
            if (end == null) break;
            var range = new TextRange(found, end);
            range.Text = repl;
            pos = range.End;
            count++;
        }

        _resultText.Text = $"{count} remplacement(s) effectué(s).";
    }

    private TextPointer? FindText(TextPointer start, string term) {
        var pos = start;
        while (pos != null) {
            if (pos.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text) {
                var text = pos.GetTextInRun(LogicalDirection.Forward);
                var idx = text.IndexOf(term, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0) return pos.GetPositionAtOffset(idx);
            }

            pos = pos.GetNextContextPosition(LogicalDirection.Forward);
        }

        return null;
    }
}