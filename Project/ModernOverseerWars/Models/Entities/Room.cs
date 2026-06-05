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
    public int ProduceValue => AssignedDwellers.Count(d => d.IsAlive) * Level;
}
