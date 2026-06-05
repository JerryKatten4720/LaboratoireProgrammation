namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;

using System;
using System.Linq;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data;

public class Dweller : IDweller {
    public Guid Id { get; set; } = Guid.NewGuid();
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
    
    public int MaxHp => GameConfigRepository.Config.DwellerBaseHp + Special_E * GameConfigRepository.Config.DwellerHpMultE;
    
    private int _currentHp;
    public int CurrentHp {
        get => _currentHp;
        set {
            int old = _currentHp;
            _currentHp = value;
            if (_currentHp < old) {
                Damaged?.Invoke(old - _currentHp);
            }
        }
    }
    public event Action<int>? Damaged;

    public bool IsSupervisor { get; set; } = false;
    public IWeapon? EquippedWeapon { get; set; }
    public IOutfit? EquippedOutfit { get; set; }
    public int AttackDamage => Special_S + (EquippedWeapon?.Damage ?? 0);
    public bool IsAlive => _currentHp > 0;
    public string Texture { get; set; } = "dweller.png";

    public Dweller() { _currentHp = GameConfigRepository.Config.DwellerStartingHp; }
}
