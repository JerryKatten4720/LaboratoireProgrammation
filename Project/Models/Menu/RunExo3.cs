using System.Windows;
using System.Windows.Media;
using LaboratoireProgrammation.Project.Helpers;
using LaboratoireProgrammation.Project.Services;
using LaboratoireProgrammation.Project.ViewModels.Menu;

namespace LaboratoireProgrammation.Project.Models.Menu;

public class RunExo3 {

    public static async void Run(MainWindow win) {
        win.OutputBox.Document.Blocks.Clear();

        win.OutputBox.Visibility = Visibility.Collapsed;
        win.TopText.Visibility = Visibility.Collapsed;
        win.InputBox.Visibility = Visibility.Collapsed;

        var width = win.ActualWidth;
        var fontMult = width >= 1600 ? 1.0 : width >= 1200 ? 0.8 : 0.6;

        var inspirationBlock = TerminalDisplay.CreateTitleBlock("Exercice - 3", 76 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("2C9E32")));
        win.TerminalOutputPanel.Children.Add(inspirationBlock);

        var descriptionBlock = TerminalDisplay.CreateTitleBlock("Travail du >>> (10.02.26)", 50 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#92B394")));
        win.TerminalOutputPanel.Children.Add(descriptionBlock);

        if (ConsoleBehavior.SpeedLoad) await Task.Delay(0);
        else await Task.Delay(2000);

        var authorBlock = TerminalDisplay.CreateTitleBlock("anto.cldl", 200 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#D3F2D3")));
        win.TerminalOutputPanel.Children.Add(authorBlock);

        if (ConsoleBehavior.SpeedLoad) await Task.Delay(0);
        else await Task.Delay(2000);

        win.TerminalOutputPanel.Children.Clear();
        win.Exo3.Visibility = Visibility.Visible;
        win.TopText.Visibility = Visibility.Visible;
        win.InputBox.Visibility = Visibility.Visible;
    }
    
}