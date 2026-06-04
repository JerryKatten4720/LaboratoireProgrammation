namespace LaboratoireProgrammation.Project.ModernOverseerWars.Views.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

public static class DamagePopupHelper {
    public static void Spawn(string text, Canvas host, double x, double y, Color color) {
        var tb = new TextBlock {
            Text = text,
            Foreground = new SolidColorBrush(color),
            FontSize = 25,
            FontWeight = FontWeights.Bold,
        };
        Canvas.SetLeft(tb, x);
        Canvas.SetTop(tb, y);
        host.Children.Add(tb);

        var moveAnim = new DoubleAnimation(y, y - 50, TimeSpan.FromMilliseconds(1500)) {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        var fadeAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(1500)) {
            BeginTime = TimeSpan.FromMilliseconds(375)
        };
        fadeAnim.Completed += (s, e) => {
            host.Children.Remove(tb);
        };
        
        tb.BeginAnimation(Canvas.TopProperty, moveAnim);
        tb.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
    }
}
