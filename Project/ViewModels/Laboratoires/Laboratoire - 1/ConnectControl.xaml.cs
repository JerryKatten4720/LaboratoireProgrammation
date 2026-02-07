using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LaboratoireProgrammation.Project.Helpers;
using LaboratoireProgrammation.Project.Services;
using MySqlConnector;

namespace LaboratoireProgrammation.Project.ViewModels.Laboratoires.Laboratoire___1;

public partial class ConnectControl : UserControl {
    private bool isTrying;
    private int TrialsLeft = 4;

    public ConnectControl() {
        InitializeComponent();
    }

    private async void ConnectButton_Click(object sender, RoutedEventArgs e) {
        if (isTrying) return;
        isTrying = true;

        pbStatus.Visibility = Visibility.Visible;

        var server = ServerInput.Text;
        var database = DatabaseInput.Text;
        var user = UserInput.Text;
        var password = PasswordInput.Password;

        var parentWindow = WindowHelper.getParentWindow(this);

        if (parentWindow != null) {
            var cursorImage = new Image();
            cursorImage.LayoutTransform = new ScaleTransform(6, 6);
            cursorImage.Source =
                new BitmapImage(new Uri(
                    "pack://application:,,,/LaboratoireProgrammation;component/Assets/images/hourglass.png",
                    UriKind.Absolute));
            parentWindow.Cursor = CursorHelper.CreateCursor(cursorImage, 40, 40);
        }

        await Task.Delay(1800);

        var db = new SqlUtils(() =>
            new MySqlConnection($"Server={server};Database={database};User={user};Password={password}"));

        try {
            db.AssertStatus();
        }
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

        ConnectButton.Foreground = ColorHelper.CriticalBrush;
        Label.Foreground = ColorHelper.CriticalBrush;
        Label.BorderBrush = ColorHelper.CriticalBrush;
        ServerLabel.Foreground = ColorHelper.CriticalBrush;
        DatabaseLabel.Foreground = ColorHelper.CriticalBrush;
        UserLabel.Foreground = ColorHelper.CriticalBrush;
        PasswordLabel.Foreground = ColorHelper.CriticalBrush;
        ServerInput.BorderBrush = ColorHelper.CriticalBrush;
        DatabaseInput.BorderBrush = ColorHelper.CriticalBrush;
        UserInput.BorderBrush = ColorHelper.CriticalBrush;
        PasswordInput.BorderBrush = ColorHelper.CriticalBrush;
        TitleLabel.Foreground = ColorHelper.CriticalBrush;
    }

    private void ConfirmConnection() {
        pbStatus.Visibility = Visibility.Hidden;
        RemainingTrials.Visibility = Visibility.Hidden;
        RemainingTrialsBar.Visibility = Visibility.Hidden;
        ConnectButton.Visibility = Visibility.Hidden;

        ConnectButton.Foreground = ColorHelper.SuccessBrush;
        Label.Foreground = ColorHelper.SuccessBrush;
        Label.BorderBrush = ColorHelper.SuccessBrush;
        ServerLabel.Foreground = ColorHelper.SuccessBrush;
        DatabaseLabel.Foreground = ColorHelper.SuccessBrush;
        UserLabel.Foreground = ColorHelper.SuccessBrush;
        PasswordLabel.Foreground = ColorHelper.SuccessBrush;
        ServerInput.BorderBrush = ColorHelper.SuccessBrush;
        DatabaseInput.BorderBrush = ColorHelper.SuccessBrush;
        UserInput.BorderBrush = ColorHelper.SuccessBrush;
        PasswordInput.BorderBrush = ColorHelper.SuccessBrush;
        TitleLabel.Foreground = ColorHelper.SuccessBrush;
    }
}