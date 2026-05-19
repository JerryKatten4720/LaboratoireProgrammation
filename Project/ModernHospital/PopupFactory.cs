using System.Windows.Media;

namespace LaboratoireProgrammation.Project.ModernHospital;

public static class PopupFactory {
    public static MenuPopper CreateConfirmationPopup(string message, string hexColor, Action yesAction,
        Action? noAction = null) {
        var converter = new BrushConverter();
        var brush = (Brush)converter.ConvertFromString(hexColor);

        return new MenuPopper {
            MessageText = message,
            OutlineBrush = brush,
            OnYesConfirmed = yesAction,
            OnNoConfirmed = noAction
        };
    }
}