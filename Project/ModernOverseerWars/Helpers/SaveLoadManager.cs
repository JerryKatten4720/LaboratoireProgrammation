using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Map;

namespace LaboratoireProgrammation.Project.ModernOverseerWars.Helpers;

public static class SaveLoadManager {
    private static readonly string SaveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");

    public static void Save(GameState state) {
        LaboratoireProgrammation.Project.ModernOverseerWars.Controllers.GameController.LoadConfig();
        Directory.CreateDirectory(SaveDir);
        string date = DateTime.Now.ToString("ddMMyyyy");
        string filename = $"{state.Player1.Pseudo}-{date}-{state.Player2.Pseudo}.json";
        string filepath = Path.Combine(SaveDir, filename);

        var options = new JsonSerializerOptions {
            WriteIndented = true
        };
        string json = JsonSerializer.Serialize(state, options);
        File.WriteAllText(filepath, json);
    }

    public static GameState Load(string filepath) {
        LaboratoireProgrammation.Project.ModernOverseerWars.Controllers.GameController.LoadConfig();
        var options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        };
        string json = File.ReadAllText(filepath);
        var state = JsonSerializer.Deserialize<GameState>(json, options) ?? throw new InvalidDataException();
        RelinkState(state);
        return state;
    }

    public static List<SaveFileEntry> GetSaves() {
        var list = new List<SaveFileEntry>();
        if (!Directory.Exists(SaveDir)) {
            return list;
        }
        var files = Directory.GetFiles(SaveDir, "*.json");
        foreach (var file in files) {
            list.Add(new SaveFileEntry {
                FileName = Path.GetFileName(file),
                FullPath = file
            });
        }
        return list;
    }

    private static void RelinkState(GameState state) {
        var p1Weapons = state.Vault1.Weapons.ToDictionary(w => w.Id);
        var p1Outfits = state.Vault1.Outfits.ToDictionary(o => o.Id);
        var p1Dwellers = state.Vault1.Dwellers.ToDictionary(d => d.Id);

        foreach (var d in state.Vault1.Dwellers) {
            if (d.EquippedWeapon != null && p1Weapons.TryGetValue(d.EquippedWeapon.Id, out var w)) {
                d.EquippedWeapon = w;
            }
            if (d.EquippedOutfit != null && p1Outfits.TryGetValue(d.EquippedOutfit.Id, out var o)) {
                d.EquippedOutfit = o;
            }
        }

        foreach (var r in state.Vault1.Rooms) {
            for (int i = 0; i < r.AssignedDwellers.Count; i++) {
                if (p1Dwellers.TryGetValue(r.AssignedDwellers[i].Id, out var d)) {
                    r.AssignedDwellers[i] = d;
                }
            }
        }

        var p2Weapons = state.Vault2.Weapons.ToDictionary(w => w.Id);
        var p2Outfits = state.Vault2.Outfits.ToDictionary(o => o.Id);
        var p2Dwellers = state.Vault2.Dwellers.ToDictionary(d => d.Id);

        foreach (var d in state.Vault2.Dwellers) {
            if (d.EquippedWeapon != null && p2Weapons.TryGetValue(d.EquippedWeapon.Id, out var w)) {
                d.EquippedWeapon = w;
            }
            if (d.EquippedOutfit != null && p2Outfits.TryGetValue(d.EquippedOutfit.Id, out var o)) {
                d.EquippedOutfit = o;
            }
        }

        foreach (var r in state.Vault2.Rooms) {
            for (int i = 0; i < r.AssignedDwellers.Count; i++) {
                if (p2Dwellers.TryGetValue(r.AssignedDwellers[i].Id, out var d)) {
                    r.AssignedDwellers[i] = d;
                }
            }
        }

        if (state.Map?.Tiles != null) {
            for (int c = 0; c < state.Map.Cols; c++) {
                for (int r = 0; r < state.Map.Rows; r++) {
                    var tile = state.Map.Get(c, r);
                    if (tile == null) continue;
                    for (int i = 0; i < tile.Player1Dwellers.Count; i++) {
                        if (p1Dwellers.TryGetValue(tile.Player1Dwellers[i].Id, out var d)) {
                            tile.Player1Dwellers[i] = d;
                        }
                    }
                    for (int i = 0; i < tile.Player2Dwellers.Count; i++) {
                        if (p2Dwellers.TryGetValue(tile.Player2Dwellers[i].Id, out var d)) {
                            tile.Player2Dwellers[i] = d;
                        }
                    }
                }
            }
        }
    }
}

public class SaveFileEntry {
    public string FileName { get; set; } = "";
    public string FullPath { get; set; } = "";
}
