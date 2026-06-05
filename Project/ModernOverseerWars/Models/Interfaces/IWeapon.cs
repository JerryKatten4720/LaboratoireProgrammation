using System.Text.Json.Serialization;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;

namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;

[JsonDerivedType(typeof(Weapon), typeDiscriminator: "weapon")]
public interface IWeapon : ICard {
    int Damage { get; set; }
    string WeaponType { get; set; }
}
