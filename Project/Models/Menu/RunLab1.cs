using System.Windows;
using System.Windows.Media;
using LaboratoireProgrammation.Project.Helpers;
using LaboratoireProgrammation.Project.Services;
using LaboratoireProgrammation.Project.ViewModels.Menu;
using Color = System.Drawing.Color;

namespace LaboratoireProgrammation.Project.Models.Menu;

public class RunLab1 {
    public static async void Run(MainWindow win) {
        win.OutputBox.Document.Blocks.Clear();

        var smokeDensity = win.VaultShader.SmokeDensity;
        var glitchIntensity = win.VaultShader.GlitchIntensity;
        var phosphorDecay = win.VaultShader.PhosphorDecay;
        var burnIn = win.VaultShader.BurnInIntensity;
        var constrast = win.VaultShader.Contrast;
        var vaultBrightness = win.VaultShader.Brightness;
        var tint = win.VaultShader.TintColor;

        win.VaultShader.GlitchIntensity *= 5;
        win.VaultShader.SmokeDensity = 1.01;
        win.VaultShader.Brightness = 1.1;
        win.VaultShader.Contrast *= 1.04;
        win.VaultShader.PhosphorDecay *= 1.5;
        win.VaultShader.BurnInIntensity *= 2;

        win.VaultShader.TintColor = Color.FromArgb(5, 255, 0, 0);
        win.OutputBox.Visibility = Visibility.Collapsed;
        win.TopText.Visibility = Visibility.Collapsed;
        win.InputBox.Visibility = Visibility.Collapsed;

        var width = win.ActualWidth;
        var fontMult = width >= 1600 ? 1.0 : width >= 1200 ? 0.8 : 0.6;

        var inspirationBlock = TerminalDisplay.CreateTitleBlock("Laboratoire - 1", 76 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#eb4634")));
        win.TerminalOutputPanel.Children.Add(inspirationBlock);

        var descriptionBlock = TerminalDisplay.CreateTitleBlock("< ! > LAB :// { - 03.02.26 - } :\\ < ! >", 50 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#eb4634")));
        win.TerminalOutputPanel.Children.Add(descriptionBlock);

        if (ConsoleBehavior.SpeedLoad) await Task.Delay(200);
        else await Task.Delay(2000);

        var authorBlock = TerminalDisplay.CreateTitleBlock("anto.cldl", 200 * fontMult, new SolidColorBrush(ColorHelper.HexToColor("#fff1f0")));
        win.TerminalOutputPanel.Children.Add(authorBlock);

        if (ConsoleBehavior.SpeedLoad) await Task.Delay(200);
        else await Task.Delay(2000);

        win.TerminalOutputPanel.Children.Clear();

        win.InputBox.Visibility = Visibility.Visible;
        win.TopText.Visibility = Visibility.Visible;

        win.VaultShader.GlitchIntensity = glitchIntensity;
        win.VaultShader.SmokeDensity = smokeDensity;
        win.VaultShader.Brightness = vaultBrightness;
        win.VaultShader.PhosphorDecay = phosphorDecay;
        win.VaultShader.BurnInIntensity = burnIn;
        win.VaultShader.Contrast = constrast;
        win.VaultShader.TintColor = tint;

        win.Labo1B.Visibility = Visibility.Visible;
    }
}