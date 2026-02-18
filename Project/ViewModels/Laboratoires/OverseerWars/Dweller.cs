namespace LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars;

public class Dweller {
    
    public String FirstName, LastName;
    public int S, P, E, C, I, A, L;

    public int X, Y;

    public Dweller(String FN, String LN, int s, int p, int e, int c, int i, int a, int l) {
        FirstName = FN;
        LastName = LN;

        S = s; P = p; E = e;
        C = c; I = i; A = a; L = l;

        X = 0;
        Y = 0;
    }

    public void Move(int x, int y) {
        X = x;
        X = y;
    }
    
}