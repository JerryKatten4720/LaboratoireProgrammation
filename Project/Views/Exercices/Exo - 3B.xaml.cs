using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.Views.Exercices;

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
        throw new NotImplementedException();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) {
        QualityComboBox.SelectedIndex = -1;
        NameTextBox.Text = "";
    }
}