namespace LaboratoireProgrammation.Project.ModernOverseerWars.Services;

using System;
using System.Linq;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Map;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Helpers;

public class CombatService {
    private readonly GameState _state;

    public event Action<string>? CombatEvent;
    public event Action<string>? DamagePopup;

    public CombatService(GameState state) {
        _state = state;
    }

    public void ResolveCombatsOnMap() {
        for (int c = 0; c < _state.Map.Cols; c++) {
            for (int r = 0; r < _state.Map.Rows; r++) {
                var tile = _state.Map.Get(c, r);
                if (tile.HasConflict) {
                    ResolveTile(tile);
                }
            }
        }
        CheckVictory();
    }

    private void ResolveTile(HexTile tile) {
        var p1 = tile.Player1Dwellers.Where(d => d.IsAlive).ToList();
        var p2 = tile.Player2Dwellers.Where(d => d.IsAlive).ToList();
        if (!p1.Any() || !p2.Any()) return;
        
        CombatEvent?.Invoke($"⚔️ Combat at ({tile.Col},{tile.Row})!");

        foreach (var atk in p1.Where(d => d.IsAlive).ToList()) {
            var tgt = p2.Where(d => d.IsAlive).OrderBy(_ => RandomProvider.Next(1000)).FirstOrDefault();
            if (tgt != null) Attack(atk, tgt, _state.Vault1, _state.Vault2);
        }
        foreach (var atk in p2.Where(d => d.IsAlive).ToList()) {
            var tgt = p1.Where(d => d.IsAlive).OrderBy(_ => RandomProvider.Next(1000)).FirstOrDefault();
            if (tgt != null) Attack(atk, tgt, _state.Vault2, _state.Vault1);
        }

        foreach (var dead in p2.Where(d => !d.IsAlive).ToList()) {
            LootTransfer(dead, _state.Vault1);
            tile.Player2Dwellers.Remove(dead);
        }
        foreach (var dead in p1.Where(d => !d.IsAlive).ToList()) {
            LootTransfer(dead, _state.Vault2);
            tile.Player1Dwellers.Remove(dead);
        }
    }

    private void Attack(Dweller atk, Dweller tgt, Vault atkV, Vault defV) {
        int dmg = (atk.EquippedWeapon?.Damage ?? 1) + (atk.Special_S / 2) + (atkV.HasReliableAim ? 1 : 0);
        int armor = (tgt.EquippedOutfit?.ArmorValue ?? 0) + (defV.HasFrankTheTank ? 1 : 0);
        dmg = Math.Max(1, dmg - armor);
        
        int critChance = (atk.EquippedWeapon is Weapon w && int.TryParse(w.CriticalChance, out int cc) ? cc : 5) + atk.Special_P;
        bool crit = RandomProvider.Next(100) < critChance;
        bool fail = RandomProvider.Next(100) < tgt.Special_C * 3;
        
        if (crit && !fail) dmg *= 2;

        tgt.CurrentHp -= dmg;
        string msg = crit && !fail
            ? $"🎯 CRIT! {atk.Name}🔫{tgt.Name} -{dmg}❤️"
            : $"{atk.Name}🔫{tgt.Name} -{dmg}❤️";
            
        CombatEvent?.Invoke(msg);
        DamagePopup?.Invoke($"-{dmg} ❤️");

        if (tgt.CurrentHp <= 0) {
            bool flees = RandomProvider.Next(100) < tgt.Special_A * 8;
            if (flees) {
                tgt.CurrentHp = 1;
                CombatEvent?.Invoke($"🏃‍♂️ {tgt.Name} fled!");
            }
            else {
                CombatEvent?.Invoke($"☠️ {tgt.Name} killed!");
            }
        }
    }

    private void LootTransfer(Dweller dead, Vault winner) {
        if (dead.EquippedWeapon == null) return;
        var target = winner.Dwellers.FirstOrDefault(d => d.IsAlive && d.EquippedWeapon == null);
        if (target != null) {
            target.EquippedWeapon = dead.EquippedWeapon;
            CombatEvent?.Invoke($"🎒 {winner.OwnerName} looted {dead.EquippedWeapon.Name}!");
        }
    }

    private void CheckVictory() {
        bool p1SuperDead = !_state.Vault1.Dwellers.Any(d => d.IsSupervisor && d.IsAlive);
        bool p2SuperDead = !_state.Vault2.Dwellers.Any(d => d.IsSupervisor && d.IsAlive);
        if (p2SuperDead) {
            _state.Phase = GamePhase.GameOver;
            _state.WinnerName = _state.Player1.Pseudo;
        }
        else if (p1SuperDead) {
            _state.Phase = GamePhase.GameOver;
            _state.WinnerName = _state.Player2.Pseudo;
        }
    }
}
