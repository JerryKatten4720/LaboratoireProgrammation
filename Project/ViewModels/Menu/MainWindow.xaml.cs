using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using HandyControl.Tools.Extension;
using LaboratoireProgrammation.Project.Helpers;
using LaboratoireProgrammation.Project.Models.Menu;
using LaboratoireProgrammation.Project.ModernHospital;
using LaboratoireProgrammation.Project.ModernOverseerWars;
using LaboratoireProgrammation.Project.ModernOverseerWars.Views;
using LaboratoireProgrammation.Project.Services;

namespace LaboratoireProgrammation.Project.ViewModels.Menu;

public partial class MainWindow : Window {
    private readonly ConsoleBehavior _behavior = new();
    private readonly Color _terminalGreen = Color.FromRgb(51, 255, 51);
    public bool IsAnimating;

    public MainWindow() {
        InitializeComponent();

        SetupModelEvents();

        TerminalDisplay.Init(OutputBox, InputBox);
        SizeHelper.setFullscreen(this);

        AnimateLoader();
    }

    private void SetupModelEvents() {
        _behavior.OnOutputRequest = (txt, clr) => TerminalDisplay.AppendOutput(txt, clr);
        _behavior.OnOutputSameLineRequest = (txt, clr) => TerminalDisplay.AppendOutputOnSameLine(txt, clr);
        _behavior.OnClearInputRequest = () => InputBox.Clear();
    }

    public void MainInit() {
        OutputBox.Visibility = Visibility.Visible;
        InputBox.Visibility = Visibility.Visible;

        Exo1.Visibility = Visibility.Collapsed;
        Exo1B.Visibility = Visibility.Collapsed;
        Exo2.Visibility = Visibility.Collapsed;
        Exo3.Visibility = Visibility.Collapsed;
        Exo3B.Visibility = Visibility.Collapsed;
        Exo5.Visibility = Visibility.Collapsed;
        Exo6.Visibility = Visibility.Collapsed;
        Exo7.Visibility = Visibility.Collapsed;
        Exo8.Visibility = Visibility.Collapsed;
        Exo9.Visibility = Visibility.Collapsed;
        Exo10.Visibility = Visibility.Collapsed;
        Labo1B.Visibility = Visibility.Collapsed;
        ESP8266.Visibility = Visibility.Collapsed;
        FlashMaster.Visibility = Visibility.Collapsed;
        MemfyAgreement.Visibility = Visibility.Collapsed;
        OverseerWarsMenu.Visibility = Visibility.Collapsed;
        BackgroundRectangle.Visibility = Visibility.Collapsed;
        BethesdaLogo.Visibility = Visibility.Collapsed;
        BabyMode.Visibility = Visibility.Collapsed;

        UpdateBabyModeUi();

        Loaded += (s, e) => InputBox.Focus();
        OutputBox.Document.Blocks.Clear();
        TerminalOutputPanel.Children.Clear();
        TerminalDisplay.SeparationLine();
        

        bool doshow = false;

        if (!doshow) {
            Window hospital = new HospitalWindow();
            hospital.Show();
            Close();
        }
        
        if (!doshow) return;
        var menu = new StartMenuWindow();
        if (menu.ShowDialog() == true) {
            var game = new OverseerWarsWindow(menu.Profile1, menu.Profile2);
            game.Show();
            Close();
        }
        

        
    }

    private void UpdateBabyModeUi() {
        if (!_behavior.GetBabyMode()) {
            VisualMode.Content = "Mode Visuel [ ❌ ]";
            OutputBox.Visibility = Visibility.Visible;
            InputBox.Visibility = Visibility.Visible;
            BabyMode.Visibility = Visibility.Collapsed;
        }
        else {
            VisualMode.Content = "Mode Visuel [ ✔ ]";
            OutputBox.Visibility = Visibility.Collapsed;
            InputBox.Visibility = Visibility.Collapsed;
            if (!IsAnimating) BabyMode.Visibility = Visibility.Visible;
            TerminalDisplay.Init(OutputBox, InputBox);
        }
    }

    private void InputBox_PreviewKeyDown(object sender, KeyEventArgs e) {
        if (_behavior.GetIsAdjustingSize()) {
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
                _behavior.SetBabyMode(false);
                TerminalDisplay.AppendOutput($"[CONFIG] FONT SIZE SAVED: {OutputBox.FontSize}");
                InputBox.Clear();
                e.Handled = true;
                break;
        }
    }

    private async void HandleStandardMode(KeyEventArgs e) {
        switch (e.Key) {
            case Key.Enter:
                await _behavior.ProcessInput(InputBox.Text, this);
                e.Handled = true;
                break;
            case Key.Up:
                UpdateInputFromHistory(-1);
                e.Handled = true;
                break;
            case Key.Down:
                UpdateInputFromHistory(1);
                e.Handled = true;
                break;
        }
    }

    private void UpdateInputFromHistory(int direction) {
        var historyText = _behavior.GetHistory(direction);
        if (historyText != null) {
            InputBox.Text = historyText;
            InputBox.CaretIndex = InputBox.Text.Length;
        }
    }

    // --- UI Rendering Helpers ---

    public void BlankSpace() {
        OutputBox.Document.Blocks.Add(new Paragraph(new Run("")));
        OutputBox.ScrollToEnd();
    }

    private void ModifyFontSize(int delta) {
        var newSize = Math.Max(OutputBox.FontSize + delta, 8);
        OutputBox.FontSize = newSize;
        InputBox.FontSize = newSize;
    }

    // --- Window Control ---

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
        IsAnimating = true;

        await MenuLoaders.Run_Main(this);

        MainInit();
        BlankSpace();

        IsAnimating = false;
    }

    public void BackToMenu(object sender, RoutedEventArgs e) {
        MainInit();
    }

    private void BabyModeSwitch(object sender, RoutedEventArgs e) {
        if (IsAnimating) return;
        _behavior.ToggleBabyMode();
        UpdateBabyModeUi();
    }
}