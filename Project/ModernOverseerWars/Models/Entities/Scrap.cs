using System;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;

namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;

public class Scrap : ICard {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Wooden Plank";
    public string Texture { get; set; } = "placeholder.png";
    public CardRarity Rarity { get; set; } = CardRarity.Common;
}
