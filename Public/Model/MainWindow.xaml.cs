using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using LaboratoireProgrammation.Public.Visual_Utils;
using LaboratoireProgrammation.Utils;

namespace LaboratoireProgrammation.Public.Model {
    public partial class MainWindow : Window {
        private bool speedLoad = false;
        
        private bool _isAdjustingSize = false;
        private readonly List<string> _commandHistory = new();
        private int _historyIndex = -1;
        
        private static readonly FontFamily TerminalFont = new FontFamily(new Uri("pack://application:,,,/LaboratoireProgrammation;component/"), "./Assets/fonts/#overseer");
        private static readonly FontFamily ConsoleFont = new FontFamily("Consolas");

        private readonly Color _terminalGreen = Color.FromRgb(51, 255, 51);

        
        public MainWindow() {
            InitializeComponent();
            SizeUtils.setFullscreen(this);
            
            animateLoader();
            
            Loaded += (s, e) => InputBox.Focus();
            
            AppendOutput("@anto.cldl | Console | Programmation.Laboratoire");
            AppendOutput("Utilisez 'help' pour obtenir la liste des commandes.");
            BlankSpace();
        }
        
        private void InputBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (_isAdjustingSize) { HandleFontSizeMode(e); return; }
            HandleStandardMode(e);
        }

        private void HandleFontSizeMode(KeyEventArgs e) {
            Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;

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
                    _isAdjustingSize = false;
                    AppendOutput($"[CONFIG] FONT SIZE SAVED: {OutputBox.FontSize}");
                    InputBox.Clear();
                    e.Handled = true;
                    break;
            }
        }

        private void HandleStandardMode(KeyEventArgs e) {
            switch (e.Key) {
                case Key.Enter:
                    ExecuteCommand();
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
        private void ExecuteCommand() {
            string cmd = InputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(cmd)) return;

            AppendOutput($"[User] >>> {cmd}", ColorUtils.UserInput);

            _commandHistory.Add(cmd);
            _historyIndex = _commandHistory.Count;

            ProcessCommand(cmd.ToLower());

            InputBox.Clear();
        }

        private void ProcessCommand(string cmd) {
            switch (cmd) {
                case "help":
                    AppendOutput("COMMAND LIST:");
                    AppendOutput("  help    - Display this database");
                    AppendOutput("  clear   - Purge screen buffer");
                    AppendOutput("  font    - Adjust terminal readability");
                    AppendOutput("  exit    - Terminate session");
                    AppendOutput("  about   - System info");
                    BlankSpace();
                    AppendOutput("  Lab1   - Launch Lab (1)");

                    break;

                case "clear":
                    OutputBox.Document.Blocks.Clear();
                    break;

                case "font":
                    _isAdjustingSize = true;
                    AppendOutput("[SYSTEM] FONT ADJUSTMENT MODE ENGAGED.", Colors.Yellow);
                    AppendOutput("Use [UP/DOWN] to scale text. Press [ENTER] to confirm.");
                    break;

                case "about":
                    AppendOutput("ROBCO UNIFIED OPERATING SYSTEM v1.0");
                    AppendOutput("Running on WPF .NET Core.");
                    break;

                case "exit":
                    Close();
                    break;
                
                case "lab1":
                    FirstLab();
                    break;

                default:
                    AppendOutput($"[ERROR] COMMAND '{cmd}' UNRECOGNIZED.", Colors.Red);
                    break;
            }
        }

        // UI.Helpers

        private void AppendOutput(string text, Color? hexColor = null) {
            Color color = hexColor ?? _terminalGreen;
            
            Run run = new Run($"{DateTime.Now:HH:mm:ss} | {text}") {
                Foreground = new SolidColorBrush(color)
            };

            Paragraph para = new Paragraph(run);
            OutputBox.Document.Blocks.Add(para);
            OutputBox.ScrollToEnd();
        }

        private void BlankSpace() {
            Run run = new Run("");
            Paragraph para = new Paragraph(run);
            OutputBox.Document.Blocks.Add(para);
            OutputBox.ScrollToEnd();
        }

        private void ModifyFontSize(int delta) {
            double newSize = Math.Max(OutputBox.FontSize + delta, 8);
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
            } else { InputBox.Clear(); }
        }
        
        // Animations

        private async void animateLoader() {
            OutputBox.Visibility = Visibility.Collapsed;
            TopText.Visibility = Visibility.Collapsed;
            
            double width = ActualWidth;
            double fontMult = width >= 1600 ? 1.0 : (width >= 1200 ? 0.8 : 0.6);
            
            var inspirationBlock = CreateTitleBlock("{ - Programmation <-> Laboratoire - }", 76 * fontMult, ColorUtils.FancyTextBrush);
            TerminalOutputPanel.Children.Add(inspirationBlock);
            
            if (speedLoad) await Task.Delay(200);
            else await Task.Delay(2000);

            var authorBlock = CreateTitleBlock("\nanto.cldl", 250 * fontMult, ColorUtils.FancyTextBrush);
            TerminalOutputPanel.Children.Add(authorBlock);
            
            if (speedLoad) await Task.Delay(200);
            else await Task.Delay(2000);
            
            TerminalOutputPanel.Children.Clear();
            OutputBox.Visibility = Visibility.Visible;
            TopText.Visibility = Visibility.Visible;
        }
        
        // Helpers
        private TextBlock CreateTitleBlock(string text, double size, Brush color) {
            return new TextBlock {
                Text = text,
                FontSize = size * 1.5,
                FontWeight = FontWeights.Regular,
                FontFamily = TerminalFont,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = color,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        // Window.Controls

        private void MinimizeProgram(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void MaximizeProgram(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        private void CloseProgram(object sender, RoutedEventArgs e) => Close();
        
        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public async void FirstLab() {
            OutputBox.Document.Blocks.Clear();
            
            OutputBox.Visibility = Visibility.Collapsed;
            TopText.Visibility = Visibility.Collapsed;
            
            double width = ActualWidth;
            double fontMult = width >= 1600 ? 1.0 : (width >= 1200 ? 0.8 : 0.6);
            
            var inspirationBlock = CreateTitleBlock("Laboratoire - 1", 76 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("#3495eb")));
            TerminalOutputPanel.Children.Add(inspirationBlock);
            
            if (speedLoad) await Task.Delay(200);
            else await Task.Delay(2000);

            var descriptionBlock = CreateTitleBlock("Travail du >>> (02.02.26)", 50 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("#3495eb")));
            TerminalOutputPanel.Children.Add(descriptionBlock);
            
            var authorBlock = CreateTitleBlock("anto.cldl", 200 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("#deefff")));
            TerminalOutputPanel.Children.Add(authorBlock);
            
            if (speedLoad) await Task.Delay(200);
            else await Task.Delay(2000);
            
            TerminalOutputPanel.Children.Clear();
            OutputBox.Visibility = Visibility.Visible;
            TopText.Visibility = Visibility.Visible;
        }
        
        
    }
}