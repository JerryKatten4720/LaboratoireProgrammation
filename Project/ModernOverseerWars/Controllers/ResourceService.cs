namespace LaboratoireProgrammation.Project.ModernOverseerWars.Controllers;

using System.Collections.Generic;
using System.Linq;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;

public class ResourceService {
    private readonly GameState _state;

    private static readonly Dictionary<RoomType, int> RoomCosts = new() {
        { RoomType.WeaponFactory,  15 },
        { RoomType.OutfitFactory,  15 },
        { RoomType.TrainingCenter, 20 },
        { RoomType.TechCenter,     25 },
    };

    public ResourceService(GameState state) {
        _state = state;
    }

    public (bool ok, string error) TryBuildRoom(Vault vault, RoomType type) {
        if (vault.Rooms.Any(r => r.Type == type)) return (false, "Room already built!");
        
        int cost = RoomCosts.GetValueOrDefault(type, 10);
        if (vault.Electricity < cost) return (false, $"Need {cost} ");
        
        vault.Electricity -= cost;
        var newRoom = new Room { Type = type, BuildCost = cost };
        vault.Rooms.Add(newRoom);
        _state.AddLog($"{vault.OwnerName} built {newRoom.Name}");
        
        return (true, "");
    }

    public bool TryAssignDweller(Vault vault, Dweller dweller, Room room) {
        if (!dweller.IsAlive) return false;
        
        foreach (var r in vault.Rooms) {
            r.AssignedDwellers.Remove(dweller);
        }
        
        room.AssignedDwellers.Add(dweller);
        _state.AddLog($"{dweller.Name} assigned to {room.Name}");
        
        return true;
    }

    public (bool ok, string error) TryUnlockTechBonus(Vault vault, string bonus) {
        if (!vault.Rooms.Any(r => r.Type == RoomType.TechCenter)) {
            return (false, "Build Tech Center first!");
        }
        
        if (vault.Electricity < 15) return (false, "Need 15 ");
        
        vault.Electricity -= 15;
        switch (bonus) {
            case "ReliableAim":  vault.HasReliableAim  = true; break;
            case "FrankTheTank": vault.HasFrankTheTank = true; break;
        }
        
        _state.AddLog($"{vault.OwnerName} unlocked {bonus}");
        return (true, "");
    }
}
