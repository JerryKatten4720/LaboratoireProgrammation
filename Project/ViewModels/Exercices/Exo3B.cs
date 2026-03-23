using System.IO;
using System.Windows;
using System.Windows.Controls;
using LaboratoireProgrammation.Project.ViewModels.Menu;
using LaboratoireProgrammation.Project.Views.Exercices;

namespace LaboratoireProgrammation.Project.ViewModels.Exercices;

public partial class Exo3B : UserControl {
    
    public Exo3B() {
        InitializeComponent();
        InitializeComboBox();
    }

    private void InitializeComboBox() {
        QualityComboBox.Items.Add("Employé Vault-Tec");
        QualityComboBox.Items.Add("Employé Robco");
        QualityComboBox.Items.Add("Employé Nuka-Cola");
        QualityComboBox.Items.Add("Employé General Atomics");
    }


    private void ConfirmButton_Click(object sender, RoutedEventArgs e) { 
        if (!Exo3.SelectedFilePath.Contains("Exercice-3") || 
            string.IsNullOrWhiteSpace(NameTextBox.Text) || 
            QualityComboBox.SelectedItem == null) return;
        
        if (!float.TryParse((string?)SalaryTextBox.Text, out float salary)) return;

        string file = Exo3.SelectedFilePath;
        Human human = new Human(
            NameTextBox.Text, 
            NameTextBox2.Text, 
            QualityComboBox.SelectedItem.ToString()!, 
            salary
        );
        File.AppendAllText(file, Environment.NewLine + Human.Serialize(human));

        MainWindow win = (MainWindow)Application.Current.MainWindow;
        if (win != null) {
            Visibility = Visibility.Collapsed;
        
            win.Exo3.Opacity = 1.0;
            Panel.SetZIndex(win.Exo3, 1);
            
            _ = win.Exo3.Refresh(); 
        }
        
        NameTextBox.Text = "";
        NameTextBox2.Text = "";
        SalaryTextBox.Text = "";
        QualityComboBox.SelectedIndex = -1;
    }


    private void CancelButton_Click(object sender, RoutedEventArgs e) {
        MainWindow win = (MainWindow) Application.Current.MainWindow;
        if (win is null) return;
        
        QualityComboBox.SelectedIndex = -1;
        NameTextBox.Text = "";
        
        Visibility = Visibility.Collapsed;
        win.Exo3.Opacity = 1.0;
        Panel.SetZIndex(win.Exo3, 1);
    }
}