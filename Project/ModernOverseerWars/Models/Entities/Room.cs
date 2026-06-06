namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;

using System.Collections.Generic;
using System.Linq;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data;

public class Room {
    public RoomType Type { get; set; }
    
    public string Name => Type switch {
        RoomType.Generator      => "Generator",
        RoomType.Garden         => "🌿 Gardens",
        RoomType.WaterPurifier  => "Water Station",
        RoomType.WeaponFactory  => "🔫 Weapon Factory",
        RoomType.OutfitFactory  => "👕 Outfit Factory",
        RoomType.TrainingCenter => "🏋️ Training Center",
        RoomType.TechCenter     => "🔬 Tech Center",
        _ => "Room"
    };
    
    public List<Dweller> AssignedDwellers { get; set; } = new();
    public int Level { get; set; } = 1;
    public int BuildCost { get; set; } = GameConfigRepository.Config.RoomBuildCost;
    public int ProduceValue => GetProduceValue(Type, AssignedDwellers);

    public int GetProduceValue(RoomType type, List<Dweller> dwellers) {
        int production = 0;
        
        switch (type) {
            case RoomType.Garden: foreach (var d in dwellers) { production += d.Special_A / 2; } break;
            case RoomType.WaterPurifier: foreach (var d in dwellers) { production += d.Special_P / 2; } break;
            case RoomType.Generator: foreach (var d in dwellers) { production += d.Special_S / 2; } break;
        }

        return production;

    } 
}
