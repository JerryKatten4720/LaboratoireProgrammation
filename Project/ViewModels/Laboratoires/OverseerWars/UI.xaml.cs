using System.Windows;
using System.Windows.Controls;
using LaboratoireProgrammation.Project.Services;
using LaboratoireProgrammation.Project.ViewModels.Menu;

namespace LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars;

public partial class UI : UserControl {
    
    private MainWindow? Win => Window.GetWindow(this) as MainWindow;
    public UI() {
        InitializeComponent();
        Loaded += (s, e) => {
            DisablePreviousUI();
        };
    }

    private void DisablePreviousUI() {
        Win.VisualMode.Visibility = Visibility.Collapsed;
        Win.InputBox.Visibility = Visibility.Collapsed;
        Win.InputBoxIndicator.Visibility = Visibility.Collapsed;
        Win.MainMenuButton.Visibility = Visibility.Collapsed;
        Win.WinBtnClose.Visibility = Visibility.Collapsed;
        Win.WinBtnMaximize.Visibility = Visibility.Collapsed;
        Win.WinBtnMinimize.Visibility = Visibility.Collapsed;
        Win.TerminalScroller.Visibility = Visibility.Collapsed;
        Win.TerminalOutputPanel.Visibility = Visibility.Collapsed;
        Win.TopText.Visibility = Visibility.Collapsed;
        TerminalDisplay._outputBox.Visibility = Visibility.Collapsed;
    }
}