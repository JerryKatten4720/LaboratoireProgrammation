namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;

using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;

public class Weapon : IWeapon {
    public string Name { get; set; } = "Pipe Pistol";
    public string Texture { get; set; } = "placeholder.png";
    public int Damages { get; set; }
    public int Damage { get => Damages; set => Damages = value; }
    public string WeaponType { get; set; } = "Ranged";
    public string CriticalChance { get; set; } = "0";
    public CardRarity Rarity { get; set; } = CardRarity.Common;
}
