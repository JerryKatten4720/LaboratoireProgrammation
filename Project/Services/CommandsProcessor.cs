using System.Windows;
using System.Windows.Media;
using LaboratoireProgrammation.Project.Models.Menu;
using LaboratoireProgrammation.Project.Models.Miscellaneous;
using LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars;
using LaboratoireProgrammation.Project.ViewModels.Menu;
using Wpf.Ui;

namespace LaboratoireProgrammation.Project.Services;

public class CommandsProcessor {
    public static async void ProcessCommand(string cmd, MainWindow window) {
        switch (cmd) {
            case "help":
                TerminalDisplay.AppendOutput("COMMAND LIST:");
                TerminalDisplay.BlankSpace();
                TerminalDisplay.AppendOutput("  help    - Display this database");
                TerminalDisplay.AppendOutput("  clear   - Purge screen buffer");
                TerminalDisplay.AppendOutput("  font    - Adjust terminal readability");
                TerminalDisplay.AppendOutput("  exit    - Terminate session");
                TerminalDisplay.AppendOutput("  about   - System info");
                TerminalDisplay.BlankSpace();
                TerminalDisplay.AppendOutput("  exo1   - Launch Exo (1+B)");
                TerminalDisplay.AppendOutput("  exo2   - Launch Exo (2)");
                TerminalDisplay.AppendOutput("  lab1   - Launch Lab (1)");
                break;

            case "info":
                TerminalDisplay.AppendOutput("ROBCO UNIFIED OPERATING SYSTEM v1.0");
                TerminalDisplay.AppendOutput("Running on WPF .NET Core.");
                TerminalDisplay.BlankSpace();
                TerminalDisplay.AppendOutput("• [exo.1+b] : UI Interactive : Contrôle de boutons et permutation d'images", Colors.Coral);
                TerminalDisplay.AppendOutput("• [exo.2]   : Simulation de transfert de fichiers (Barres de progression)", Colors.Coral);
                TerminalDisplay.AppendOutput("• [lab.1]   : Système de gestion de BDD (CRUD Complet) [EN DÉVELOPPEMENT]", Colors.Gold);
                break;

            case "clear":
                window.OutputBox.Document.Blocks.Clear();
                break;

            case "font":
                ConsoleBehavior.IsAdjustingSize = true;
                TerminalDisplay.AppendOutput("[SYSTEM] FONT ADJUSTMENT MODE ENGAGED.", Colors.Yellow);
                TerminalDisplay.AppendOutput("Use [UP/DOWN] to scale text. Press [ENTER] to confirm.", Colors.Yellow);
                break;

            case "about":
                TerminalDisplay.AppendOutput("ROBCO UNIFIED OPERATING SYSTEM v1.0");
                TerminalDisplay.AppendOutput("Running on WPF .NET Core.");
                break;

            case "exit":
                window.Close();
                break;

            case "exo1":
                TerminalDisplay.AppendOutput("[SYSTEM] DÉMARRAGE >>> EXO [1+B]", Colors.Yellow);
                TerminalDisplay.AppendOutput("...", Colors.Yellow);
                RunExo1.Run(window);
                break;

            case "exo2":
                TerminalDisplay.AppendOutput("[SYSTEM] DÉMARRAGE >>> EXO [2]", Colors.Yellow);
                TerminalDisplay.AppendOutput("...", Colors.Yellow);
                RunExo2.Run(window);
                break;
            
            case "exo3":
                TerminalDisplay.AppendOutput("[SYSTEM] DÉMARRAGE >>> EXO [3]", Colors.Yellow);
                TerminalDisplay.AppendOutput("...", Colors.Yellow);
                RunExo3.Run(window);
                break;

            case "lab1":
                TerminalDisplay.AppendOutput("[SYSTEM] DÉMARRAGE >>> LABORATOIRE [1]", Colors.Yellow);
                TerminalDisplay.AppendOutput("...", Colors.Yellow);
                RunLab1.Run(window);
                break;

            case "memfy":
            case "memfyai":
            case "memfy ai":
                RunMemfy.Run(window);
                break;
            
            case "sep":
                TerminalDisplay.SeparationLine();
                break;
            
            case "ow":
                OverseerWarInit.Introduce(window);
                break;
            
            case "te":
                window.OverseerWarsMenu.Visibility = Visibility.Visible;
                break;

            default:
                TerminalDisplay.AppendOutput($"[ERROR] COMMAND '{cmd}' UNRECOGNIZED.", Colors.Red);
                break;
        }

        TerminalDisplay.BlankSpace();
    }
}