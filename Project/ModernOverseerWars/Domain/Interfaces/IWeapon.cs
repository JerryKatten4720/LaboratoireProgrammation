namespace LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Interfaces;

public interface IWeapon : ICard {
    int Damage { get; set; }
    string WeaponType { get; set; }
}
