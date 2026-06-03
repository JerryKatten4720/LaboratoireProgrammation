namespace LaboratoireProgrammation.Project.ModernOverseerWars.Helpers;

public static class WeaponTypeHelper {
    public static string DetermineWeaponType(string name) {
        if (string.IsNullOrEmpty(name)) return "Ranged";
        return (name.Contains("Bat") || name.Contains("Knife") || name.Contains("Sword")) ? "Melee" : "Ranged";
    }
}
