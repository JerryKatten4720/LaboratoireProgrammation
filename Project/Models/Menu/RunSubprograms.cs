using LaboratoireProgrammation.Project.ViewModels.Menu;

namespace LaboratoireProgrammation.Project.Models.Menu;

public class RunSubprograms {
    
    public enum Subprograms {
        Exercice1, Exercice2, Exercice3,
        Labo1,
        Memfy
    }

    public static void RunSub(Subprograms sub, MainWindow win) {
        switch (sub) {
            
            case Subprograms.Exercice1:
                RunExo1.Run(win);
                break;
            
            case Subprograms.Exercice2:
                RunExo2.Run(win);
                break;
            
            case Subprograms.Exercice3:
                RunExo3.Run(win);
                break;
            
            case Subprograms.Labo1:
                RunLab1.Run(win);
                break;
            
        }
    }
}