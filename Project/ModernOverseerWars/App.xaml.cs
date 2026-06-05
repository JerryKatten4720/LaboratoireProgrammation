using System.Windows;
using LaboratoireProgrammation.Project.ModernOverseerWars.Views;
namespace LaboratoireProgrammation.Project.ModernOverseerWars;
public partial class App : Application {
    private void OnStartup(object s, StartupEventArgs e) {
        var menu = new StartMenuWindow();
        if (menu.ShowDialog() == true) {
            if (menu.LoadedState != null) {
                var game = new OverseerWarsWindow(menu.LoadedState);
                game.Show();
            } else {
                var game = new OverseerWarsWindow(menu.Profile1, menu.Profile2);
                game.Show();
            }
        } else {
            Shutdown();
        }
    }
}
