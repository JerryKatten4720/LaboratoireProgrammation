using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LaboratoireProgrammation.Project.Helpers;

namespace LaboratoireProgrammation.Project.Views.Exercices;

public delegate double TargetFunction(double x);

public partial class Exo___10 : UserControl {
    public Exo___10() {
        InitializeComponent();
        WriteLog("ROB.CO TERMINAL INITIALIZED. READY FOR NUMERICAL INTEGRATION.");
    }

    private void OnStandardPolyClick(object sender, RoutedEventArgs e) {
        if (!TryGetInputs(out var a, out var b, out var n)) return;

        WriteLog($"\n--- DEB STD POLYNOMIAL [a={a}, b={b}, base_n={n}] ---");

        int[] multipliers = { 1, 2, 4, 8, 16 };
        var datasets = new List<(int Steps, double Area)>();

        foreach (var mult in multipliers) {
            var steps = n * mult;
            var h = (b - a) / steps;
            double area = 0;

            for (var i = 0; i < steps; i++) {
                var x1 = a + i * h;
                var x2 = a + (i + 1) * h;

                var y1 = Math.Pow(x1, 2) + 2;
                var y2 = Math.Pow(x2, 2) + 2;

                area += CalculateTrapezoidArea(h, y1, y2);
            }

            WriteLog($"[n={steps}] Area = {area:F6}");
            datasets.Add((steps, area));
        }

        RenderHistogram(datasets);
    }

    private void OnStandardTrigClick(object sender, RoutedEventArgs e) {
        if (!TryGetInputs(out var a, out var b, out var n)) return;

        WriteLog($"\n--- DEB STD TRIGONOMETRIC [a={a}, b={b}, base_n={n}] ---");

        int[] multipliers = { 1, 2, 4, 8, 16 };
        var datasets = new List<(int Steps, double Area)>();

        foreach (var mult in multipliers) {
            var steps = n * mult;
            var h = (b - a) / steps;
            double area = 0;

            for (var i = 0; i < steps; i++) {
                var x1 = a + i * h;
                var x2 = a + (i + 1) * h;

                var y1 = Math.Sin(x1);
                var y2 = Math.Sin(x2);

                area += CalculateTrapezoidArea(h, y1, y2);
            }

            WriteLog($"[n={steps}] Area = {area:F6}");
            datasets.Add((steps, area));
        }

        RenderHistogram(datasets);
    }

    private void OnDelegatePolyClick(object sender, RoutedEventArgs e) {
        RunIntegrationSuite(x => Math.Pow(x, 2) + 2, "PTR POLYNOMIAL", sender, e);
    }

    private void OnDelegateTrigClick(object sender, RoutedEventArgs e) {
        RunIntegrationSuite(Math.Sin, "PTR TRIGONOMETRIC", sender, e);
    }

    private void OnDelegateExpClick(object sender, RoutedEventArgs e) {
        RunIntegrationSuite(Math.Exp, "PTR EXPONENTIAL", sender, e);
    }

    private void OnClearLogsClick(object sender, RoutedEventArgs e) {
        LbxIntegrationLogs.Items.Clear();
        GridHistogram.Children.Clear();
        WriteLog("TERMINAL CLEARED.");
    }

    private void OnHelpClick(object sender, RoutedEventArgs e) {
        if (sender is Button btn && btn.Tag is string tag) {
            TxtExplanation.Text = GetExplanation(tag);
            OverlayDialog.Visibility = Visibility.Visible;
        }
    }

    private void OnCloseDialogClick(object sender, RoutedEventArgs e) {
        OverlayDialog.Visibility = Visibility.Collapsed;
    }

    private double ExecuteTrapezoidalRule(TargetFunction f, double left, double right, int intervals) {
        var h = (right - left) / intervals;
        double totalArea = 0;

        for (var i = 0; i < intervals; i++) {
            var x1 = left + i * h;
            var x2 = left + (i + 1) * h;

            totalArea += CalculateTrapezoidArea(h, f(x1), f(x2));
        }

        return totalArea;
    }

    private void RunIntegrationSuite(TargetFunction f, string title, object sender, RoutedEventArgs e) {
        if (!TryGetInputs(out var a, out var b, out var n)) return;

        WriteLog($"\n--- DEB {title} [a={a}, b={b}, base_n={n}] ---");

        int[] multipliers = { 1, 2, 4, 8, 16 };
        var datasets = new List<(int Steps, double Area)>();

        foreach (var mult in multipliers) {
            var steps = n * mult;
            var result = ExecuteTrapezoidalRule(f, a, b, steps);
            WriteLog($"[n={steps}] Area = {result:F6}");
            datasets.Add((steps, result));
        }

        RenderHistogram(datasets);
    }

    private void RenderHistogram(List<(int Steps, double Area)> datasets) {
        GridHistogram.Children.Clear();
        if (datasets == null || datasets.Count == 0) return;

        double maxArea = datasets.Max(d => d.Area);
        if (maxArea <= 0) maxArea = 1;

        foreach (var data in datasets) {
            var barContainer = new Grid { VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(6, 0, 6, 0) };
            barContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            barContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var calculatedHeight = (data.Area / maxArea) * 400;
            
            var barVisual = new Border {
                Background = ColorHelper.FancyTextBrush,
                Height = Math.Max(calculatedHeight, 4),
                VerticalAlignment = VerticalAlignment.Bottom,
                CornerRadius = new CornerRadius(2, 2, 0, 0),
                Opacity = 0.75
            };

            var barLabel = new TextBlock {
                Text = $"n={data.Steps}\n{data.Area:F4}",
                FontFamily = new FontFamily("pack://application:,,,/Assets/fonts/#ubuntu mono"),
                FontSize = 11,
                Foreground = ColorHelper.FancyTextBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0),
                Opacity = 0.8
            };

            Grid.SetRow(barVisual, 0);
            Grid.SetRow(barLabel, 1);

            barContainer.Children.Add(barVisual);
            barContainer.Children.Add(barLabel);

            GridHistogram.Children.Add(barContainer);
        }
    }

    private double CalculateTrapezoidArea(double height, double base1, double base2) {
        return (base1 + base2) * height / 2.0;
    }

    private bool TryGetInputs(out double a, out double b, out int n) {
        a = 0;
        b = 0;
        n = 0;

        if (!double.TryParse(TxtLowerBound.Text, out a)) {
            WriteLog("ERR: INVALID LOWER BOUND.");
            return false;
        }

        if (!double.TryParse(TxtUpperBound.Text, out b)) {
            WriteLog("ERR: INVALID UPPER BOUND.");
            return false;
        }

        if (!int.TryParse(TxtDivisions.Text, out n) || n < 1) {
            WriteLog("ERR: INVALID SUB-INTERVALS (MUST BE >= 1).");
            return false;
        }

        return true;
    }

    private void WriteLog(string message) {
        LbxIntegrationLogs.Items.Add(message);
        LbxIntegrationLogs.ScrollIntoView(LbxIntegrationLogs.Items[^1]);
    }

    private string GetExplanation(string key) {
        return key switch {
            "POLY_STD" =>
                @"Je calcule ici l'aire qui se trouve sous la courbe de l'équation $f(x) = x^2 + 2$.  Imaginez que je découpe cette surface en plusieurs petites tranches verticales, qui ont toutes la forme d'un trapèze. Pour trouver l'aire totale, j'additionne simplement l'aire de chacun de ces petits trapèzes. 

Le problème majeur de cette approche, c'est que la formule mathématique $x^2 + 2$ est directement collée et coincée à l'intérieur de mon mécanisme de calcul. Si je veux calculer l'aire d'une autre courbe demain, je serai obligé de copier-coller tout le code de la méthode des trapèzes et de modifier manuellement la formule à l'intérieur. C'est ce qu'on appelle du code 'en dur', et c'est très peu pratique à long terme car cela crée beaucoup de répétitions inutiles.",

            "TRIG_STD" =>
                @"Dans ce module, je fais exactement le même travail de découpage, mais cette fois-ci pour approcher la valeur de l'aire sous la courbe d'une vague : la fonction $\sin(x)$. 

Comme pour le polynôme précédent, j'ai dû écrire une toute nouvelle fonction dans mon programme, et y réécrire l'intégralité de la logique mathématique qui permet de calculer la base et la hauteur de mes trapèzes. L'équation de la fonction sinus est elle aussi prisonnière de ma boucle de calcul. Même si le résultat mathématique est parfaitement exact pour cette courbe précise, cette façon de programmer est extrêmement lourde et difficile à maintenir. Cela prouve bien qu'il nous manque un outil pour séparer la 'méthode de découpage en trapèzes' de la 'forme de la courbe'.",

            "POLY_PTR" =>
                @"C'est ici que la magie des 'Delegates' (que l'on peut voir comme des pointeurs de fonction) entre en jeu ! Au lieu de coincer la formule dans mon moteur de calcul, j'util use un delegate pour dire à mon programme : 'Voici la recette générique pour découper des trapèzes, et voici la courbe spécifique sur laquelle tu dois travailler aujourd'hui'. 

Je définis ma courbe $f(x) = x^2 + 2$ totalement à part, et je la passe comme un simple paramètre à ma machine à calculer les intégrales. Le code qui s'occupe de faire la somme des trapèzes n'a plus besoin de savoir quelle est la formule exacte. Il se contente d'appeler la fonction que je lui ai montrée du doigt. Mon code devient ainsi totalement générique, propre et réutilisable à l'infini !",

            "TRIG_PTR" =>
                @"En exploitant cette nouvelle architecture très flexible basée sur les delegates, je peux calculer l'intégrale de la fonction trigonométrique $\sin(x)$ de manière incroyablement élégante. 

Je prends simplement la fonction sinus qui existe déjà par défaut dans le système de l'ordinateur, et je l'injecte directement dans mon moteur de calcul des trapèzes. Mon algorithme évalue la hauteur de la courbe sans même se soucier de savoir si c'est une vague, une ligne droite ou une courbe complexe. Je n'ai pas eu besoin d'écrire une seule ligne de code supplémentaire pour réinventer la méthode des trapèzes. C'est le but ultime de la programmation : écrire un outil de calcul une seule et unique fois, et l'utiliser avec une infinité de données différentes.",

            "EXP_PTR" =>
                @"Ce dernier module expérimental est la preuve ultime que notre moteur de calcul est devenu parfaitement modulable et tout-terrain.  En une seule petite ligne de code, j'ai pu ajouter le support pour calculer l'aire sous une courbe à croissance rapide, la fonction exponentielle $f(x) = e^x$. 

Je fournis juste l'opérateur exponentiel à mon delegate, et le tour est joué. Le moteur d'intégration prend cette nouvelle fonction, la découpe sagement en trapèzes, et me recrache le résultat sans jamais broncher ni bugger. Je pourrais faire exactement la même chose avec n'importe quelle autre formule mathématique complexe, pouvan t ainsi que mon schéma numérique est devenu totalement indépendant et universel !",

            _ =>
                "Je ne dispose d'aucune archive ni d'aucune donnée analytique pour expliquer le comportement de ce module spécifique."
        };
    }
}