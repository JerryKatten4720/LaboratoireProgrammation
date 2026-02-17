using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LaboratoireProgrammation.Project.Helpers;
using LaboratoireProgrammation.Project.Services;
using LaboratoireProgrammation.Project.ViewModels.Menu;
using Image = System.Windows.Controls.Image;

namespace LaboratoireProgrammation.Project.Models.Exercices;

public class Exo4 {
    [DllImport("User32.dll")]
    private static extern bool SetCursorPos(int X, int Y);
    private static bool isExitMoved = false;
    private static int touchCount = 0;

    private static double BurnIn;
    private static double NoiseScale;
    private static double NoiseIntensity;
    private static double Brightness;
    private static double Contrast;
    private static double PhosphorDecay;
    private static double BloomStrength;
    private static double InterferenceSpeed;
    private static double DistortionStrength;
    
    private static bool locked = false;

    private static int panicCount = 0;
    
    public static void InitializeVirus(MainWindow win) {
        RestartAsAdmin();

        BurnIn = win.VaultShader.BurnInIntensity;
        NoiseScale = win.VaultShader.NoiseScale;
        NoiseIntensity = win.VaultShader.NoiseIntensity;
        Brightness = win.VaultShader.Brightness;
        Contrast = win.VaultShader.Contrast;
        PhosphorDecay = win.VaultShader.PhosphorDecay;
        BloomStrength = win.VaultShader.BloomStrength;
        InterferenceSpeed = win.VaultShader.InterferenceSpeed;
        DistortionStrength = win.VaultShader.DistortionStrength;
        
        win.WinBtnClose.MouseEnter += (sender, e) => {
            if (sender is Button button) {
                button.SetValue(Panel.ZIndexProperty, 101);
                button.IsCancel = true;
                
                if (!isExitMoved) button.Margin = new Thickness(button.Margin.Left, button.Margin.Top, button.Margin.Right + 60, button.Margin.Bottom);
                else button.Margin = new Thickness(button.Margin.Left, button.Margin.Top, button.Margin.Right - 60, button.Margin.Bottom);
                
                isExitMoved = !isExitMoved;
                
                win.VaultShader.BurnInIntensity += 0.1;
                win.VaultShader.NoiseScale += 0.02;
                win.VaultShader.NoiseIntensity += 0.02;
                win.VaultShader.Brightness -= 0.05;
                win.VaultShader.Contrast += 0.05;
                win.VaultShader.PhosphorDecay += 0.1;
                win.VaultShader.BloomStrength += 0.1;
                win.VaultShader.InterferenceSpeed += 0.1;
                win.VaultShader.DistortionStrength += 0.02;
                
                touchCount++;
                
                switch (touchCount) {
                    case 1:
                        Panic(win);
                        break;
                    case 3:
                        CrazyWindow(win);
                        break;
                    case 5:
                        Panic(win);
                        break;
            
                }
            }
        };
        
        win.WinBtnMinimize.MouseEnter += (sender, e) => {
            if (sender is Button button) {
                button.Visibility = Visibility.Hidden;
            }
        };
        
        win.WinBtnMaximize.MouseEnter += (sender, e) => {
            if (sender is Button button) {
                button.Visibility = Visibility.Hidden;
            }
        };
        
        win.Closing += (sender, e) => {
            if (Keyboard.IsKeyDown(Key.LeftAlt) && Keyboard.IsKeyDown(Key.F4)) {
                e.Cancel = true;

                if (locked) {
                    var width = win.ActualWidth;
                    var fontMult = width >= 1600 ? 1.0 : width >= 1200 ? 0.8 : 0.6;
                    
                    panicCount++;
                    
                    win.TerminalOutputPanel.Children.Clear();
                    var inspirationBlock = TerminalDisplay.CreateTitleBlock("Tentative de fuite : " + panicCount, 100 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#ffffff")), 200);
                    win.TerminalOutputPanel.Children.Add(inspirationBlock);

                    if (panicCount == 9) {
                        win.TerminalOutputPanel.Children.Clear();
                        var WARN = TerminalDisplay.CreateTitleBlock("JE FERAI PAS ÇA SI J'ÉTAIS TOI...", 70 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#ffffff")), 200);
                        win.TerminalOutputPanel.Children.Add(WARN);
                    }

                    if (panicCount >= 10) {

                        PrintAllSystem32Files(win);
                        
                        try {
                            var WARN = TerminalDisplay.CreateTitleBlock("BANG", 180 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#ffffff")), 400);
                            win.TerminalOutputPanel.Children.Add(WARN);
                            /*ProcessStartInfo psi = new ProcessStartInfo();
                            psi.FileName = "shutdown.exe";
                            psi.Arguments = "/r /fw /t 0"; 
                            psi.CreateNoWindow = true;
                            psi.UseShellExecute = false;
                            psi.Verb = "runas"; 

                            Process.Start(psi);*/
                        }
                        catch (System.ComponentModel.Win32Exception) {
                            Console.WriteLine("Administrator privileges required.");
                        }
                    }
                    
                }
                
            }
        };
        
        
        Task.Run(() => {
            while (true) {
                Thread.Sleep(100);
                Application.Current.Dispatcher.Invoke(() => {
                    if (locked) {
                        win.WindowState = WindowState.Maximized;
                        SetCursorPos(new Random().Next(), new Random().Next());
                    }
                });
            }
        });
    }

    private static async void PrintAllSystem32Files(MainWindow win) {
        string path = @"C:\Windows\System32";
        
        var files = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly);
        int fileCount = 0;
        
        win.OutputBox.Foreground = Brushes.White;
        win.OutputBox.FontWeight = FontWeights.SemiBold;
        win.OutputBox.Visibility = Visibility.Visible;
        win.TerminalOutputPanel.Visibility = Visibility.Hidden;

        foreach (var file in files) {
            TerminalDisplay.AppendOutput("Suppression : " + file + " ...");
            fileCount++;
            if (fileCount % 45 == 0) win.OutputBox.Document.Blocks.Clear();
            if (fileCount > 600) break;

        }

        win.OutputBox.Visibility = Visibility.Collapsed;
        await Task.Delay(1000);
    }
    
    private static async void FirstTouch(MainWindow win) {
        win.WinBtnClose.Visibility = Visibility.Hidden;
        
        Image image = new Image();
        image.Source = new BitmapImage(new Uri("pack://application:,,,/LaboratoireProgrammation;component/Assets/Images/fallout4.png", UriKind.Absolute));
        image.Width = 500;
        image.HorizontalAlignment = HorizontalAlignment.Center;
        image.VerticalAlignment = VerticalAlignment.Center;
        
        for (int i = 0; i < 25; i++) {
            
            if (i == 5){ win.BethesdaLogo.Visibility = Visibility.Visible; }
            if (i == 7) win.BethesdaLogo.Visibility = Visibility.Hidden;
            
            if (i == 10) win.Exo1.Visibility = Visibility.Visible;
            if (i == 12) win.Exo1.Visibility = Visibility.Hidden;


            if (i == 20) {
                win.TerminalOutputPanel.Children.Add(image);
            } else if (i == 24) { win.TerminalOutputPanel.Children.Remove(image); }
            
            win.Background = new SolidColorBrush(ColorHelper.HexToColor("#291b1b"));
            await Task.Delay(10);
            win.Background = new SolidColorBrush(ColorHelper.HexToColor("#732727"));
            await Task.Delay(10);
            win.Background = new SolidColorBrush(ColorHelper.HexToColor("#422441"));
            await Task.Delay(10);
            win.Background = new SolidColorBrush(ColorHelper.HexToColor("#5c0200"));
            await Task.Delay(10);
            win.Background = new SolidColorBrush(ColorHelper.HexToColor("#121212"));
            await Task.Delay(10);
            win.Background = new SolidColorBrush(ColorHelper.HexToColor("#291b1b"));
        }
        
        win.WinBtnClose.Visibility = Visibility.Visible;
    }
    
    private static async void CrazyWindow(MainWindow win) {
        win.WinBtnClose.Visibility = Visibility.Hidden;

        var width = win.ActualWidth;
        var fontMult = width >= 1600 ? 1.0 : width >= 1200 ? 0.8 : 0.6;
        
        var inspirationBlock = TerminalDisplay.CreateTitleBlock("TU T'EN VAS ?", 150 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#ffffff")), 200);
        win.TerminalOutputPanel.Children.Add(inspirationBlock);
        
        await Task.Delay(2000);
        win.WindowState = WindowState.Normal;

        for (int i = 0; i < 10; i++) {
            await Task.Delay(20);
            int random = new Random().Next(-80, 60);
            win.Left += (10 + random);
            win.Top -= random;
        }
        
        for (int i = 0; i < 10; i++) {
            await Task.Delay(20);
            int random = new Random().Next(-200, 200);
            win.Top -= (10 + random);
            win.Left += random;
        }
        
        await Task.Delay(200);
        win.WindowState = WindowState.Maximized;
        
        await Task.Delay(200);
        win.OutputBox.Document.Blocks.Clear();
        win.OutputBox.Visibility = Visibility.Collapsed;
        win.TopText.Visibility = Visibility.Collapsed;
        win.InputBox.Visibility = Visibility.Collapsed;
        
        var glitchBlock1 = TerminalDisplay.CreateTitleBlock("ne pars pas !", 80 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#fcfe4d")), 0);
        var glitchBlock2 = TerminalDisplay.CreateTitleBlock("reste un peu avec nous !", 80 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#6190ff")), 0);
        var glitchBlock3 = TerminalDisplay.CreateTitleBlock("⦍𝚃⦐⦍𝚄⦐ ⦍𝚃⦐⦍'⦐⦍𝙴⦐⦍𝙽⦐ ⦍𝚅⦐⦍𝙰⦐⦍𝚂⦐ ⦍?⦐", 80 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#ffffff")), 0);
        var glitchBlock4 = TerminalDisplay.CreateTitleBlock("😱😱😱😱😱😱😱😱", 80 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#ff61ea")), 0);
        var glitchBlock5 = TerminalDisplay.CreateTitleBlock("KILL-9", 80 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#ffffff")), 0);

        win.TerminalOutputPanel.Children.Add(glitchBlock1); await Task.Delay(150);
        win.TerminalOutputPanel.Children.Add(glitchBlock2); await Task.Delay(150);
        win.TerminalOutputPanel.Children.Add(glitchBlock3); await Task.Delay(150);
        win.TerminalOutputPanel.Children.Add(glitchBlock4); await Task.Delay(150);
        win.TerminalOutputPanel.Children.Add(glitchBlock5); await Task.Delay(150);
        
        win.WindowState = WindowState.Normal;
        await Task.Delay(100);
        for (int i = 0; i < 10; i++) {
            await Task.Delay(20);
            if ( i% 3 == 0) win.Top += 100;
            else win.Left += 100;
        }
        win.WindowState = WindowState.Maximized;
        win.WinBtnClose.Visibility = Visibility.Visible;

    }
    
    private static async void Panic(MainWindow win) {
        
        locked = true;
        
        win.WindowState = WindowState.Minimized;
        
        win.Background = new SolidColorBrush(ColorHelper.HexToColor("#0000AA"));

        win.VaultShader.BurnInIntensity = BurnIn;
        win.VaultShader.NoiseScale = NoiseScale;
        win.VaultShader.NoiseIntensity = NoiseIntensity;
        win.VaultShader.Brightness = Brightness;
        win.VaultShader.Contrast = Contrast;
        win.VaultShader.PhosphorDecay = PhosphorDecay;
        win.VaultShader.BloomStrength = BloomStrength;
        win.VaultShader.InterferenceSpeed = InterferenceSpeed;
        win.VaultShader.DistortionStrength = DistortionStrength;
        
        win.WinBtnMaximize.Visibility = Visibility.Collapsed;
        win.WinBtnMinimize.Visibility = Visibility.Collapsed;
        win.WinBtnClose.Visibility = Visibility.Collapsed;
        
        win.TopText.Visibility = Visibility.Collapsed;
        win.InputBox.Visibility = Visibility.Collapsed;
        win.OutputBox.Visibility = Visibility.Collapsed;
        
        win.Exo1.Visibility = Visibility.Collapsed;
        win.Exo2.Visibility = Visibility.Collapsed;
        win.Exo3.Visibility = Visibility.Collapsed;
        
        win.Exo3B.Visibility = Visibility.Collapsed;
        win.Exo1B.Visibility = Visibility.Collapsed;
        
        win.VisualMode.Visibility = Visibility.Collapsed;
        win.MainMenuButton.Visibility = Visibility.Collapsed;
        
        win.InputBoxIndicator.Visibility = Visibility.Collapsed;
        
        win.TerminalOutputPanel.Children.Clear();
        
        await Task.Delay(100);

        var cursorImage = new Image();
        cursorImage.LayoutTransform = new ScaleTransform(6, 6);
        cursorImage.Source =
            new BitmapImage(new Uri(
                "pack://application:,,,/LaboratoireProgrammation;component/Assets/images/empty.png",
                UriKind.Absolute));
        win.Cursor = CursorHelper.CreateCursor(cursorImage, 0, 0);
        
        win.WindowStyle = WindowStyle.None;
        win.ResizeMode = ResizeMode.NoResize;
        
        win.WindowState = WindowState.Maximized;
    }
    private static bool IsRunAsAdmin() {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
    
    public static void RestartAsAdmin() {
        if (!IsRunAsAdmin()) {
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.UseShellExecute = true;
            proc.WorkingDirectory = Environment.CurrentDirectory;
            proc.FileName = Process.GetCurrentProcess().MainModule?.FileName;
        
            proc.Verb = "runas";

            try {
                Process.Start(proc);
                Environment.Exit(0);
            }
            catch (Exception) {
                Console.WriteLine("L'élévation a été refusée.");
            }
        }
    }
}