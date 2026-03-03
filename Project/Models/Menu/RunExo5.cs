using System.Windows;
using System.Windows.Media;
using LaboratoireProgrammation.Project.Helpers;
using LaboratoireProgrammation.Project.Services;
using LaboratoireProgrammation.Project.ViewModels.Menu;

namespace LaboratoireProgrammation.Project.Models.Menu;

public class RunExo5 {

    public static async void Run(MainWindow win) {
        win.OutputBox.Document.Blocks.Clear();

        win.OutputBox.Visibility = Visibility.Collapsed;
        win.TopText.Visibility = Visibility.Collapsed;
        win.InputBox.Visibility = Visibility.Collapsed;
        win.InputBoxIndicator.Visibility = Visibility.Hidden;

        var width = win.ActualWidth;
        var fontMult = width >= 1600 ? 1.0 : width >= 1200 ? 0.8 : 0.6;

        var inspirationBlock = TerminalDisplay.CreateTitleBlock("Exercice - 5", 76 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("2C9E32")));
        win.TerminalOutputPanel.Children.Add(inspirationBlock);

        var descriptionBlock = TerminalDisplay.CreateTitleBlock("Travail du >>> (17.02.26)", 50 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#92B394")));
        win.TerminalOutputPanel.Children.Add(descriptionBlock);

        if (ConsoleBehavior.SpeedLoad) await Task.Delay(0);
        else await Task.Delay(2000);

        var authorBlock = TerminalDisplay.CreateTitleBlock("anto.cldl", 200 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#D3F2D3")));
        win.TerminalOutputPanel.Children.Add(authorBlock);

        if (ConsoleBehavior.SpeedLoad) await Task.Delay(0);
        else await Task.Delay(2000);

        win.TerminalOutputPanel.Children.Clear();
        win.Exo5.Visibility = Visibility.Visible;

    }
    
}