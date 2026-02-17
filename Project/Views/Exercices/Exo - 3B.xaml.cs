using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LaboratoireProgrammation.Project.Helpers;
using LaboratoireProgrammation.Project.ViewModels.Menu;

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
        if (!Exo3.SelectedFilePath.Contains("Exercice-3") || 
            string.IsNullOrWhiteSpace(NameTextBox.Text) || 
            QualityComboBox.SelectedItem == null) return;
        
        if (!float.TryParse(SalaryTextBox.Text, out float salary)) return;

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

public class Human {
    public string Name { get; set; }
    public string Name2 { get; set; }
    public string Quality { get; set; }
    public float Salary { get; set; }
    public int Id { get; set; }

    public Human(string name, string name2, string quality, float salary) {
        Name = name;
        Name2 = name2;
        Quality = quality;
        Salary = salary;
        Id = Guid.NewGuid().GetHashCode();
    }

    public static String Serialize(Human human) {
        var json = new {
            Name = human.Name,
            Name2 = human.Name2,
            Quality = human.Quality,
            Salary = human.Salary,
            Id = human.Id
        };
        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented);
        return jsonString;
    }

    public static Human? Deserialize(String json) {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<Human>(json);
    }
}