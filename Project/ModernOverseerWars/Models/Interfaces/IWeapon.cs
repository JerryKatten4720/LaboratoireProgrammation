namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;

public interface IWeapon : ICard {
    int Damage { get; set; }
    string WeaponType { get; set; }
}
