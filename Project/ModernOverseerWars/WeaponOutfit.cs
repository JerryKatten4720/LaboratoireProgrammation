using System.Text.Json;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public class Weapon : IWeapon {
    public string Name { get; set; } = "Pipe Pistol";
    public string Texture { get; set; } = "placeholder.png";
    public int Damages { get; set; }
    public int Damage { get => Damages; set => Damages = value; }
    public string WeaponType { get; set; } = "Ranged";
    public string CriticalChance { get; set; } = "0";
    public CardRarity Rarity { get; set; } = CardRarity.Common;

    public static Weapon CreateRandom(CardRarity rarity = CardRarity.Common) {
        if (GameDataRepository.Weapons.Count > 0) {
            var wJson = GameDataRepository.Weapons[Random.Shared.Next(GameDataRepository.Weapons.Count)];
            var w = new Weapon {
                Name = wJson.WeaponName,
                Texture = wJson.Texture,
                Damages = wJson.Damages,
                CriticalChance = wJson.CriticalChance,
                Rarity = rarity
            };
            w.WeaponType = (w.Name.Contains("Bat") || w.Name.Contains("Knife") || w.Name.Contains("Sword")) ? "Melee" : "Ranged";
            return w;
        }
        return new Weapon { Rarity = rarity };
    }
}

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

    public static Outfit CreateRandom(CardRarity rarity = CardRarity.Common) {
        if (GameDataRepository.Outfits.Count > 0) {
            var oJson = GameDataRepository.Outfits[Random.Shared.Next(GameDataRepository.Outfits.Count)];
            return new Outfit {
                Name = oJson.OutfitName,
                Texture = oJson.Texture,
                Defense = oJson.Defense,
                S = oJson.S, P = oJson.P, E = oJson.E, C = oJson.C, I = oJson.I, A = oJson.A, L = oJson.L,
                Rarity = rarity
            };
        }
        return new Outfit { Rarity = rarity };
    }
}

public class ScrapJson {
    public string ScrapName { get; set; } = "";
    public string Texture { get; set; } = "";
    public string Rarity { get; set; } = "1";
}

public class Scrap : ICard {
    public string Name { get; set; } = "Wooden Plank";
    public string Texture { get; set; } = "placeholder.png";
    public CardRarity Rarity { get; set; } = CardRarity.Common;
}


public class DwellerJson {
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int S { get; set; }
    public int P { get; set; }
    public int E { get; set; }
    public int C { get; set; }
    public int I { get; set; }
    public int A { get; set; }
    public int L { get; set; }
    public int Rarity { get; set; }
    public string Texture { get; set; } = "";
}

public class WeaponJson {
    public string WeaponName { get; set; } = "";
    public string Texture { get; set; } = "";
    public int Damages { get; set; }
    public string CriticalChance { get; set; } = "";
}

public class OutfitJson {
    public string OutfitName { get; set; } = "";
    public string Texture { get; set; } = "";
    public string Defense { get; set; } = "";
    public int S { get; set; }
    public int P { get; set; }
    public int E { get; set; }
    public int C { get; set; }
    public int I { get; set; }
    public int A { get; set; }
    public int L { get; set; }
}

public class GameConfig {
    public int MaxActionPoints { get; set; } = 5;
    public int TurnDurationSeconds { get; set; } = 60;
    public int StarterDwellersCount { get; set; } = 3;
    public int StarterWeaponsCount { get; set; } = 1;
    public int StarterOutfitsCount { get; set; } = 1;
    public int MapCols { get; set; } = 12;
    public int MapRows { get; set; } = 5;
    public double MapTransparency { get; set; } = 0.8;
    public double VaultUITransparency { get; set; } = 1.0;
    public string UIColor { get; set; } = "#0D0D20";
    public bool EnableFogOfWar { get; set; } = false;
}

public static class GameConfigManager {
    public static GameConfig Config { get; private set; } = new();
    public static void Load(string path) {
        try {
            if (System.IO.File.Exists(path)) {
                Config = JsonSerializer.Deserialize<GameConfig>(System.IO.File.ReadAllText(path)) ?? new();
            }
        }
        catch {
            
        }
    }
}

public static class GameDataRepository {
    public static List<DwellerJson> Dwellers { get; set; } = new();
    public static List<WeaponJson> Weapons { get; set; } = new();
    public static List<OutfitJson> Outfits { get; set; } = new();
    public static List<ScrapJson> Scraps { get; set; } = new();
}
