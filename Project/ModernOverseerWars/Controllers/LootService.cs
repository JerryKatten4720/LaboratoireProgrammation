namespace LaboratoireProgrammation.Project.ModernOverseerWars.Controllers;

using System.Linq;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Map;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data;
using LaboratoireProgrammation.Project.ModernOverseerWars.Helpers;

public class LootService {
    public (ICard? card, string message) SearchTile(HexTile tile, Dweller dweller, bool isPlayer1, Vault vault) {
        tile.RevealFor(isPlayer1);
        int roll = RandomProvider.Next(100);
        int bonus = tile.Type == TileType.Location ? 10 : 0;
        roll -= bonus;

        if (roll < 5 && GameDataRepository.Dwellers.Count > 0) {
            var d = CreateRandomDweller("Dweller", false);
            return (d, $"Recruited {d.Name}!");
        }
        else if (roll < 10 && GameDataRepository.Weapons.Count > 0) {
            var w = CreateRandomWeapon(RollRarity(dweller.Special_L));
            return (w, $"Found weapon: {w.Name}!");
        }
        else if (roll < 15 && GameDataRepository.Outfits.Count > 0) {
            var o = CreateRandomOutfit(RollRarity(dweller.Special_L));
            return (o, $"Found outfit: {o.Name}!");
        }
        else if (roll < 40 && GameDataRepository.Scraps.Count > 0) {
            var sJson = GameDataRepository.Scraps[RandomProvider.Next(GameDataRepository.Scraps.Count)];
            CardRarity rarity = RarityHelper.ParseRarity(sJson.Rarity);
            var s = new Scrap { Name = sJson.ScrapName, Texture = sJson.Texture, Rarity = rarity };
            vault.Scraps.Add(s);
            return (s, $"Found scrap: {s.Name}!");
        }
        else if (roll < 70) {
            int resRoll = RandomProvider.Next(3);
            int amount = RandomProvider.Next(10, 31);
            if (resRoll == 0) { vault.Food += amount; return (null, $"Found {amount} Food!"); }
            else if (resRoll == 1) { vault.Water += amount; return (null, $"Found {amount} Water!"); }
            else { vault.Electricity += amount; return (null, $"Found {amount} Electricity!"); }
        }
        
        return (null, "Found some caps... nothing useful.");
    }

    public CardRarity RollRarity(int luckStat) {
        int roll = RandomProvider.Next(100) + luckStat;
        return roll switch {
            >= 95 => CardRarity.Legendary,
            >= 85 => CardRarity.Epic,
            >= 70 => CardRarity.Rare,
            >= 50 => CardRarity.Uncommon,
            _     => CardRarity.Common
        };
    }

    public Dweller CreateRandomDweller(string fallbackName, bool isSupervisor = false) {
        if (GameDataRepository.Dwellers.Count > 0) {
            var dj = GameDataRepository.Dwellers[RandomProvider.Next(GameDataRepository.Dwellers.Count)];
            var d = new Dweller {
                FirstName = dj.FirstName,
                LastName = dj.LastName,
                Special_S = dj.S,
                Special_P = dj.P,
                Special_E = dj.E,
                Special_C = dj.C,
                Special_I = dj.I,
                Special_A = dj.A,
                Special_L = dj.L,
                RarityVal = dj.Rarity,
                Texture = dj.Texture,
                IsSupervisor = isSupervisor
            };
            d.CurrentHp = d.MaxHp;
            return d;
        }
        var d2 = new Dweller {
            FirstName = fallbackName,
            IsSupervisor = isSupervisor,
            Special_S = RandomProvider.Next(2, 6),
            Special_P = RandomProvider.Next(2, 6),
            Special_E = RandomProvider.Next(2, 6),
            Special_C = RandomProvider.Next(2, 6),
            Special_I = RandomProvider.Next(2, 6),
            Special_A = RandomProvider.Next(2, 6),
            Special_L = RandomProvider.Next(2, 6)
        };
        d2.CurrentHp = d2.MaxHp;
        return d2;
    }

    public Weapon CreateRandomWeapon(CardRarity rarity = CardRarity.Common) {
        if (GameDataRepository.Weapons.Count > 0) {
            var wJson = GameDataRepository.Weapons[RandomProvider.Next(GameDataRepository.Weapons.Count)];
            var w = new Weapon {
                Name = wJson.WeaponName,
                Texture = wJson.Texture,
                Damages = wJson.Damages,
                CriticalChance = wJson.CriticalChance,
                Rarity = rarity
            };
            w.WeaponType = WeaponTypeHelper.DetermineWeaponType(w.Name);
            return w;
        }
        return new Weapon { Rarity = rarity };
    }

    public Outfit CreateRandomOutfit(CardRarity rarity = CardRarity.Common) {
        if (GameDataRepository.Outfits.Count > 0) {
            var oJson = GameDataRepository.Outfits[RandomProvider.Next(GameDataRepository.Outfits.Count)];
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
