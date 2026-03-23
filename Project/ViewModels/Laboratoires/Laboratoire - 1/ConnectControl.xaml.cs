using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LaboratoireProgrammation.Project.Helpers;
using LaboratoireProgrammation.Project.Services;
using MySqlConnector;
using Microsoft.Data.SqlClient;

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

        // Define our factory based on the ComboBox selection
        Func<IDbConnection> connectionFactory;

        if (DbTypeInput.SelectedIndex == 1) // SQL Server (MSSQL)
        {
            // Note: TrustServerCertificate is often required for modern MSSQL instances on local dev environments
            string msSqlConnString = $"Server={server};Database={database};User Id={user};Password={password};TrustServerCertificate=True;";
            connectionFactory = () => new SqlConnection(msSqlConnString);
        }
        else // MySQL (Default)
        {
            string mySqlConnString = $"Server={server};Database={database};User={user};Password={password};";
            connectionFactory = () => new MySqlConnection(mySqlConnString);
        }

        var db = new SqlUtils(connectionFactory);

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
        
        // At this point, you might want to store your 'db' instance in a singleton
        // or a global Dependency Injection container so the rest of your app uses the same DbType.
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
        DbTypeLabel.Foreground = ColorHelper.CriticalBrush;
        DbTypeInput.BorderBrush = ColorHelper.CriticalBrush;
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
        DbTypeLabel.Foreground = ColorHelper.SuccessBrush;
        DbTypeInput.BorderBrush = ColorHelper.SuccessBrush;
    }
}