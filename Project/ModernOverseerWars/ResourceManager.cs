namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public class ResourceManager {
    private readonly GameState _state;

    private static readonly Dictionary<RoomType, int> RoomCosts = new() {
        { RoomType.WeaponFactory,  15 },
        { RoomType.OutfitFactory,  15 },
        { RoomType.TrainingCenter, 20 },
        { RoomType.TechCenter,     25 },
    };

    public ResourceManager(GameState state) { _state = state; }

    public (bool ok, string error) TryBuildRoom(Vault vault, RoomType type) {
        if (vault.Rooms.Any(r => r.Type == type)) return (false, "Room already built!");
        int cost = RoomCosts.GetValueOrDefault(type, 10);
        if (vault.Electricity < cost) return (false, $"Need {cost} ⚡");
        vault.Electricity -= cost;
        vault.Rooms.Add(new Room { Type = type, BuildCost = cost });
        _state.AddLog($"{vault.OwnerName} built {new Room { Type = type }.Name}");
        return (true, "");
    }

    public bool TryAssignDweller(Vault vault, Dweller dweller, Room room) {
        if (!dweller.IsAlive) return false;
        foreach (var r in vault.Rooms) r.AssignedDwellers.Remove(dweller);
        room.AssignedDwellers.Add(dweller);
        _state.AddLog($"{dweller.Name} assigned to {room.Name}");
        return true;
    }

    public (bool ok, string error) TryUnlockTechBonus(Vault vault, string bonus) {
        if (!vault.Rooms.Any(r => r.Type == RoomType.TechCenter))
            return (false, "Build Tech Center first!");
        if (vault.Electricity < 15) return (false, "Need 15 ⚡");
        vault.Electricity -= 15;
        switch (bonus) {
            case "ReliableAim":  vault.HasReliableAim  = true; break;
            case "FrankTheTank": vault.HasFrankTheTank = true; break;
        }
        _state.AddLog($"{vault.OwnerName} unlocked {bonus}");
        return (true, "");
    }
}
