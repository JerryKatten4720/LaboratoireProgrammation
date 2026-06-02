using System.Windows;
namespace LaboratoireProgrammation.Project.ModernOverseerWars;
public partial class App : Application {
    private void OnStartup(object s, StartupEventArgs e) {
        var menu = new StartMenuWindow();
        if (menu.ShowDialog() == true) {
            var game = new OverseerWarsWindow(menu.Profile1, menu.Profile2);
            game.Show();
        } else {
            Shutdown();
        }
    }
}
