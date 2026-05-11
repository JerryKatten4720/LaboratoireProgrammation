namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public static class ControlCardFactory {
    public static ControlCard Create(string image, int power) {
        return new ControlCard {
            CardImage = image,
            Power = power
        };
    }
}