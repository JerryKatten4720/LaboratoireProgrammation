namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;

using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;

public class Scrap : ICard {
    public string Name { get; set; } = "Wooden Plank";
    public string Texture { get; set; } = "placeholder.png";
    public CardRarity Rarity { get; set; } = CardRarity.Common;
}
