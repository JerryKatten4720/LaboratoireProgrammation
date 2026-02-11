using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using LaboratoireProgrammation.Project.Helpers;
using LaboratoireProgrammation.Project.Services;

namespace LaboratoireProgrammation.Project.ViewModels.Menu;

public partial class MainWindow : Window {
    private static readonly FontFamily TerminalFont =
        new(new Uri("pack://application:,,,/LaboratoireProgrammation;component/"), "./Assets/fonts/#overseer");

    private static readonly FontFamily ConsoleFont = new("Consolas");
    private readonly List<string> _commandHistory = new();

    public bool _speedLoad = true;
    public bool _babymode = false;
    public bool _isAnimating = false;

    private readonly Color _terminalGreen = Color.FromRgb(51, 255, 51);
    private int _historyIndex = -1;

    public bool IsAdjustingSize;

    public bool _memfyMode;


    public MainWindow() {
        InitializeComponent();
        
        TerminalDisplay.Init(OutputBox, InputBox);
        SizeHelper.setFullscreen(this);
        AnimateLoader();

        MainInit();

        BlankSpace();
        
        //ThirdExo();
    }

    public void MainInit() {
        OutputBox.Visibility = Visibility.Visible;
        InputBox.Visibility = Visibility.Visible;
        Exo1.Visibility = Visibility.Collapsed;
        Exo1B.Visibility = Visibility.Collapsed;
        Exo2.Visibility = Visibility.Collapsed;
        Labo1B.Visibility = Visibility.Collapsed;

        if (_babymode && !_isAnimating) { BabyMode.Visibility = Visibility.Visible; } else { BabyMode.Visibility = Visibility.Collapsed; }
        
        Loaded += (s, e) => InputBox.Focus();

        OutputBox.Document.Blocks.Clear();
        TerminalDisplay.SeparationLine();

        BabyModeInit();
    }

    private void BabyModeInit() {
        if (!_babymode) {
            VisualMode.Content = "Mode Visuel [ ❌ ]";
            OutputBox.Visibility = Visibility.Visible;
            InputBox.Visibility = Visibility.Visible;
            return;
        }

        VisualMode.Content = "Mode Visuel [ ✔ ]";
        OutputBox.Visibility = Visibility.Collapsed;
        InputBox.Visibility = Visibility.Collapsed;
        TerminalDisplay.Init(OutputBox, InputBox);
    }

    private void InputBox_PreviewKeyDown(object sender, KeyEventArgs e) {
        if (IsAdjustingSize) {
            HandleFontSizeMode(e);
            return;
        }

        HandleStandardMode(e);
    }

    private void HandleFontSizeMode(KeyEventArgs e) {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        switch (key) {
            case Key.Up:
                ModifyFontSize(1);
                e.Handled = true;
                break;

            case Key.Down:
                ModifyFontSize(-1);
                e.Handled = true;
                break;

            case Key.Enter:
                IsAdjustingSize = false;
                AppendOutput($"[CONFIG] FONT SIZE SAVED: {OutputBox.FontSize}");
                InputBox.Clear();
                e.Handled = true;
                break;
        }
    }

    private void HandleStandardMode(KeyEventArgs e) {
        switch (e.Key) {
            case Key.Enter:
                _ = ExecuteCommand();
                e.Handled = true;
                break;

            case Key.Up:
                CycleHistory(-1);
                e.Handled = true;
                break;

            case Key.Down:
                CycleHistory(1);
                e.Handled = true;
                break;
        }
    }

    private async Task ExecuteCommand() {
        var cmd = InputBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(cmd)) return;

        AppendOutputOnSameLine($"[User] >>> {cmd}", ColorHelper.UserInput);

        _commandHistory.Add(cmd);
        _historyIndex = _commandHistory.Count;

        if (_memfyMode) {
            InputBox.Clear();
            InputBox.IsHitTestVisible = false;
            InputBox.IsReadOnly = true;
            var memfyAi = new MemfyAI();
            var fullResponse = "";

            BlankSpace();
            BlankSpace();
            AppendOutputOnSameLine("[MemfyAI] : ", ColorHelper.Text);

            await foreach (var chunk in memfyAi.AskQuestionAsync(cmd)) {
                fullResponse += chunk;
                var replace = chunk.Replace("[$exit$token$]", "").Replace("$exit$token$", "");
                AppendOutputOnSameLine(replace, ColorHelper.Text);
            }

            if (fullResponse.ToLower().Contains("$exit$token$")) {
                BlankSpace();
                AppendOutput("• [MemfyAI] has left the conversation", ColorHelper.Warning);
                BlankSpace();
                _memfyMode = false;
            }

            InputBox.IsHitTestVisible = true;
            InputBox.IsReadOnly = false;
            InputBox.Focus();
            return;
        }

        CommandsProcessor.ProcessCommand(cmd.ToLower(), this);
        InputBox.Clear();
        BlankSpace();
    }
    
    public void AppendOutput(string text, Color? hexColor = null) {
        var color = hexColor ?? _terminalGreen;

        var run = new Run($"{DateTime.Now:HH:mm:ss} | {text}") {
            Foreground = new SolidColorBrush(color)
        };

        var para = new Paragraph(run);
        OutputBox.Document.Blocks.Add(para);
        OutputBox.ScrollToEnd();
    }

    private void AppendOutputOnSameLine(string text, Color? hexColor = null) {
        
        var color = hexColor ?? _terminalGreen;
        var run = new Run(text) { Foreground = new SolidColorBrush(color) };
        var para = new Paragraph();
        
        para.Inlines.Add(run);
        if (OutputBox.Document.Blocks.Count > 0) {
            var lastPara = OutputBox.Document.Blocks.LastBlock as Paragraph;
            if (lastPara != null) lastPara.Inlines.Add(run);
        } else {
            OutputBox.Document.Blocks.Add(para);
        }

        OutputBox.ScrollToEnd();
    }

    public void BlankSpace() {
        var run = new Run("");
        var para = new Paragraph(run);
        OutputBox.Document.Blocks.Add(para);
        OutputBox.ScrollToEnd();
    }

    private void ModifyFontSize(int delta) {
        var newSize = Math.Max(OutputBox.FontSize + delta, 8);
        OutputBox.FontSize = newSize;
        InputBox.FontSize = newSize;
    }

    private void CycleHistory(int direction) {
        if (_commandHistory.Count == 0) return;

        _historyIndex += direction;

        if (_historyIndex < 0) _historyIndex = 0;
        if (_historyIndex > _commandHistory.Count) _historyIndex = _commandHistory.Count;

        if (_historyIndex < _commandHistory.Count) {
            InputBox.Text = _commandHistory[_historyIndex];
            InputBox.CaretIndex = InputBox.Text.Length; // Move cursor to end
        }
        else {
            InputBox.Clear();
        }
    }

    // Helpers
    
    private void MinimizeProgram(object sender, RoutedEventArgs e) {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeProgram(object sender, RoutedEventArgs e) {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseProgram(object sender, RoutedEventArgs e) {
        Close();
    }

    private async void AnimateLoader() {
        _isAnimating = true;
        OutputBox.Visibility = Visibility.Collapsed;
        TopText.Visibility = Visibility.Collapsed;
        BabyMode.Visibility = Visibility.Collapsed;
            
        double width = ActualWidth;
        double fontMult = width >= 1600 ? 1.0 : (width >= 1200 ? 0.8 : 0.6);
            
        var inspirationBlock = TerminalDisplay.CreateTitleBlock("{ - Programmation <-> Laboratoire - }", 76 * fontMult, ColorHelper.FancyTextBrush, 100);
        TerminalOutputPanel.Children.Add(inspirationBlock);
        await Task.Delay(1000);
            
        if (_speedLoad) await Task.Delay(200);
        else await Task.Delay(1000);

        var authorBlock = TerminalDisplay.CreateTitleBlock("\nanto.cldl", 350 * fontMult, Brushes.White, - 200);
        TerminalOutputPanel.Children.Add(authorBlock);
        await Task.Delay(1000);
            
        if (_speedLoad) await Task.Delay(200);
        else await Task.Delay(800);
            
        TerminalOutputPanel.Children.Clear();
        OutputBox.Visibility = Visibility.Visible;
        TopText.Visibility = Visibility.Visible;
        if (_babymode) BabyMode.Visibility = Visibility.Visible;
        _isAnimating = false;
    }
    

    // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private void BackToMenu(object sender, RoutedEventArgs e) { MainInit(); }

    private void BabyModeSwitch(object sender, RoutedEventArgs e) {
        if (_isAnimating) return;
        
        if (_babymode) _babymode = false;
        else _babymode = true;
        
        BabyMode.Visibility = _babymode ? Visibility.Visible : Visibility.Collapsed;
        VisualMode.Content = _babymode ? "Mode Visuel [ ✅ ]" : "Mode Visuel [ ❌ ]";
    }
}