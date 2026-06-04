using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data.Json;

namespace LaboratoireProgrammation.Project.ModernOverseerWars.Controllers;

public class GameController {
    public GameState State { get; private set; }
    public TurnService Turns { get; private set; }
    public ResourceService Resources { get; private set; }
    public CombatService Combat { get; private set; }
    public LootService Loot { get; private set; }
    public MovementService Movement { get; private set; }
    public EquipmentService Equipment { get; private set; }

    public GameController(GameState state) {
        State = state;
        Turns = new TurnService(state);
        Resources = new ResourceService(state);
        Combat = new CombatService(state);
        Loot = new LootService();
        Movement = new MovementService(state, Combat);
        Equipment = new EquipmentService(state);
    }

    public static void LoadConfig() {
        GameConfigRepository.Load(FindJsonFile("config.json"));
    }

    public void SetupPlayers() {
        try {
            string dp = FindJsonFile("dwellers.json");
            string wp = FindJsonFile("weapons.json");
            string op = FindJsonFile("outfits.json");
            string sp = FindJsonFile("scraps.json");

            GameDataRepository.Dwellers = JsonSerializer.Deserialize<List<DwellerJson>>(File.ReadAllText(dp)) ?? new();
            GameDataRepository.Weapons = JsonSerializer.Deserialize<List<WeaponJson>>(File.ReadAllText(wp)) ?? new();
            GameDataRepository.Outfits = JsonSerializer.Deserialize<List<OutfitJson>>(File.ReadAllText(op)) ?? new();
            GameDataRepository.Scraps = JsonSerializer.Deserialize<List<ScrapJson>>(File.ReadAllText(sp)) ?? new();

            var rng = Random.Shared;
            var sh = GameDataRepository.Dwellers.OrderBy(_ => rng.Next()).ToList();
            int count = Math.Min(GameConfigRepository.Config.StarterDwellersCount, sh.Count / 2);

            for (int i = 0; i < count; i++) {
                var d1 = CreateDwellerFromParsed(sh[i], i == 0);
                State.Vault1.Dwellers.Add(d1);
                var d2 = CreateDwellerFromParsed(sh[i + count], i == 0);
                State.Vault2.Dwellers.Add(d2);
            }

            if (GameDataRepository.Weapons.Count > 0) {
                var w1 = CreateWeaponFromParsed(GameDataRepository.Weapons[rng.Next(GameDataRepository.Weapons.Count)]);
                State.Vault1.Weapons.Add(w1);
                var w2 = CreateWeaponFromParsed(GameDataRepository.Weapons[rng.Next(GameDataRepository.Weapons.Count)]);
                State.Vault2.Weapons.Add(w2);
            }

            if (GameDataRepository.Outfits.Count > 0) {
                var o1 = CreateOutfitFromParsed(GameDataRepository.Outfits[rng.Next(GameDataRepository.Outfits.Count)]);
                State.Vault1.Outfits.Add(o1);
                var o2 = CreateOutfitFromParsed(GameDataRepository.Outfits[rng.Next(GameDataRepository.Outfits.Count)]);
                State.Vault2.Outfits.Add(o2);
            }

            EquipStarter(State.Vault1);
            EquipStarter(State.Vault2);
        }
        catch (Exception ex) {
            // Note: Since this is a Controller, it shouldn't show a MessageBox directly.
            // But we will throw or let it be handled by the view for now.
            throw new Exception($"Error loading data: {ex.Message}");
        }
    }

    private static string FindJsonFile(string filename) {
        string[] paths = {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "data", filename),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Assets", "data", filename),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Assets", "data", filename),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Assets", "data", filename),
            Path.Combine("Assets", "data", filename),
            Path.Combine("..", "Assets", "data", filename),
            Path.Combine("..", "..", "Assets", "data", filename),
            Path.Combine("..", "..", "..", "Assets", "data", filename),
            @"C:\Users\antoi\Documents\GitHub\LaboratoireProgrammation\Assets\data\" + filename
        };
        foreach (var p in paths) {
            if (File.Exists(p)) return p;
        }
        throw new FileNotFoundException($"Could not find JSON file: {filename}");
    }

    private Dweller CreateDwellerFromParsed(DwellerJson dj, bool sup) {
        var d = new Dweller {
            FirstName = dj.FirstName, LastName = dj.LastName,
            Special_S = dj.S, Special_P = dj.P, Special_E = dj.E, Special_C = dj.C, Special_I = dj.I, Special_A = dj.A, Special_L = dj.L,
            RarityVal = dj.Rarity, Texture = dj.Texture, IsSupervisor = sup
        };
        d.CurrentHp = d.MaxHp;
        return d;
    }

    private Weapon CreateWeaponFromParsed(WeaponJson wj) {
        var w = new Weapon { Name = wj.WeaponName, Texture = wj.Texture, Damages = wj.Damages, CriticalChance = wj.CriticalChance, Rarity = CardRarity.Common };
        w.WeaponType = (w.Name.Contains("Bat") || w.Name.Contains("Knife") || w.Name.Contains("Sword")) ? "Melee" : "Ranged";
        return w;
    }

    private Outfit CreateOutfitFromParsed(OutfitJson oj) {
        return new Outfit { Name = oj.OutfitName, Texture = oj.Texture, Defense = oj.Defense, S = oj.S, P = oj.P, E = oj.E, C = oj.C, I = oj.I, A = oj.A, L = oj.L, Rarity = CardRarity.Common };
    }

    private void EquipStarter(Vault v) {
        var s = v.Supervisor;
        if (s != null) {
            var w = v.Weapons.FirstOrDefault();
            if (w != null) s.EquippedWeapon = w;
            var o = v.Outfits.FirstOrDefault();
            if (o != null) s.EquippedOutfit = o;
        }
    }
}
