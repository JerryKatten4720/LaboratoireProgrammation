namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public enum RoomType { Generator, Garden, WaterPurifier, WeaponFactory, OutfitFactory, TrainingCenter, TechCenter }

public class Room {
    public RoomType Type { get; set; }
    public string Name => Type switch {
        RoomType.Generator      => "⚡ Generator",
        RoomType.Garden         => "🍅 Gardens",
        RoomType.WaterPurifier  => "💧 Water Station",
        RoomType.WeaponFactory  => "🔫 Weapon Factory",
        RoomType.OutfitFactory  => "🧥 Outfit Factory",
        RoomType.TrainingCenter => "🏋 Training Center",
        RoomType.TechCenter     => "💻 Tech Center",
        _ => "Room"
    };
    public List<Dweller> AssignedDwellers { get; } = new();
    public int Level     { get; set; } = 1;
    public int BuildCost { get; set; } = 10;
    public int ProduceValue => AssignedDwellers.Count(d => d.IsAlive) * Level;
}

public class Vault {
    public string       OwnerName    { get; set; } = "Player";
    public List<Room>   Rooms        { get; } = new();
    public List<Dweller> Dwellers    { get; } = new();
    public List<IWeapon> Weapons     { get; } = new();
    public List<IOutfit> Outfits     { get; } = new();
    public List<Scrap> Scraps        { get; } = new();
    public IEnumerable<IWeapon> UnusedWeapons => Weapons.Where(w => !Dwellers.Any(d => d.EquippedWeapon == w));
    public IEnumerable<IOutfit> UnusedOutfits => Outfits.Where(o => !Dwellers.Any(d => d.EquippedOutfit == o));

    public int Electricity  { get; set; } = 20;
    public int Water        { get; set; } = 20;
    public int Food         { get; set; } = 20;
    public int ActionPoints { get; set; } = 5;
    public int MaxActionPoints { get => GameConfigManager.Config.MaxActionPoints; set {} }
    public bool HasReliableAim  { get; set; } = false;
    public bool HasFrankTheTank { get; set; } = false;

    public Vault(string owner) {
        OwnerName = owner;
        ActionPoints = GameConfigManager.Config.MaxActionPoints;
        Rooms.Add(new Room { Type = RoomType.Generator });
        Rooms.Add(new Room { Type = RoomType.Garden });
        Rooms.Add(new Room { Type = RoomType.WaterPurifier });
    }

    public void ProduceResources() {
        foreach (var room in Rooms)
            switch (room.Type) {
                case RoomType.Generator:     Electricity += room.ProduceValue; break;
                case RoomType.Garden:        Food        += room.ProduceValue; break;
                case RoomType.WaterPurifier: Water       += room.ProduceValue; break;
            }
            
        int livingDwellers = Dwellers.Count(d => d.IsAlive);
        Food -= livingDwellers;
        Water -= livingDwellers;
        Electricity -= Rooms.Count;

        if (Water <= 0 || Food <= 0)
            foreach (var d in Dwellers.Where(d => d.IsAlive)) {
                if (Water <= 0) d.CurrentHp -= 2;
                if (Food  <= 0) d.CurrentHp -= 1;
                if (d.CurrentHp < 0) d.CurrentHp = 0;
            }
    }

    public void ResetActionPoints() => ActionPoints = MaxActionPoints;
    public Dweller? Supervisor => Dwellers.FirstOrDefault(d => d.IsSupervisor);

    public IEnumerable<Dweller> DwellersInVault(HexMap map) {
        var onMap = new HashSet<Dweller>();
        for (int c = 0; c < map.Cols; c++)
            for (int r = 0; r < map.Rows; r++) {
                foreach (var d in map.Get(c, r).Player1Dwellers) onMap.Add(d);
                foreach (var d in map.Get(c, r).Player2Dwellers) onMap.Add(d);
            }
        return Dwellers.Where(d => !onMap.Contains(d));
    }
}
