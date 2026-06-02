namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public static class LootManager {
    private static readonly Random _rng = new();

    public static (ICard? card, string message) SearchTile(HexTile tile, Dweller dweller, bool isPlayer1, Vault vault) {
        tile.RevealFor(isPlayer1);
        int roll = _rng.Next(100);
        int bonus = tile.Type == TileType.Location ? 10 : 0;
        roll -= bonus; // Lower roll is better

        if (roll < 5 && GameDataRepository.Dwellers.Count > 0) {
            var dJson = GameDataRepository.Dwellers[_rng.Next(GameDataRepository.Dwellers.Count)];
            var d = new Dweller {
                FirstName = dJson.FirstName, LastName = dJson.LastName,
                Special_S = dJson.S, Special_P = dJson.P, Special_E = dJson.E, Special_C = dJson.C, Special_I = dJson.I, Special_A = dJson.A, Special_L = dJson.L,
                RarityVal = dJson.Rarity, Texture = dJson.Texture
            };
            d.CurrentHp = d.MaxHp;
            return (d, $"Recruited {d.Name}!");
        }
        else if (roll < 10 && GameDataRepository.Weapons.Count > 0) {
            var wJson = GameDataRepository.Weapons[_rng.Next(GameDataRepository.Weapons.Count)];
            var w = new Weapon { Name = wJson.WeaponName, Texture = wJson.Texture, Damages = wJson.Damages, CriticalChance = wJson.CriticalChance, Rarity = RollRarity(dweller.Special_L) };
            w.WeaponType = (w.Name.Contains("Bat") || w.Name.Contains("Knife") || w.Name.Contains("Sword")) ? "Melee" : "Ranged";
            return (w, $"Found weapon: {w.Name}!");
        }
        else if (roll < 15 && GameDataRepository.Outfits.Count > 0) {
            var oJson = GameDataRepository.Outfits[_rng.Next(GameDataRepository.Outfits.Count)];
            var o = new Outfit { Name = oJson.OutfitName, Texture = oJson.Texture, Defense = oJson.Defense, S = oJson.S, P = oJson.P, E = oJson.E, C = oJson.C, I = oJson.I, A = oJson.A, L = oJson.L, Rarity = RollRarity(dweller.Special_L) };
            return (o, $"Found outfit: {o.Name}!");
        }
        else if (roll < 40 && GameDataRepository.Scraps.Count > 0) {
            var sJson = GameDataRepository.Scraps[_rng.Next(GameDataRepository.Scraps.Count)];
            int rarityVal = int.TryParse(sJson.Rarity, out int r) ? r : 1;
            CardRarity rarity = rarityVal switch { 1 => CardRarity.Common, 2 => CardRarity.Rare, 3 => CardRarity.Legendary, _ => CardRarity.Common };
            var s = new Scrap { Name = sJson.ScrapName, Texture = sJson.Texture, Rarity = rarity };
            vault.Scraps.Add(s);
            return (s, $"Found scrap: {s.Name}!");
        }
        else if (roll < 70) {
            int resRoll = _rng.Next(3);
            int amount = _rng.Next(10, 31);
            if (resRoll == 0) { vault.Food += amount; return (null, $"Found {amount} Food!"); }
            else if (resRoll == 1) { vault.Water += amount; return (null, $"Found {amount} Water!"); }
            else { vault.Electricity += amount; return (null, $"Found {amount} Electricity!"); }
        }
        
        return (null, "Found some caps... nothing useful.");
    }

    private static CardRarity RollRarity(int luckStat) {
        int roll = _rng.Next(100) + luckStat;
        return roll switch {
            >= 95 => CardRarity.Legendary,
            >= 85 => CardRarity.Epic,
            >= 70 => CardRarity.Rare,
            >= 50 => CardRarity.Uncommon,
            _     => CardRarity.Common
        };
    }
}
