using LaboratoireProgrammation.Project.ViewModels.Menu;

namespace LaboratoireProgrammation.Project.Models.Menu;

public class RunSubprograms {
    
    public enum Subprograms {
        Exercice1, Exercice2, Exercice3, Exercice4, Exercice5, Exercice6, Exercice7, Exercice8,
        Labo1,
        Memfy,
        OverseerWars,
    }

    public static void RunSub(Subprograms sub, MainWindow win) {
        switch (sub) {
            
            case Subprograms.Exercice1:
                _ = MenuLoaders.Run_Exo1(win);
                break;
            
            case Subprograms.Exercice2:
                _ = MenuLoaders.Run_Exo2(win);
                break;
            
            case Subprograms.Exercice3:
                _ = MenuLoaders.Run_Exo3(win);
                break;
            
            case Subprograms.Exercice4:
                _ = MenuLoaders.Run_Exo4(win);
                break;
            
            case Subprograms.Exercice5:
                _ = MenuLoaders.Run_Exo5(win);
                break;
            
            case Subprograms.Exercice6:
                _ = MenuLoaders.Run_Exo6(win);
                break;
            
            case Subprograms.Exercice7:
                _ = MenuLoaders.Run_Exo7(win);
                break;
            
            case Subprograms.Exercice8:
                _ = MenuLoaders.Run_Exo8(win);
                break;
            
            case Subprograms.Labo1:
                RunLab1.Run(win);
                break;
            
            case Subprograms.OverseerWars:
                break;
            
        }
    }
}