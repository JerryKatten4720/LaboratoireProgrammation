using System.Windows.Media;
using LaboratoireProgrammation.Project.ViewModels.Menu;

namespace LaboratoireProgrammation.Project.Services;

public class CommandsProcessor {
    public static void ProcessCommand(string cmd, MainWindow Window) {
        switch (cmd) {
            case "help":
                Window.AppendOutput("COMMAND LIST:");
                Window.BlankSpace();
                Window.AppendOutput("  help    - Display this database");
                Window.AppendOutput("  clear   - Purge screen buffer");
                Window.AppendOutput("  font    - Adjust terminal readability");
                Window.AppendOutput("  exit    - Terminate session");
                Window.AppendOutput("  about   - System info");
                Window.BlankSpace();
                Window.AppendOutput("  exo1   - Launch Exo (1+B)");
                Window.AppendOutput("  exo2   - Launch Exo (2)");
                Window.AppendOutput("  lab1   - Launch Lab (1)");
                break;

            case "info":
                Window.AppendOutput("ROBCO UNIFIED OPERATING SYSTEM v1.0");
                Window.AppendOutput("Running on WPF .NET Core.");
                Window.BlankSpace();
                Window.AppendOutput("• [exo.1+b] : UI Interactive : Contrôle de boutons et permutation d'images",
                    Colors.Cyan);
                Window.AppendOutput("• [exo.2]   : Simulation de transfert de fichiers (Barres de progression)",
                    Colors.Cyan);
                Window.AppendOutput("• [lab.1]   : Système de gestion de BDD (CRUD Complet) [EN DÉVELOPPEMENT]",
                    Colors.Yellow);
                break;

            case "clear":
                Window.OutputBox.Document.Blocks.Clear();
                break;

            case "font":
                Window._isAdjustingSize = true;
                Window.AppendOutput("[SYSTEM] FONT ADJUSTMENT MODE ENGAGED.", Colors.Yellow);
                Window.AppendOutput("Use [UP/DOWN] to scale text. Press [ENTER] to confirm.", Colors.Yellow);
                break;

            case "about":
                Window.AppendOutput("ROBCO UNIFIED OPERATING SYSTEM v1.0");
                Window.AppendOutput("Running on WPF .NET Core.");
                break;

            case "exit":
                Window.Close();
                break;

            case "exo1":
                Window.AppendOutput("[SYSTEM] DÉMARRAGE >>> EXO [1+B]", Colors.Yellow);
                Window.AppendOutput("...", Colors.Yellow);
                Window.FirstExo();
                break;

            case "exo2":
                Window.AppendOutput("[SYSTEM] DÉMARRAGE >>> EXO [2]", Colors.Yellow);
                Window.AppendOutput("...", Colors.Yellow);
                Window.SecondExo();
                break;

            case "lab1":
                Window.AppendOutput("[SYSTEM] DÉMARRAGE >>> LABORATOIRE [1]", Colors.Yellow);
                Window.AppendOutput("...", Colors.Yellow);
                Window.FirstLab();
                break;

            case "memfy":
            case "memfyai":
            case "memfy ai":
                Window.MemfyAgreementLaunch();
                break;

            default:
                Window.AppendOutput($"[ERROR] COMMAND '{cmd}' UNRECOGNIZED.", Colors.Red);
                break;
        }

        Window.BlankSpace();
    }
}