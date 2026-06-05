namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;

using System.Collections.Generic;
using System.Linq;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Map;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data;

public class Vault {
    public string OwnerName { get; set; } = "Player";
    public List<Room> Rooms { get; set; } = new();
    public List<Dweller> Dwellers { get; set; } = new();
    public List<IWeapon> Weapons { get; set; } = new();
    public List<IOutfit> Outfits { get; set; } = new();
    public List<Scrap> Scraps { get; set; } = new();
    
    public IEnumerable<IWeapon> UnusedWeapons => Weapons.Where(w => !Dwellers.Any(d => d.EquippedWeapon == w));
    public IEnumerable<IOutfit> UnusedOutfits => Outfits.Where(o => !Dwellers.Any(d => d.EquippedOutfit == o));

    public int Electricity { get; set; }
    public int Water { get; set; }
    public int Food { get; set; }
    public int ActionPoints { get; set; } = 5;
    
    public int MaxActionPoints {
        get => GameConfigRepository.Config.MaxActionPoints;
        set { }
    }
    
    public bool HasReliableAim { get; set; } = false;
    public bool HasFrankTheTank { get; set; } = false;

    public Vault() {}

    public Vault(string owner) {
        OwnerName = owner;
        ActionPoints = GameConfigRepository.Config.MaxActionPoints;
        Electricity = GameConfigRepository.Config.InitialElectricity;
        Water = GameConfigRepository.Config.InitialWater;
        Food = GameConfigRepository.Config.InitialFood;
        Rooms.Add(new Room { Type = RoomType.Generator });
        Rooms.Add(new Room { Type = RoomType.Garden });
        Rooms.Add(new Room { Type = RoomType.WaterPurifier });
    }

    public void ProduceResources() {
        foreach (var room in Rooms) {
            switch (room.Type) {
                case RoomType.Generator:     Electricity += room.ProduceValue; break;
                case RoomType.Garden:        Food        += room.ProduceValue; break;
                case RoomType.WaterPurifier: Water       += room.ProduceValue; break;
            }
        }
            
        int livingDwellers = Dwellers.Count(d => d.IsAlive);
        Food -= livingDwellers;
        Water -= livingDwellers;
        Electricity -= Rooms.Count;

        if (Water <= 0 || Food <= 0) {
            foreach (var d in Dwellers.Where(d => d.IsAlive)) {
                if (Water <= 0) {
                    d.CurrentHp -= GameConfigRepository.Config.StarvationWaterHpPenalty;
                }
                if (Food <= 0) {
                    d.CurrentHp -= GameConfigRepository.Config.StarvationFoodHpPenalty;
                }
                if (d.CurrentHp < 0) {
                    d.CurrentHp = 0;
                }
            }
        }
    }

    public void ResetActionPoints() => ActionPoints = MaxActionPoints;
    
    public Dweller? Supervisor => Dwellers.FirstOrDefault(d => d.IsSupervisor);

    public IEnumerable<Dweller> DwellersInVault(HexMap map) {
        var onMap = new HashSet<Dweller>();
        for (int c = 0; c < map.Cols; c++) {
            for (int r = 0; r < map.Rows; r++) {
                foreach (var d in map.Get(c, r).Player1Dwellers) onMap.Add(d);
                foreach (var d in map.Get(c, r).Player2Dwellers) onMap.Add(d);
            }
        }
        return Dwellers.Where(d => !onMap.Contains(d));
    }
}
