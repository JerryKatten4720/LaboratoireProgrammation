using System.Windows.Threading;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public class TurnManager {
    private readonly GameState _state;
    private readonly DispatcherTimer _timer;
    public int SecondsRemaining    { get; private set; } = 60;
    public int TurnDurationSeconds { get; set; }        = 60;

    public event Action? TurnTick;
    public event Action? TurnExpired;

    public TurnManager(GameState state) {
        _state = state;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) => {
            SecondsRemaining--;
            TurnTick?.Invoke();
            if (SecondsRemaining <= 0) TurnExpired?.Invoke();
        };
    }

    public void StartTurn(bool isPlayer1) {
        SecondsRemaining = TurnDurationSeconds;
        _state.Phase = isPlayer1 ? GamePhase.PlayerTurn : GamePhase.Player2Turn;
        _state.ActiveVault.ResetActionPoints();
        _timer.Start();
    }

    public void EndTurn() {
        _timer.Stop();

        _state.ActiveVault.ProduceResources();
        bool wasP1 = _state.IsPlayer1Turn;
        if (wasP1) {
            _state.Phase = GamePhase.Player2Turn;
            _state.Vault2.ResetActionPoints();
        } else {
            _state.Phase = GamePhase.PlayerTurn;
            _state.Vault1.ResetActionPoints();
            _state.TurnNumber++;
        }
        SecondsRemaining = TurnDurationSeconds;
        _timer.Start();
        TurnTick?.Invoke();
    }
}
