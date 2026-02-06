using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LaboratoireProgrammation.Public.Visual_Utils;
using LaboratoireProgrammation.Utils;
using MySqlConnector;
using WPFCursorTest;

namespace LaboratoireProgrammation.Public.Model.Laboratoires.Laboratoire___1;

public partial class ConnectControl : UserControl {
    private int TrialsLeft = 4;
    private bool isTrying = false;
    public ConnectControl() {
        InitializeComponent();
    }
    
    private async void ConnectButton_Click(object sender, RoutedEventArgs e) {
        if (isTrying) return;
        isTrying = true;
        
        pbStatus.Visibility = Visibility.Visible;
        
        String server = ServerInput.Text;
        String database = DatabaseInput.Text;
        String user = UserInput.Text;
        String password = PasswordInput.Password;
        
        var parentWindow = WindowUtils.getParentWindow(this);

        if (parentWindow != null) {
            Image cursorImage = new Image();
            cursorImage.LayoutTransform = new ScaleTransform(6, 6);
            cursorImage.Source = new BitmapImage(new Uri("pack://application:,,,/LaboratoireProgrammation;component/Assets/images/hourglass.png", UriKind.Absolute));
            parentWindow.Cursor = CursorHelper.CreateCursor(cursorImage, 40, 40);
        }

        await Task.Delay(1800);
        
        var db = new SqlUtils(() => new MySqlConnection($"Server={server};Database={database};User={user};Password={password}"));
        
        try { db.AssertStatus(); }
        catch (Exception ex) {
            pbStatus.Visibility = Visibility.Hidden;
            Label.Content = "[ERROR] > Oh non! La connexion à la base de données a échoué";
            Label.Visibility = Visibility.Visible;
            RemainingTrials.Visibility = Visibility.Visible;
            RemainingTrialsBar.Visibility = Visibility.Visible;
            TrialsLeft--;
            RemainingTrials.Content = "Essais restants (" + TrialsLeft + ")";
            RemainingTrialsBar.Value = TrialsLeft * 25;
            
            ServerInput.Text = "";
            DatabaseInput.Text = "";
            UserInput.Text = "";
            PasswordInput.Password = "";
            
            if (TrialsLeft == 0) LockTrials();
            
            isTrying = false;
            if (parentWindow != null) parentWindow.Cursor = null;
            
            return;
        }
        
        isTrying = false;    
        if (parentWindow != null) parentWindow.Cursor = null;
        Label.Content = "[SUCCESS] > Connexion à la base de données effectuée avec succès !";
        Label.Visibility = Visibility.Visible;
        pbStatus.Visibility = Visibility.Hidden;
        ConfirmConnection();
        
    }

    private void LockTrials() {
        Label.Visibility = Visibility.Visible;
        Label.Content = "[ERROR] <!> Trop de tentatives ! Vous avez été bloqué(e) ! <!>";
        pbStatus.Visibility = Visibility.Hidden;
        RemainingTrials.Visibility = Visibility.Hidden;
        RemainingTrialsBar.Visibility = Visibility.Hidden;
        ConnectButton.Visibility = Visibility.Hidden;
        
        ConnectButton.Foreground = ColorUtils.CriticalBrush;
        Label.Foreground = ColorUtils.CriticalBrush;
        Label.BorderBrush = ColorUtils.CriticalBrush;
        ServerLabel.Foreground = ColorUtils.CriticalBrush;
        DatabaseLabel.Foreground = ColorUtils.CriticalBrush;
        UserLabel.Foreground = ColorUtils.CriticalBrush;
        PasswordLabel.Foreground = ColorUtils.CriticalBrush;
        ServerInput.BorderBrush = ColorUtils.CriticalBrush;
        DatabaseInput.BorderBrush = ColorUtils.CriticalBrush;
        UserInput.BorderBrush = ColorUtils.CriticalBrush;
        PasswordInput.BorderBrush = ColorUtils.CriticalBrush;
        TitleLabel.Foreground = ColorUtils.CriticalBrush;
    }
    
    private void ConfirmConnection() {
        pbStatus.Visibility = Visibility.Hidden;
        RemainingTrials.Visibility = Visibility.Hidden;
        RemainingTrialsBar.Visibility = Visibility.Hidden;
        ConnectButton.Visibility = Visibility.Hidden;
        
        ConnectButton.Foreground = ColorUtils.SuccessBrush;
        Label.Foreground = ColorUtils.SuccessBrush;
        Label.BorderBrush = ColorUtils.SuccessBrush;
        ServerLabel.Foreground = ColorUtils.SuccessBrush;
        DatabaseLabel.Foreground = ColorUtils.SuccessBrush;
        UserLabel.Foreground = ColorUtils.SuccessBrush;
        PasswordLabel.Foreground = ColorUtils.SuccessBrush;
        ServerInput.BorderBrush = ColorUtils.SuccessBrush;
        DatabaseInput.BorderBrush = ColorUtils.SuccessBrush;
        UserInput.BorderBrush = ColorUtils.SuccessBrush;
        PasswordInput.BorderBrush = ColorUtils.SuccessBrush;
        TitleLabel.Foreground = ColorUtils.SuccessBrush;
    }

}