using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using LaboratoireProgrammation.Public.Controller.Misc;
using LaboratoireProgrammation.Public.Visual_Utils;
using LaboratoireProgrammation.Utils;

namespace LaboratoireProgrammation.Public.Model {
    public partial class MainWindow : Window {
        
        private bool _memfyMode = false;
        
        private readonly bool _speedLoad = true;
        private bool _babymode = false;
        
        private bool _isAdjustingSize = false;
        private readonly List<string> _commandHistory = new();
        private int _historyIndex = -1;
        
        private static readonly FontFamily TerminalFont = new FontFamily(new Uri("pack://application:,,,/LaboratoireProgrammation;component/"), "./Assets/fonts/#overseer");
        private static readonly FontFamily ConsoleFont = new FontFamily("Consolas");

        private readonly Color _terminalGreen = Color.FromRgb(51, 255, 51);

        
        public MainWindow() {
            InitializeComponent();
            
            SizeUtils.setFullscreen(this);
            
            MainInit();
            
            BlankSpace();
        }

        public void MainInit() {
            OutputBox.Visibility = Visibility.Visible;
            InputBox.Visibility = Visibility.Visible;
            BabyMode.Visibility = Visibility.Collapsed;
            Exo1.Visibility = Visibility.Collapsed;
            Exo1B.Visibility = Visibility.Collapsed;
            Exo2.Visibility = Visibility.Collapsed;
            Labo1B.Visibility = Visibility.Collapsed;
            
            Loaded += (s, e) => InputBox.Focus();
            InputBox.Focus();
            
            OutputBox.Document.Blocks.Clear();
            AppendOutput("@anto.cldl | Console | Programmation.Laboratoire");
            AppendOutput("Utilisez 'help' pour obtenir la liste des commandes.");
            BlankSpace();
            
            BabyModeInit();
        }

        private void BabyModeInit() {
            if (!_babymode) {
                VisualMode.Content = "Mode Visuel [ ❌ ]";
                OutputBox.Visibility = Visibility.Visible;
                InputBox.Visibility = Visibility.Visible;
                BabyMode.Visibility = Visibility.Collapsed;
                return;
            }
            
            VisualMode.Content = "Mode Visuel [ ✔ ]";
            OutputBox.Visibility = Visibility.Collapsed;
            InputBox.Visibility = Visibility.Collapsed;
            BabyMode.Visibility = Visibility.Visible;
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
            string cmd = InputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(cmd)) return;
            
            AppendOutputOnSameLine($"[User] >>> {cmd}", ColorUtils.UserInput);

            _commandHistory.Add(cmd);
            _historyIndex = _commandHistory.Count;
            
            if (_memfyMode) {
                InputBox.Clear();
                InputBox.IsHitTestVisible = false;
                InputBox.IsReadOnly = true;
                MemfyAI memfyAi = new MemfyAI();
                string fullResponse = "";

                BlankSpace(); BlankSpace();
                AppendOutputOnSameLine($"[MemfyAI] : ", ColorUtils.Text);

                await foreach (string chunk in memfyAi.AskQuestionAsync(cmd)) {
                    fullResponse += chunk; 
                    var replace = chunk.Replace("[$exit$token$]", "").Replace("$exit$token$", "");
                    AppendOutputOnSameLine(replace, ColorUtils.Text);
                }

                if (fullResponse.ToLower().Contains("$exit$token$")) {
                    BlankSpace();
                    AppendOutput($"• [MemfyAI] has left the conversation", ColorUtils.Warning);
                    BlankSpace();
                    _memfyMode = false;
                }
    
                InputBox.IsHitTestVisible = true;
                InputBox.IsReadOnly = false;
                InputBox.Focus();
                return;
            }
            
            ProcessCommand(cmd.ToLower());
            InputBox.Clear();
            BlankSpace(); BlankSpace();
        }

        private void ProcessCommand(string cmd) {
            switch (cmd) {
                case "help":
                    AppendOutput("COMMAND LIST:");
                    BlankSpace();
                    AppendOutput("  help    - Display this database");
                    AppendOutput("  clear   - Purge screen buffer");
                    AppendOutput("  font    - Adjust terminal readability");
                    AppendOutput("  exit    - Terminate session");
                    AppendOutput("  about   - System info");
                    BlankSpace();
                    AppendOutput("  exo1   - Launch Exo (1+B)");
                    AppendOutput("  exo2   - Launch Exo (2)");
                    AppendOutput("  lab1   - Launch Lab (1)");
                    break;
                
                case "info":
                    AppendOutput("ROBCO UNIFIED OPERATING SYSTEM v1.0");
                    AppendOutput("Running on WPF .NET Core.");
                    BlankSpace();
                    AppendOutput("• [exo.1+b] : UI Interactive : Contrôle de boutons et permutation d'images", Colors.Cyan);
                    AppendOutput("• [exo.2]   : Simulation de transfert de fichiers (Barres de progression)", Colors.Cyan);
                    AppendOutput("• [lab.1]   : Système de gestion de BDD (CRUD Complet) [EN DÉVELOPPEMENT]", Colors.Yellow);
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
                
                case "exo 1":
                case "exo1":
                    AppendOutput("[SYSTEM] DÉMARRAGE >>> EXO [1+B]", Colors.Yellow);
                    AppendOutput("...", Colors.Yellow);
                    FirstExo();
                    break;
                
                case "exo 2":
                case "exo2":
                    AppendOutput("[SYSTEM] DÉMARRAGE >>> EXO [2]", Colors.Yellow);
                    AppendOutput("...", Colors.Yellow);
                    SecondExo();
                    break;
                
                case "lab 1":
                case "lab1":
                    AppendOutput("[SYSTEM] DÉMARRAGE >>> LABORATOIRE [1]", Colors.Yellow);
                    AppendOutput("...", Colors.Yellow);
                    FirstLab();
                    break;
                
                case "memfy":
                case "memfyai":
                case "memfy ai":
                    MemfyAgreementLaunch();
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

        private void AppendOutputOnSameLine(string text, Color? hexColor = null) {
            Color color = hexColor ?? _terminalGreen;
            
            Run run = new Run(text) { Foreground = new SolidColorBrush(color) };

            Paragraph para = new Paragraph();
            para.Inlines.Add(run);
            if (OutputBox.Document.Blocks.Count > 0) {
                var lastPara = OutputBox.Document.Blocks.LastBlock as Paragraph;
                if (lastPara != null) { lastPara.Inlines.Add(run); }
            } else OutputBox.Document.Blocks.Add(para);
            
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

        public async void FirstExo() {
            OutputBox.Document.Blocks.Clear();
            
            OutputBox.Visibility = Visibility.Collapsed;
            TopText.Visibility = Visibility.Collapsed;
            InputBox.Visibility = Visibility.Collapsed;
            
            double width = ActualWidth;
            double fontMult = width >= 1600 ? 1.0 : (width >= 1200 ? 0.8 : 0.6);
            
            var inspirationBlock = CreateTitleBlock("Exercice - 1", 76 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("#3495eb")));
            TerminalOutputPanel.Children.Add(inspirationBlock);

            var descriptionBlock = CreateTitleBlock("Travail du >>> (02.02.26)", 50 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("#3495eb")));
            TerminalOutputPanel.Children.Add(descriptionBlock);
            
            if (_speedLoad) await Task.Delay(200);
            else await Task.Delay(2000);
            
            var authorBlock = CreateTitleBlock("anto.cldl", 200 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("#deefff")));
            TerminalOutputPanel.Children.Add(authorBlock);
            
            if (_speedLoad) await Task.Delay(200);
            else await Task.Delay(2000);
            
            TerminalOutputPanel.Children.Clear();
            Exo1.Visibility = Visibility.Visible;
            TopText.Visibility = Visibility.Visible;
            Exo1B.Visibility = Visibility.Visible;
            
            InputBox.Visibility = Visibility.Visible;
        }
        
        public async void SecondExo() {
            OutputBox.Document.Blocks.Clear();
            
            OutputBox.Visibility = Visibility.Collapsed;
            TopText.Visibility = Visibility.Collapsed;
            InputBox.Visibility = Visibility.Collapsed;
            
            double width = ActualWidth;
            double fontMult = width >= 1600 ? 1.0 : (width >= 1200 ? 0.8 : 0.6);
            
            var inspirationBlock = CreateTitleBlock("Exercice - 2", 76 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("a834eb")));
            TerminalOutputPanel.Children.Add(inspirationBlock);

            var descriptionBlock = CreateTitleBlock("Travail du >>> (03.02.26)", 50 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("#a834eb")));
            TerminalOutputPanel.Children.Add(descriptionBlock);
            
            if (_speedLoad) await Task.Delay(200);
            else await Task.Delay(2000);
            
            var authorBlock = CreateTitleBlock("anto.cldl", 200 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("#fedeff")));
            TerminalOutputPanel.Children.Add(authorBlock);
            
            if (_speedLoad) await Task.Delay(200);
            else await Task.Delay(2000);
            
            TerminalOutputPanel.Children.Clear();
            Exo2.Visibility = Visibility.Visible;
            TopText.Visibility = Visibility.Visible;
            InputBox.Visibility = Visibility.Visible;
        }
        
        public async void FirstLab() {
            OutputBox.Document.Blocks.Clear();

            double smokeDensity = VaultShader.SmokeDensity;
            double glitchIntensity = VaultShader.GlitchIntensity;
            double phosphorDecay = VaultShader.PhosphorDecay;
            double burnIn = VaultShader.BurnInIntensity;
            double constrast = VaultShader.Contrast;
            double vaultBrightness = VaultShader.Brightness;
            Color tint = VaultShader.TintColor;

            VaultShader.GlitchIntensity *= 5;
            VaultShader.SmokeDensity = 1.01;
            VaultShader.Brightness = 1.1;
            VaultShader.Contrast *= 1.04;
            VaultShader.PhosphorDecay *= 1.5;
            VaultShader.BurnInIntensity *= 2;
            
            VaultShader.TintColor = Color.FromArgb(5, 255, 0, 0);
            OutputBox.Visibility = Visibility.Collapsed;
            TopText.Visibility = Visibility.Collapsed;
            InputBox.Visibility = Visibility.Collapsed;
            
            double width = ActualWidth;
            double fontMult = width >= 1600 ? 1.0 : (width >= 1200 ? 0.8 : 0.6);
            
            var inspirationBlock = CreateTitleBlock("Laboratoire - 1", 76 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("#eb4634")));
            TerminalOutputPanel.Children.Add(inspirationBlock);

            var descriptionBlock = CreateTitleBlock("< ! > LAB :// { - 03.02.26 - } :\\ < ! >", 50 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("#eb4634")));
            TerminalOutputPanel.Children.Add(descriptionBlock);
            
            if (_speedLoad) await Task.Delay(200);
            else await Task.Delay(2000);
            
            var authorBlock = CreateTitleBlock("anto.cldl", 200 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("#fff1f0")));
            TerminalOutputPanel.Children.Add(authorBlock);
            
            if (_speedLoad) await Task.Delay(200);
            else await Task.Delay(2000);
            
            TerminalOutputPanel.Children.Clear();
            
            InputBox.Visibility = Visibility.Visible;
            TopText.Visibility = Visibility.Visible;

            VaultShader.GlitchIntensity = glitchIntensity;
            VaultShader.SmokeDensity = smokeDensity;
            VaultShader.Brightness = vaultBrightness;
            VaultShader.PhosphorDecay = phosphorDecay;
            VaultShader.BurnInIntensity = burnIn;
            VaultShader.Contrast = constrast;
            VaultShader.TintColor = tint;
            
            Labo1B.Visibility = Visibility.Visible;
        }
        
        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void MemfyAgreementLaunch() {
            OutputBox.Document.Blocks.Clear();

            OutputBox.Visibility = Visibility.Collapsed;
            TopText.Visibility = Visibility.Collapsed;
            InputBox.Visibility = Visibility.Collapsed;
            
            TerminalOutputPanel.Children.Clear();
            
            MemfyAgreement.Visibility = Visibility.Visible;
        }
        
        public async void MemfyAI() {
            OutputBox.Document.Blocks.Clear();

            OutputBox.Visibility = Visibility.Collapsed;
            TopText.Visibility = Visibility.Collapsed;
            InputBox.Visibility = Visibility.Collapsed;
            
            double width = ActualWidth;
            double fontMult = width >= 1600 ? 1.0 : (width >= 1200 ? 0.8 : 0.6);
            
            var inspirationBlock = CreateTitleBlock("[ - MemfyAI - ]", 76 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("#ff96fa")));
            TerminalOutputPanel.Children.Add(inspirationBlock);

            var descriptionBlock = CreateTitleBlock("< Disclaimer > Use Cautiously !", 50 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("#fcb3f9")));
            TerminalOutputPanel.Children.Add(descriptionBlock);
            
            if (_speedLoad) await Task.Delay(200);
            else await Task.Delay(2000);
            
            var authorBlock = CreateTitleBlock("anto.cldl", 200 * fontMult, new SolidColorBrush(ColorUtils.HexToColor("#ffffff")));
            TerminalOutputPanel.Children.Add(authorBlock);
            
            if (_speedLoad) await Task.Delay(200);
            else await Task.Delay(2000);
            
            TerminalOutputPanel.Children.Clear();
            
            InputBox.Visibility = Visibility.Visible;
            TopText.Visibility = Visibility.Visible;
            OutputBox.Visibility = Visibility.Visible;
            InputBox.Focus();

            _memfyMode = true;
        }

        private void BackToMenu(object sender, RoutedEventArgs e) {
            MainInit();

        }

        private void BabyModeSwitch(object sender, RoutedEventArgs e) {
            if (_babymode) _babymode = false;
            else _babymode = true;
            
            BabyModeInit();
        }
    }
}