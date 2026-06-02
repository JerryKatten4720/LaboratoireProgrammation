namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public class Dweller : IDweller {
    public string FirstName { get; set; } = "Dweller";
    public string LastName { get; set; } = "";
    public string Name {
        get => string.IsNullOrEmpty(LastName) ? FirstName : $"{FirstName} {LastName}";
        set {
            var parts = value.Split(' ');
            FirstName = parts[0];
            LastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";
        }
    }
    public int RarityVal { get; set; }
    public CardRarity Rarity {
        get => (CardRarity)Math.Clamp(RarityVal, 0, 4);
        set => RarityVal = (int)value;
    }
    private int _s = 3;
    private int _p = 3;
    private int _e = 3;
    private int _c = 3;
    private int _i = 3;
    private int _a = 3;
    private int _l = 3;

    public int Special_S { get => _s + (EquippedOutfit is Outfit o ? o.S : 0); set => _s = value; }
    public int Special_P { get => _p + (EquippedOutfit is Outfit o ? o.P : 0); set => _p = value; }
    public int Special_E { get => _e + (EquippedOutfit is Outfit o ? o.E : 0); set => _e = value; }
    public int Special_C { get => _c + (EquippedOutfit is Outfit o ? o.C : 0); set => _c = value; }
    public int Special_I { get => _i + (EquippedOutfit is Outfit o ? o.I : 0); set => _i = value; }
    public int Special_A { get => _a + (EquippedOutfit is Outfit o ? o.A : 0); set => _a = value; }
    public int Special_L { get => _l + (EquippedOutfit is Outfit o ? o.L : 0); set => _l = value; }
    
    public int MaxHp => 10 + Special_E * 2;
    public int CurrentHp { get; set; }
    public bool IsSupervisor { get; set; } = false;
    public IWeapon? EquippedWeapon { get; set; }
    public IOutfit? EquippedOutfit { get; set; }
    public int AttackDamage => Special_S + (EquippedWeapon?.Damage ?? 0);
    public bool IsAlive => CurrentHp > 0;
    public string Texture { get; set; } = "dweller.png";

    public Dweller() { CurrentHp = MaxHp; }

    public static Dweller CreateRandom(string name, bool isSupervisor = false) {
        if (GameDataRepository.Dwellers.Count > 0) {
            var dj = GameDataRepository.Dwellers[new Random().Next(GameDataRepository.Dwellers.Count)];
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
        var rng = new Random();
        var d2 = new Dweller {
            FirstName = name,
            IsSupervisor = isSupervisor,
            Special_S = rng.Next(2, 6),
            Special_P = rng.Next(2, 6),
            Special_E = rng.Next(2, 6),
            Special_C = rng.Next(2, 6),
            Special_I = rng.Next(2, 6),
            Special_A = rng.Next(2, 6),
            Special_L = rng.Next(2, 6)
        };
        d2.CurrentHp = d2.MaxHp;
        return d2;
    }
}
