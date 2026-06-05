using System;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;

namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;

public interface ICard {
    Guid Id { get; set; }
    string Name { get; set; }
    CardRarity Rarity { get; set; }
}
