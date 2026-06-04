namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;

public interface IDweller : ICard {
    int Special_S { get; set; }
    int Special_P { get; set; }
    int Special_E { get; set; }
    int Special_C { get; set; }
    int Special_I { get; set; }
    int Special_A { get; set; }
    int Special_L { get; set; }
    int MaxHp { get; }
    int CurrentHp { get; set; }
    bool IsSupervisor { get; set; }
    IWeapon? EquippedWeapon { get; set; }
    IOutfit? EquippedOutfit { get; set; }
    int AttackDamage { get; }
}
