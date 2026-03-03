using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using LaboratoireProgrammation.Project.Helpers;
using LaboratoireProgrammation.Project.Models.Exercices;
using LaboratoireProgrammation.Project.Services;
using LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars;
using LaboratoireProgrammation.Project.ViewModels.Menu;

namespace LaboratoireProgrammation.Project.Models.Menu {

    public class MenuLoaders {

        public enum IntroStyle { TypewriterGlitch, FadeOnly }

        private static bool _isAnimating = false;
        private static bool _skipIntro = false;

        private static SolidColorBrush Brush(string hex) {
            var b = new SolidColorBrush(ColorHelper.HexToColor(hex));
            b.Freeze();
            return b;
        }

        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // [ - Colors - ]
        
        private static SolidColorBrush _exo1_title = Brush("FFC000");
        private static SolidColorBrush _exo1_sub = Brush("FFE170");
        private static SolidColorBrush _exo1_author = Brush("FFFFFF");

        private static SolidColorBrush _exo2_title = Brush("FF4242");
        private static SolidColorBrush _exo2_sub = Brush("FF9191");
        private static SolidColorBrush _exo2_author = Brush("FFFFFF");

        private static SolidColorBrush _exo3_title = Brush("4AFF4D");
        private static SolidColorBrush _exo3_sub = Brush("75FF77");
        private static SolidColorBrush _exo3_author = Brush("FFFFFF");

        private static SolidColorBrush _exo4_title = Brush("294DFF");
        private static SolidColorBrush _exo4_sub = Brush("788DFF");
        private static SolidColorBrush _exo4_author = Brush("FFFFFF");

        private static SolidColorBrush _exo5_title = Brush("FD47FF");
        private static SolidColorBrush _exo5_sub = Brush("FD91FF");
        private static SolidColorBrush _exo5_author = Brush("FFFFFF");
        
        private static SolidColorBrush _exo6_title = Brush("51AD64");
        private static SolidColorBrush _exo6_sub = Brush("77B582");
        private static SolidColorBrush _exo6_author = Brush("FFFFFF");

        private static SolidColorBrush _lab1_title = Brush("FD47FF");
        private static SolidColorBrush _lab1_sub = Brush("FD91FF");
        private static SolidColorBrush _lab1_author = Brush("FFFFFF");

        private static SolidColorBrush _main_title = Brush("872020");
        private static SolidColorBrush _main_sub = Brush("A63C3C");
        private static SolidColorBrush _main_author = Brush("D3DEDD");

        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // [ - Fonts - ]
        private static FontFamily _fontTitle = new FontFamily(new Uri("pack://application:,,,/"), "./Assets/fonts/#bebas neue");
        private static FontFamily _fontSub = new FontFamily(new Uri("pack://application:,,,/"), "./Assets/fonts/#bebas neue");
        private static FontFamily _fontAuthor = new FontFamily(new Uri("pack://application:,,,/"), "./Assets/fonts/#bebas neue");

        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // [ - Texts - ]
        private static string _title_exo1 = "Exercice [ 1 ]";
        private static string _title_exo2 = "Exercice [ 2 ]";
        private static string _title_exo3 = "Exercice [ 3 ]";
        private static string _title_exo4 = "Exercice [ 4 ]";
        private static string _title_exo5 = "Exercice [ 5 ]";
        private static string _title_exo6 = "Exercice [ 6 ]";
        private static string _title_lab1 = "Laboratoire [ 1 ]";
        private static string _title_main = "TERMINAL ROB:CO";

        private static string _sub_exo1 = ": // DATE : FEB . 2026  //  BOUTONS ET IMAGES";
        private static string _sub_exo2 = ": // DATE : FEB . 2026  //  TRANSFERT DE FICHIERS";
        private static string _sub_exo3 = ": // DATE : FEB . 2026  //  GESTIONNAIRE D'EMPLOYÉS";
        private static string _sub_exo4 = ": // DATE : FEB . 2026  //  SIMULATION.VIRUS";
        private static string _sub_exo5 = ": // DATE : FEB . 2026  //  ÉDITEUR DE TEXTE";
        private static string _sub_exo6 = ": // DATE : MAR . 2026  //  SPIROGRAPHIE";
        private static string _sub_lab1 = ": // DATE : FEB . 2026  //  MISE EN PRATIQUE";
        private static string _sub_main = "//: WELCOME -  USER ://";

        private static string _author = "@ anto.cldl";
        private static string _author2 = "MADE.BY :// @ anto.cldl";

        private static readonly Random _rng = new Random();

        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // [ - Utilitaries - ]
        public static void InitializeVariables(MainWindow win) {
        }

        private static void HideEverything(MainWindow win) {
            win.OutputBox.Visibility = Visibility.Collapsed;
            win.TopText.Visibility = Visibility.Collapsed;
            win.InputBox.Visibility = Visibility.Collapsed;
            win.InputBoxIndicator.Visibility = Visibility.Collapsed;
            win.VisualMode.Visibility = Visibility.Collapsed;
            win.MainMenuButton.Visibility = Visibility.Collapsed;
            win.TopText.Visibility = Visibility.Collapsed;
            win.WinBtnClose.Visibility = Visibility.Collapsed;
            win.WinBtnMaximize.Visibility = Visibility.Collapsed;
            win.WinBtnMinimize.Visibility = Visibility.Collapsed;
        }
    
        private static void RestoreEverything(MainWindow win) {
            win.OutputBox.Visibility = Visibility.Visible;
            win.TopText.Visibility = Visibility.Visible;
            win.InputBox.Visibility = Visibility.Visible;
            win.InputBoxIndicator.Visibility = Visibility.Visible;
            win.VisualMode.Visibility = Visibility.Visible;
            win.MainMenuButton.Visibility = Visibility.Visible;
            win.TopText.Visibility = Visibility.Visible;
            win.WinBtnClose.Visibility = Visibility.Visible;
            win.WinBtnMaximize.Visibility = Visibility.Visible;
            win.WinBtnMinimize.Visibility = Visibility.Visible;
        }

        private static string GenerateHexNoise(int length = 48) {
            const string chars = "0123456789ABCDEF .:";
            var sb = new System.Text.StringBuilder(length);
            for (int i = 0; i < length; i++) {
                sb.Append(chars[_rng.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        private static async Task Wait(int ms) {
            int elapsed = 0;
            while (elapsed < ms && !_skipIntro) {
                await Task.Delay(16);
                elapsed += 16;
            }
        }

        private static void HandleInputSkip(object sender, InputEventArgs e) {
            _skipIntro = true;
        }

        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // [ - Intro Helpers - ]
        private static async Task ShowNoiseBlast(MainWindow win, SolidColorBrush accentBrush, int lines = 4) {
            var noiseBlocks = new TextBlock[lines];
            for (int i = 0; i < lines; i++) {
                
                if (_skipIntro) { _isAnimating = false; break; }
                
                var unfrozen = new SolidColorBrush(accentBrush.Color);
                unfrozen.Opacity = 0.2 + _rng.NextDouble() * 0.25;
                
                var tb = new TextBlock {
                    Text = GenerateHexNoise(52),
                    FontSize = 20,
                    FontFamily = _fontSub,
                    Foreground = unfrozen,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                };
                
                win.TerminalOutputPanel.Children.Add(tb);
                noiseBlocks[i] = tb;
                await Task.Delay(30);
            }
            await Wait(120);
            foreach (var tb in noiseBlocks) {
                if (tb != null) { win.TerminalOutputPanel.Children.Remove(tb); }
            }
        }

        private static async Task ShowBootLines(MainWindow win, SolidColorBrush accentBrush) {
            string[] bootLines = {
                "> INIT SEQUENCE . . . . . . . [ OK ]",
                "> LOADING MODULE  . . . . . . [ OK ]",
                "> CHECKSUM VERIFY . . . . . . [ " + GenerateHexNoise(8).Replace(" ", "").Substring(0, 8) + " ]",
            };
            foreach (var line in bootLines) {
                
                if (_skipIntro) { _isAnimating = false; break; }
                
                var tb = new TextBlock {
                    Text = line,
                    FontSize = 20,
                    FontFamily = _fontSub,
                    Foreground = accentBrush,
                    Opacity = 0,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                };
                win.TerminalOutputPanel.Children.Add(tb);
                FadeIn(tb, 150);
                await Wait(220);
            }
            await Wait(200);
        }

        private static async Task TypewriterGlitch(TextBlock tb, int msPerChar = 18) {
            var fullText = tb.Text;
            tb.Text = "";
            tb.Opacity = 1;
            const string glitchChars = "&*#@%?![]{}<>X01";
            for (int i = 0; i < fullText.Length; i++) {
                
                if (_skipIntro) { tb.Text = fullText; _isAnimating = false; return; }
                if (fullText[i] == ' ') { tb.Text += ' '; continue; }
                
                for (int g = 0; g < 2; g++) { if (_skipIntro) { _isAnimating = false; break; }
                    tb.Text = fullText.Substring(0, i) + glitchChars[_rng.Next(glitchChars.Length)];
                    await Task.Delay(10);
                }
                
                tb.Text = fullText.Substring(0, i + 1);
                int delay = msPerChar;
                if (_rng.Next(10) > 7) { delay = msPerChar * 2; }
                
                await Task.Delay(delay);
            }
        }

        private static TextBlock BuildCenteredLabel(string text, double fontSize, FontFamily font, SolidColorBrush brush, double topMargin = 0) {
            return new TextBlock {
                Text = text,
                FontSize = fontSize,
                FontWeight = FontWeights.Regular,
                FontFamily = font,
                Foreground = brush,
                Opacity = 0,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, topMargin, 0, 0),
                Effect = new DropShadowEffect {
                    Color = brush.Color,
                    ShadowDepth = 0,
                    BlurRadius = 18,
                    Opacity = 0.65
                }
            };
        }

        private static TextBlock BuildAccentRule(SolidColorBrush brush) {
            return new TextBlock {
                Text = new string('─', 60),
                FontSize = 20,
                FontFamily = _fontSub,
                Foreground = brush,
                Opacity = 0,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
            };
        }

        private static TextBlock BuildBlinkingCursor(SolidColorBrush brush) {
            var cursor = new TextBlock {
                Text = "█",
                FontSize = 22,
                FontFamily = _fontSub,
                Foreground = brush,
                Opacity = 1,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
            };
            
            var blink = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
            
            blink.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            blink.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500))));
            blink.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1000))));
            cursor.BeginAnimation(UIElement.OpacityProperty, blink);
            return cursor;
        }

        private static void FadeIn(UIElement el, int ms = 300, double from = 0, double to = 1) {
            var anim = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(ms));
            
            anim.EasingFunction = new CubicEase {
                EasingMode = EasingMode.EaseOut
            };
            
            el.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        private static void SlideUp(UIElement el, int ms = 350, double fromY = 20) {
            el.RenderTransform = new TranslateTransform(0, fromY);
            var anim = new DoubleAnimation(fromY, 0, TimeSpan.FromMilliseconds(ms));
            
            anim.EasingFunction = new CubicEase {
                EasingMode = EasingMode.EaseOut
            };
            
            ((TranslateTransform)el.RenderTransform).BeginAnimation(TranslateTransform.YProperty, anim);
        }

        private static async Task CRTPowerOff(MainWindow win) {
            var flash = new System.Windows.Shapes.Rectangle {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Fill = new SolidColorBrush(Colors.White),
                Opacity = 0.8,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 1)
            };
            win.TerminalOutputPanel.Children.Add(flash);
            var st = (ScaleTransform)flash.RenderTransform;

            var animY = new DoubleAnimation(1, 0.005, TimeSpan.FromMilliseconds(120));
            animY.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };

            var animX = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(120));
            animX.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
            animX.BeginTime = TimeSpan.FromMilliseconds(120);

            var animFade = new DoubleAnimation(0.8, 0, TimeSpan.FromMilliseconds(80));
            animFade.BeginTime = TimeSpan.FromMilliseconds(240);

            var tcs = new TaskCompletionSource<bool>();
            animFade.Completed += (_, _) => tcs.SetResult(true);

            st.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
            st.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
            flash.BeginAnimation(UIElement.OpacityProperty, animFade);

            await tcs.Task;
        }

        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // [ - Intro Engine - ]
        private static async Task RunIntro(MainWindow win, string title, string subtitle, string author, SolidColorBrush titleBrush, SolidColorBrush subBrush, SolidColorBrush authorBrush, IntroStyle style = IntroStyle.TypewriterGlitch) {
            if (_isAnimating) return;
            _isAnimating = true;
            _skipIntro = false;
            win.PreviewKeyDown += HandleInputSkip;
            win.PreviewMouseDown += HandleInputSkip;

            double fontMult = 1.4;

            win.OutputBox.Document.Blocks.Clear();
            HideEverything(win);
            win.TerminalOutputPanel.Children.Clear();
            win.TerminalOutputPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            win.TerminalOutputPanel.VerticalAlignment = VerticalAlignment.Center;

            if (style == IntroStyle.TypewriterGlitch && !_skipIntro) {
                await ShowNoiseBlast(win, titleBrush);
                await ShowBootLines(win, subBrush);
                win.TerminalOutputPanel.Children.Clear();
            }

            var rule1 = BuildAccentRule(titleBrush);
            win.TerminalOutputPanel.Children.Add(rule1);
            FadeIn(rule1, 200);
            SlideUp(rule1, 250);

            int delay1 = 120;
            if (ConsoleBehavior.SpeedLoad) { delay1 = 0; }
            await Wait(delay1);

            var titleBlock = BuildCenteredLabel(title, 100 * fontMult * 1.5, _fontTitle, titleBrush, 8);
            win.TerminalOutputPanel.Children.Add(titleBlock);
            SlideUp(titleBlock, 350, 30);
            if (style == IntroStyle.TypewriterGlitch) { await TypewriterGlitch(titleBlock, 22); }
            else { FadeIn(titleBlock, 350); }

            int delay2 = 400;
            if (ConsoleBehavior.SpeedLoad) { delay2 = 0; }
            await Wait(delay2);

            var subBlock = BuildCenteredLabel(subtitle, 40 * fontMult, _fontSub, subBrush, 10);
            win.TerminalOutputPanel.Children.Add(subBlock);
            SlideUp(subBlock, 300, 15);
            if (style == IntroStyle.TypewriterGlitch) { await TypewriterGlitch(subBlock, 12); }
            else { FadeIn(subBlock, 300); }

            int delay3 = 600;
            if (ConsoleBehavior.SpeedLoad) { delay3 = 0; }
            await Wait(delay3);

            var rule2 = BuildAccentRule(titleBrush);
            win.TerminalOutputPanel.Children.Add(rule2);
            FadeIn(rule2, 200);

            int delay4 = 200;
            if (ConsoleBehavior.SpeedLoad) { delay4 = 0; }
            await Wait(delay4);

            var cursor = BuildBlinkingCursor(titleBrush);
            win.TerminalOutputPanel.Children.Add(cursor);

            int delay5 = 700;
            if (ConsoleBehavior.SpeedLoad) { delay5 = 0; }
            await Wait(delay5);
            win.TerminalOutputPanel.Children.Remove(cursor);

            var authorBlock = BuildCenteredLabel(author, 40 * fontMult, _fontAuthor, authorBrush, 14);
            win.TerminalOutputPanel.Children.Add(authorBlock);
            FadeIn(authorBlock, 400);
            SlideUp(authorBlock, 400, 10);

            int delay6 = 1200;
            if (ConsoleBehavior.SpeedLoad) { delay6 = 0; }
            await Wait(delay6);

            if (!ConsoleBehavior.SpeedLoad && !_skipIntro) { await CRTPowerOff(win); }

            win.TerminalOutputPanel.Children.Clear();
            win.TerminalOutputPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            win.TerminalOutputPanel.VerticalAlignment = VerticalAlignment.Top;

            await Task.Delay(100);
            RestoreEverything(win);
            win.PreviewKeyDown -= HandleInputSkip;
            win.PreviewMouseDown -= HandleInputSkip;
            _isAnimating = false;
        }

        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // [ - Run Methods - ]
        public static async Task Run_Exo1(MainWindow win) {
            if (_isAnimating) { return; }
            await RunIntro(win, _title_exo1, _sub_exo1, _author, _exo1_title, _exo1_sub, _exo1_author);
            win.Exo1.Visibility = Visibility.Visible;
            win.Exo1B.Visibility = Visibility.Visible;
        }

        public static async Task Run_Exo2(MainWindow win) {
            if (_isAnimating) { return; }
            await RunIntro(win, _title_exo2, _sub_exo2, _author, _exo2_title, _exo2_sub, _exo2_author);
            win.Exo2.Visibility = Visibility.Visible;
        }

        public static async Task Run_Exo3(MainWindow win) {
            if (_isAnimating) { return; }
            await RunIntro(win, _title_exo3, _sub_exo3, _author, _exo3_title, _exo3_sub, _exo3_author);
            win.Exo3.Visibility = Visibility.Visible;
        }

        public static async Task Run_Exo4(MainWindow win) {
            if (_isAnimating) { return; }
            await RunIntro(win, _title_exo4, _sub_exo4, _author, _exo4_title, _exo4_sub, _exo4_author);
            Exo4.InitializeVirus(win);
        }

        public static async Task Run_Exo5(MainWindow win) {
            if (_isAnimating) { return; }
            await RunIntro(win, _title_exo5, _sub_exo5, _author, _exo5_title, _exo5_sub, _exo5_author);
            win.Exo5.Visibility = Visibility.Visible;
        }
        
        public static async Task Run_Exo6(MainWindow win) {
            if (_isAnimating) { return; }
            await RunIntro(win, _title_exo6, _sub_exo6, _author, _exo6_title, _exo6_sub, _exo6_author);
            win.Exo6.Visibility = Visibility.Visible;
        }

        public static async Task Run_Lab1(MainWindow win) {
            if (_isAnimating) { return; }
            await RunIntro(win, _title_lab1, _sub_lab1, _author, _lab1_title, _lab1_sub, _lab1_author);
            win.Labo1B.Visibility = Visibility.Visible;
        }

        public static async Task Run_Main(MainWindow win) {
            if (_isAnimating) { return; }
            await RunIntro(win, _title_main, _sub_main, _author2, _main_title, _main_sub, _main_author, IntroStyle.FadeOnly);
            if (ConsoleBehavior.Babymode == true) win.BabyMode.Visibility = Visibility.Visible;
            else { win.BabyMode.Visibility = Visibility.Collapsed; }

        }

        public static async Task Run_OverseerWars(MainWindow win) {
            if (_isAnimating) { return; }
            OverseerWarInit.Introduce(win);
        }
    }
}