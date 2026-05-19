using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class MenuPopper : UserControl {
    public static readonly DependencyProperty MessageTextProperty =
        DependencyProperty.Register(nameof(MessageText), typeof(string), typeof(MenuPopper),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty OutlineBrushProperty =
        DependencyProperty.Register(nameof(OutlineBrush), typeof(Brush), typeof(MenuPopper),
            new PropertyMetadata(Brushes.Green));

    public MenuPopper() {
        InitializeComponent();
    }

    public string MessageText {
        get => (string)GetValue(MessageTextProperty);
        set => SetValue(MessageTextProperty, value);
    }

    public Brush OutlineBrush {
        get => (Brush)GetValue(OutlineBrushProperty);
        set => SetValue(OutlineBrushProperty, value);
    }

    public Action? OnYesConfirmed { get; set; }
    public Action? OnNoConfirmed { get; set; }

    private void YesButton_Click(object sender, RoutedEventArgs e) {
        OnYesConfirmed?.Invoke();
        ClosePopup();
    }

    private void NoButton_Click(object sender, RoutedEventArgs e) {
        OnNoConfirmed?.Invoke();
        ClosePopup();
    }

    private void ClosePopup() {
        DependencyObject current = this;
        while (current != null) {
            if (current is Panel panel) {
                panel.Children.Remove(this);
                return;
            }

            current = VisualTreeHelper.GetParent(current);
        }
    }
}