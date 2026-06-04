namespace LaboratoireProgrammation.Project.ModernOverseerWars.Controllers;

using LaboratoireProgrammation.Project.ModernOverseerWars.Models;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;

public class EquipmentService {
    private readonly GameState _state;

    public EquipmentService(GameState state) {
        _state = state;
    }

    public void EquipWeapon(Dweller dweller, Weapon weapon) {
        if (dweller.EquippedWeapon != null) {
            UnequipWeapon(dweller);
        }
        dweller.EquippedWeapon = weapon;
        _state.AddLog($"{dweller.Name} equipped {weapon.Name}");
    }

    public void UnequipWeapon(Dweller dweller) {
        if (dweller.EquippedWeapon != null) {
            _state.AddLog($"{dweller.Name} unequipped {dweller.EquippedWeapon.Name}");
            dweller.EquippedWeapon = null;
        }
    }

    public void EquipOutfit(Dweller dweller, Outfit outfit) {
        if (dweller.EquippedOutfit != null) {
            UnequipOutfit(dweller);
        }
        dweller.EquippedOutfit = outfit;
        _state.AddLog($"{dweller.Name} equipped {outfit.Name}");
    }

    public void UnequipOutfit(Dweller dweller) {
        if (dweller.EquippedOutfit != null) {
            _state.AddLog($"{dweller.Name} unequipped {dweller.EquippedOutfit.Name}");
            dweller.EquippedOutfit = null;
        }
    }
}
