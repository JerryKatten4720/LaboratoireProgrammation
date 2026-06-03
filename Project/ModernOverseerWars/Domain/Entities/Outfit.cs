namespace LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Entities;

using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Enums;

public class Outfit : IOutfit {
    public string Name { get; set; } = "Vault Suit";
    public string Texture { get; set; } = "placeholder.png";
    public string Defense { get; set; } = "0";
    
    public int ArmorValue {
        get => int.TryParse(Defense, out int v) ? v : 0;
        set => Defense = value.ToString();
    }
    
    public int S { get; set; }
    public int P { get; set; }
    public int E { get; set; }
    public int C { get; set; }
    public int I { get; set; }
    public int A { get; set; }
    public int L { get; set; }
    public CardRarity Rarity { get; set; } = CardRarity.Common;
}
