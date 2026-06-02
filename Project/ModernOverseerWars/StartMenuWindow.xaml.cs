using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public class ThemeViewModel {
    public PlayerTheme Theme        { get; init; } = null!;
    public Color       DisplayColor { get; init; }
    public static ThemeViewModel From(PlayerTheme t) {
        var c = (Color)ColorConverter.ConvertFromString(t.Color);
        return new() { Theme = t, DisplayColor = c };
    }
}

public partial class StartMenuWindow : Window {

    public PlayerProfile Profile1 { get; } = new() { Index = 0 };
    public PlayerProfile Profile2 { get; } = new() { Index = 1 };

    private int _p1ThemeIdx = 0;
    private int _p2ThemeIdx = 1;

    public StartMenuWindow() {
        InitializeComponent();
        var vms = PlayerThemes.All.Select(ThemeViewModel.From).ToList();
        P1Themes.ItemsSource = vms;
        P2Themes.ItemsSource = vms;
        P1Name.TextChanged += (s, e) => UpdatePreviews();
        P2Name.TextChanged += (s, e) => UpdatePreviews();
        UpdatePreviews();
    }

    private void OnP1ThemeClick(object s, System.Windows.Input.MouseButtonEventArgs e) {
        if (s is FrameworkElement el && el.Tag is ThemeViewModel vm) {
            int newIdx = PlayerThemes.All.ToList().IndexOf(vm.Theme);
            if (newIdx == _p2ThemeIdx) return;
            _p1ThemeIdx = newIdx;
            UpdatePreviews();
        }
    }
    private void OnP2ThemeClick(object s, System.Windows.Input.MouseButtonEventArgs e) {
        if (s is FrameworkElement el && el.Tag is ThemeViewModel vm) {
            int newIdx = PlayerThemes.All.ToList().IndexOf(vm.Theme);
            if (newIdx == _p1ThemeIdx) return;
            _p2ThemeIdx = newIdx;
            UpdatePreviews();
        }
    }

    private void UpdatePreviews() {
        var t1 = PlayerThemes.All[_p1ThemeIdx];
        var t2 = PlayerThemes.All[_p2ThemeIdx];
        P1Preview.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(t1.BgColor));
        P1PreviewText.Text   = P1Name.Text;
        P1PreviewText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(t1.Color));
        P2Preview.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(t2.BgColor));
        P2PreviewText.Text   = P2Name.Text;
        P2PreviewText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(t2.Color));

        RefreshDotSelection(P1Themes, _p1ThemeIdx, _p2ThemeIdx);
        RefreshDotSelection(P2Themes, _p2ThemeIdx, _p1ThemeIdx);
    }

    private static void RefreshDotSelection(ItemsControl list, int selectedIdx, int otherIdx) {
        list.UpdateLayout();
        for (int i = 0; i < list.Items.Count; i++) {
            var container = list.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
            if (container == null) continue;
            var border = FindChild<Border>(container);
            if (border == null) continue;
            var check  = FindChild<TextBlock>(border);
            if (check  == null) continue;
            check.Visibility = (i == selectedIdx) ? Visibility.Visible : Visibility.Collapsed;
            border.BorderThickness = (i == selectedIdx) ? new Thickness(2) : new Thickness(0);
            border.BorderBrush = new SolidColorBrush(Colors.White);
            container.Opacity = (i == otherIdx) ? 0.3 : 1.0;
        }
    }

    private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++) {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T t) return t;
            var result = FindChild<T>(child);
            if (result != null) return result;
        }
        return null;
    }

    private void OnStart(object s, RoutedEventArgs e) {
        Profile1.Pseudo = string.IsNullOrWhiteSpace(P1Name.Text) ? "Player 1" : P1Name.Text;
        Profile1.Theme  = PlayerThemes.All[_p1ThemeIdx];
        Profile2.Pseudo = string.IsNullOrWhiteSpace(P2Name.Text) ? "Player 2" : P2Name.Text;
        Profile2.Theme  = PlayerThemes.All[_p2ThemeIdx];
        DialogResult = true;
    }
}
