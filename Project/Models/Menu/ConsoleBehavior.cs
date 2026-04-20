using System.Windows.Media;
using LaboratoireProgrammation.Project.Services;
using LaboratoireProgrammation.Project.ViewModels.Menu;

namespace LaboratoireProgrammation.Project.Models.Menu;

public class ConsoleBehavior {
    private readonly List<string> _commandHistory = new();
    private int _historyIndex = -1;
    public Action? OnClearInputRequest;

    public Action<string, Color?>? OnOutputRequest;
    public Action<string, Color?>? OnOutputSameLineRequest;
    public static bool SpeedLoad { get; set; } = true;
    public static bool Babymode { get; set; }
    public static bool MemfyMode { get; set; }
    public static bool IsAdjustingSize { get; set; }

    public async Task ProcessInput(string cmd, MainWindow context) {
        if (string.IsNullOrWhiteSpace(cmd)) return;

        _commandHistory.Add(cmd);
        _historyIndex = _commandHistory.Count;

        if (MemfyMode) {
            await HandleMemfyConversation(cmd);
            return;
        }


        CommandsProcessor.ProcessCommand(cmd.ToLower(), context);
        OnClearInputRequest?.Invoke();
    }

    private async Task HandleMemfyConversation(string cmd) {
        OnClearInputRequest?.Invoke();

        var memfyAi = new MemfyAI();
        var fullResponse = "";

        OnOutputRequest?.Invoke("", null);
        OnOutputRequest?.Invoke("", null);
        OnOutputSameLineRequest?.Invoke("[MemfyAI] : ", null);

        await foreach (var chunk in memfyAi.AskQuestionAsync(cmd)) {
            fullResponse += chunk;
            var cleanChunk = chunk.Replace("[$exit$token$]", "").Replace("$exit$token$", "");
            OnOutputSameLineRequest?.Invoke(cleanChunk, null);
        }

        if (fullResponse.ToLower().Contains("$exit$token$")) {
            OnOutputRequest?.Invoke("", null);
            OnOutputRequest?.Invoke("• [MemfyAI] has left the conversation", null);
            MemfyMode = false;
        }
    }

    public string? GetHistory(int direction) {
        if (_commandHistory.Count == 0) return null;

        _historyIndex += direction;
        _historyIndex = Math.Clamp(_historyIndex, 0, _commandHistory.Count);

        if (_historyIndex < _commandHistory.Count) return _commandHistory[_historyIndex];

        return string.Empty;
    }

    public void ToggleBabyMode() {
        Babymode = !Babymode;
    }

    public bool GetBabyMode() {
        return Babymode;
    }

    public bool GetSpeedLoad() {
        return SpeedLoad;
    }

    public bool GetMemfyMode() {
        return MemfyMode;
    }

    public bool GetIsAdjustingSize() {
        return IsAdjustingSize;
    }

    public void SetBabyMode(bool value) {
        Babymode = value;
    }

    public void SetSpeedLoad(bool value) {
        SpeedLoad = value;
    }

    public void SetMemfyMode(bool value) {
        MemfyMode = value;
    }

    public void SetIsAdjustingSize(bool value) {
        IsAdjustingSize = value;
    }
}