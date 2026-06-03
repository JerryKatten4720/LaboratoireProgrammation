namespace LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Entities;

using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Enums;

public class Scrap : ICard {
    public string Name { get; set; } = "Wooden Plank";
    public string Texture { get; set; } = "placeholder.png";
    public CardRarity Rarity { get; set; } = CardRarity.Common;
}
