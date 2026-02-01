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

    // - > Top Bar Buttons [START]
    private void CloseProgram(object sender, RoutedEventArgs e) { Close(); }
    private void MaximizeProgram(object sender, RoutedEventArgs e) { if (WindowState == WindowState.Maximized) { WindowState = WindowState.Normal; } else if (WindowState == WindowState.Normal) { WindowState = WindowState.Maximized; } }
    private void MinimizeProgram(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }
    // - > Top Bar Buttons [END]
    
}