using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.Views.Exercices;

public partial class Exo3 : UserControl {
    public Exo3() {
        InitializeComponent();
        initFolder();
        AddDummyFiles();
    }

    private void initFolder() {
        var exeDir = AppDomain.CurrentDomain.BaseDirectory;
        if (!Directory.Exists(exeDir + "\\Exo3")) Directory.CreateDirectory(exeDir + "\\Program Files");
        
        var binDir = AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..\\..\\";
        if (!Directory.Exists(binDir + "Exo3")) Directory.CreateDirectory(binDir + "Program Files");
        
        if (!Directory.Exists(binDir + "Program Files\\Exercice - 3")) Directory.CreateDirectory(binDir + "Program Files\\Exercice - 3");
    }
    
    private void AddDummyFiles() {
        var container = new StackPanel();

        for (int i = 0; i < 20; i++) {
            var button = new Button { 
                Content = $"File {i}", 
                Style = (Style)FindResource("FileButtonStyle") 
            };
            FileContainer.Children.Add(button);
        }
    }

    private void NewFileButton_Click(object sender, RoutedEventArgs e) {
        throw new NotImplementedException();
    }
}