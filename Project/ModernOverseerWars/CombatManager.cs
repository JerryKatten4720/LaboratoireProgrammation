using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public class CombatManager {
    private readonly GameState _state;
    private readonly Random    _rng = new();

    public event Action<string>?             CombatEvent;

    public event Action<string, Panel, double, double>? DamagePopup;

    public CombatManager(GameState state) { _state = state; }

    public void ResolveCombatsOnMap() {
        for (int c = 0; c < _state.Map.Cols; c++)
            for (int r = 0; r < _state.Map.Rows; r++) {
                var tile = _state.Map.Get(c, r);
                if (tile.HasConflict) ResolveTile(tile);
            }
        CheckVictory();
    }

    private void ResolveTile(HexTile tile) {
        var p1 = tile.Player1Dwellers.Where(d => d.IsAlive).ToList();
        var p2 = tile.Player2Dwellers.Where(d => d.IsAlive).ToList();
        if (!p1.Any() || !p2.Any()) return;
        CombatEvent?.Invoke($"⚔ Combat at ({tile.Col},{tile.Row})!");

        while (p1.Any(d => d.IsAlive) && p2.Any(d => d.IsAlive)) {
            foreach (var atk in p1.Where(d => d.IsAlive).ToList()) {
                var tgt = p2.Where(d => d.IsAlive).OrderBy(_ => _rng.Next()).FirstOrDefault();
                if (tgt != null) Attack(atk, tgt, _state.Vault1, _state.Vault2);
            }
            foreach (var atk in p2.Where(d => d.IsAlive).ToList()) {
                var tgt = p1.Where(d => d.IsAlive).OrderBy(_ => _rng.Next()).FirstOrDefault();
                if (tgt != null) Attack(atk, tgt, _state.Vault2, _state.Vault1);
            }
        }

        foreach (var dead in p2.Where(d => !d.IsAlive).ToList()) {
            LootTransfer(dead, _state.Vault1); tile.Player2Dwellers.Remove(dead);
        }
        foreach (var dead in p1.Where(d => !d.IsAlive).ToList()) {
            LootTransfer(dead, _state.Vault2); tile.Player1Dwellers.Remove(dead);
        }
    }

    private void Attack(Dweller atk, Dweller tgt, Vault atkV, Vault defV) {
        int dmg  = (atk.EquippedWeapon?.Damage ?? 1) + (atk.Special_S / 2) + (atkV.HasReliableAim ? 1 : 0);
        int armor = (tgt.EquippedOutfit?.ArmorValue ?? 0) + (defV.HasFrankTheTank ? 1 : 0);
        dmg = Math.Max(1, dmg - armor);
        
        int critChance = (atk.EquippedWeapon is Weapon w && int.TryParse(w.CriticalChance, out int cc) ? cc : 5) + atk.Special_P;
        bool crit = _rng.Next(100) < critChance;
        bool fail = _rng.Next(100) < tgt.Special_C * 3;
        if (crit && !fail) dmg *= 2;

        tgt.CurrentHp -= dmg;
        string msg = crit && !fail
            ? $"💥 CRIT! {atk.Name}→{tgt.Name} -{dmg}❤"
            : $"{atk.Name}→{tgt.Name} -{dmg}❤";
        CombatEvent?.Invoke(msg);
        DamagePopup?.Invoke($"-{dmg} ❤", null!, 0, 0);

        if (tgt.CurrentHp <= 0) {
            bool flees = _rng.Next(100) < tgt.Special_A * 8;
            if (flees) { tgt.CurrentHp = 1; CombatEvent?.Invoke($"🏃 {tgt.Name} fled!"); }
            else CombatEvent?.Invoke($"💀 {tgt.Name} killed!");
        }
    }

    private void LootTransfer(Dweller dead, Vault winner) {
        if (dead.EquippedWeapon == null) return;
        var target = winner.Dwellers.FirstOrDefault(d => d.IsAlive && d.EquippedWeapon == null);
        if (target != null) {
            target.EquippedWeapon = dead.EquippedWeapon;
            CombatEvent?.Invoke($"🎁 {winner.OwnerName} looted {dead.EquippedWeapon.Name}!");
        }
    }

    private void CheckVictory() {
        bool p1SuperDead = !_state.Vault1.Dwellers.Any(d => d.IsSupervisor && d.IsAlive);
        bool p2SuperDead = !_state.Vault2.Dwellers.Any(d => d.IsSupervisor && d.IsAlive);
        if (p2SuperDead) { _state.Phase = GamePhase.GameOver; _state.WinnerName = _state.Player1.Pseudo; }
        else if (p1SuperDead) { _state.Phase = GamePhase.GameOver; _state.WinnerName = _state.Player2.Pseudo; }
    }
}

public static class DamagePopupHelper {
    public static void Spawn(string text, Canvas host, double x, double y, Color color) {
        var tb = new TextBlock {
            Text       = text,
            Foreground = new SolidColorBrush(color),
            FontSize   = 16,
            FontWeight = FontWeights.Bold,
        };
        Canvas.SetLeft(tb, x);
        Canvas.SetTop(tb,  y);
        host.Children.Add(tb);

        var moveAnim = new DoubleAnimation(y, y - 50, TimeSpan.FromMilliseconds(800)) {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        var fadeAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(800)) {
            BeginTime = TimeSpan.FromMilliseconds(200)
        };
        fadeAnim.Completed += (s, e) => host.Children.Remove(tb);
        tb.BeginAnimation(Canvas.TopProperty, moveAnim);
        tb.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
    }
}
