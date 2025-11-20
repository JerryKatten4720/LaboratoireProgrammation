using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LaboratoireProgrammation.Games;
using ColorConverter = System.Windows.Media.ColorConverter;


namespace LaboratoireProgrammation;

public partial class MainWindow : Window {
    
    private const int GWL_STYLE = -16; private const int WS_SYSMENU = 0x80000; private const int WS_MINIMIZEBOX = 0x20000; private const int WS_MAXIMIZEBOX = 0x10000;
    

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    
    public MainWindow() {
        InitializeComponent();
        
        StateChanged += MainWindow_StateChanged;
        this.KeyDown += new KeyEventHandler(OnButtonKeyDown);
    }

    private void MainWindow_StateChanged(object sender, EventArgs e) {
        if (WindowState == WindowState.Maximized) MainGrid.Margin = new Thickness(8); else MainGrid.Margin = new Thickness(0);
    }
    
    protected override void OnSourceInitialized(EventArgs e) {
        base.OnSourceInitialized(e);
        
        var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        int style = GetWindowLong(hwnd, GWL_STYLE);

        style &= ~WS_MINIMIZEBOX;
        style &= ~WS_SYSMENU;

        SetWindowLong(hwnd, GWL_STYLE, style);
        
        GameList.LoadGames();
    }
    private bool isFullscreen = false;
    private void OnButtonKeyDown(object sender, KeyEventArgs e) {
        if (e.Key == Key.F11 && !isFullscreen) {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            Topmost = true;
            isFullscreen = true;
        } else if (e.Key == Key.F11 && isFullscreen) {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.SingleBorderWindow;
            Topmost = false;
            isFullscreen = false;
        }

        Game game = new Game("Title", "Desc", 3.5, x);

        if (e.Key == Key.F2) {
            if (!Game.IsDisplayed) {
                Game.LoadGameImage();
                return;
            }
            
            if (Game.IsDisplayed) {
                Game.UnloadGameImage();
                return;
            }
        }
        
        if (e.Key == Key.F3) {
            Game.UnloadGameImage();
        }
        
    }

    // - > Top Bar Buttons [START]
    private void CloseProgram(object sender, RoutedEventArgs e) { Close(); }
    private void MaximizeProgram(object sender, RoutedEventArgs e) { if (WindowState == WindowState.Maximized) { WindowState = WindowState.Normal; } else if (WindowState == WindowState.Normal) { WindowState = WindowState.Maximized; } }
    private void MinimizeProgram(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }
    // - > Top Bar Buttons [END]
    
    // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
     
    
    // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

    private void GamePrevious(object sender, RoutedEventArgs e) { Game.previousGame(); }
    private void GameNext(object sender, RoutedEventArgs e) { Game.nextGame(); }
    
    // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

    private const string _lightModeForeground = "#1a1a1a";
    private const string _lightModeBackground = "#F5F5F5";
    private const string _darkModeForeground = "#F5F5F5";
    private const string _darkModeBackground = "#1a1a1a";
    
    private string _currentForeground = _lightModeForeground;
    private string _currentBackground = _lightModeBackground;
    private void SetLightMode() {
        Background = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(_lightModeBackground));
        Foreground = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(_lightModeForeground));
    }
    private void SetDarkMode() {
        Background = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(_darkModeBackground));
        Foreground = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(_darkModeForeground));

    }
    
    public void ToggleMode(object sender, RoutedEventArgs e) {
        if (_currentBackground == _darkModeBackground && _currentForeground == _darkModeForeground) { SetLightMode(); }
        else { SetDarkMode(); }
    }
}