using System.Text.Json.Serialization;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;

namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;

[JsonDerivedType(typeof(Outfit), typeDiscriminator: "outfit")]
public interface IOutfit : ICard {
    int ArmorValue { get; set; }
}
