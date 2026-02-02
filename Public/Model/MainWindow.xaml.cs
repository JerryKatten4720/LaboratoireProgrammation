using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LaboratoireProgrammation.Public.Model
{
    public partial class MainWindow : Window
    {
        // --- State & History ---
        private bool _isAdjustingSize = false;
        private readonly List<string> _commandHistory = new();
        private int _historyIndex = -1;

        // --- Aesthetic Config ---
        // This references your "TerminalGreen" from XAML roughly, but C# needs its own Color object.
        private readonly Color _terminalGreen = Color.FromRgb(51, 255, 51); // #33FF33
        
        public MainWindow()
        {
            InitializeComponent();
            
            // Auto-focus the input line when window loads
            Loaded += (s, e) => InputBox.Focus();
            
            // Initial Welcome Message
            AppendOutput("ROBCO INDUSTRIES (TM) TERMLINK PROTOCOL INITIALIZED.");
            AppendOutput("Type 'help' for command list.");
        }

        // =========================================================
        //                 INPUT HANDLING (The Brain)
        // =========================================================

        /// <summary>
        /// We use PreviewKeyDown because standard TextBox controls swallow arrow keys.
        /// This intercepts the key before the UI creates navigation events.
        /// </summary>
        private void InputBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 1. Handle Font Size Adjustment Mode
            if (_isAdjustingSize)
            {
                HandleFontSizeMode(e);
                return; 
            }

            // 2. Handle Standard Terminal Mode
            HandleStandardMode(e);
        }

        private void HandleFontSizeMode(KeyEventArgs e)
        {
            // Fix for some keyboards sending System/NumPad keys
            Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;

            switch (key)
            {
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

        private void HandleStandardMode(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ExecuteCommand();
                    e.Handled = true; // Prevents "ding" sound or new line
                    break;

                case Key.Up:
                    CycleHistory(-1); // Go back in history
                    e.Handled = true;
                    break;

                case Key.Down:
                    CycleHistory(1); // Go forward in history
                    e.Handled = true;
                    break;
            }
        }

        // =========================================================
        //                     LOGIC & COMMANDS
        // =========================================================

        private void ExecuteCommand()
        {
            string cmd = InputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(cmd)) return;

            // 1. Echo the command to the screen
            AppendOutput($"> {cmd}", Colors.GreenYellow); // Slightly brighter for user input

            // 2. Add to history
            _commandHistory.Add(cmd);
            _historyIndex = _commandHistory.Count;

            // 3. Run Logic
            ProcessCommand(cmd.ToLower());

            // 4. Cleanup
            InputBox.Clear();
        }

        private void ProcessCommand(string cmd)
        {
            switch (cmd)
            {
                case "help":
                    AppendOutput("COMMAND LIST:");
                    AppendOutput("  help    - Display this database");
                    AppendOutput("  clear   - Purge screen buffer");
                    AppendOutput("  font    - Adjust terminal readability");
                    AppendOutput("  exit    - Terminate session");
                    AppendOutput("  about   - System info");
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

                default:
                    AppendOutput($"[ERROR] COMMAND '{cmd}' UNRECOGNIZED.", Colors.Red);
                    break;
            }
        }

        // =========================================================
        //                      UI HELPERS
        // =========================================================

        private void AppendOutput(string text, Color? hexColor = null) {
            Color color = hexColor ?? _terminalGreen;
            
            Run run = new Run($"{DateTime.Now:HH:mm:ss} | {text}") {
                Foreground = new SolidColorBrush(color)
            };

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

            // Update Index
            _historyIndex += direction;

            // Clamp Index
            if (_historyIndex < 0) _historyIndex = 0;
            if (_historyIndex > _commandHistory.Count) _historyIndex = _commandHistory.Count;

            // Display
            if (_historyIndex < _commandHistory.Count) {
                InputBox.Text = _commandHistory[_historyIndex];
                InputBox.CaretIndex = InputBox.Text.Length; // Move cursor to end
            } else { InputBox.Clear(); }
        }

        // =========================================================
        //                    WINDOW CHROME
        // =========================================================

        private void MinimizeProgram(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void MaximizeProgram(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        private void CloseProgram(object sender, RoutedEventArgs e) => Close();
        
    }
}