using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace LaboratoireProgrammation.Assets.shaders.wrappers {
    public class CRT : ShaderEffect {
        private static readonly PixelShader _pixelShader = new PixelShader();
        private DispatcherTimer _animationTimer;
        private double _time = 0.0;

        static CRT() {
            _pixelShader.UriSource = new Uri("pack://application:,,,/Assets/shaders/effects/crt.ps");
        }

        public CRT() {
            PixelShader = _pixelShader;
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(TimeProperty);
            UpdateShaderValue(FlickerSpeedProperty);
            UpdateShaderValue(GlitchIntensityProperty);
            UpdateShaderValue(ScreenResolutionProperty);
            UpdateShaderValue(DistortionStrengthProperty);
            UpdateShaderValue(CurveRadiusProperty);
            UpdateShaderValue(ScanlineIntensityProperty);
            UpdateShaderValue(ScanlineCountProperty);
            UpdateShaderValue(PhosphorDecayProperty);
            UpdateShaderValue(BloomStrengthProperty);
            UpdateShaderValue(TintColorProperty);
            UpdateShaderValue(BrightnessProperty);
            UpdateShaderValue(ContrastProperty);
            UpdateShaderValue(NoiseIntensityProperty);
            UpdateShaderValue(VignetteStrengthProperty);
            UpdateShaderValue(ChromaticAberrationProperty);
            UpdateShaderValue(RefreshLinePositionProperty);
            UpdateShaderValue(BurnInIntensityProperty);
            UpdateShaderValue(PixelGridIntensityProperty);
            UpdateShaderValue(FlickerIntensityProperty);
            UpdateShaderValue(InterferenceSpeedProperty);
            UpdateShaderValue(DustDensityProperty);
            UpdateShaderValue(SmokeDensityProperty);
            UpdateShaderValue(SparkIntensityProperty);
            UpdateShaderValue(ParticleSpeedProperty);
            UpdateShaderValue(NoiseScaleProperty);
            UpdateShaderValue(ScanlineSpeedProperty);
            
            StartAnimation();
        }

        public void StartAnimation()
        {
            if (_animationTimer != null) return;
            _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _animationTimer.Tick += (s, e) => { _time += 0.016; Time = _time; };
            _animationTimer.Start();
        }

        public void StopAnimation() { _animationTimer?.Stop(); _animationTimer = null; }

        // --- PROPERTIES ---
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(CRT), 0);
        public Brush Input { get => (Brush)GetValue(InputProperty); set => SetValue(InputProperty, value); }

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(double), typeof(CRT), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0)));
        public double Time { get => (double)GetValue(TimeProperty); set => SetValue(TimeProperty, value); }

        public static readonly DependencyProperty FlickerSpeedProperty = DependencyProperty.Register("FlickerSpeed", typeof(double), typeof(CRT), new UIPropertyMetadata(0.5, PixelShaderConstantCallback(1))); // Slower default
        public double FlickerSpeed { get => (double)GetValue(FlickerSpeedProperty); set => SetValue(FlickerSpeedProperty, value); }

        public static readonly DependencyProperty GlitchIntensityProperty = DependencyProperty.Register("GlitchIntensity", typeof(double), typeof(CRT), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(2))); // Clean default
        public double GlitchIntensity { get => (double)GetValue(GlitchIntensityProperty); set => SetValue(GlitchIntensityProperty, value); }

        public static readonly DependencyProperty ScreenResolutionProperty = DependencyProperty.Register("ScreenResolution", typeof(Size), typeof(CRT), new UIPropertyMetadata(new Size(1920, 1080), PixelShaderConstantCallback(3)));
        public Size ScreenResolution { get => (Size)GetValue(ScreenResolutionProperty); set => SetValue(ScreenResolutionProperty, value); }

        public static readonly DependencyProperty DistortionStrengthProperty = DependencyProperty.Register("DistortionStrength", typeof(double), typeof(CRT), new UIPropertyMetadata(0.1, PixelShaderConstantCallback(4)));
        public double DistortionStrength { get => (double)GetValue(DistortionStrengthProperty); set => SetValue(DistortionStrengthProperty, value); }

        public static readonly DependencyProperty CurveRadiusProperty = DependencyProperty.Register("CurveRadius", typeof(double), typeof(CRT), new UIPropertyMetadata(2.0, PixelShaderConstantCallback(5)));
        public double CurveRadius { get => (double)GetValue(CurveRadiusProperty); set => SetValue(CurveRadiusProperty, value); }

        public static readonly DependencyProperty ScanlineIntensityProperty = DependencyProperty.Register("ScanlineIntensity", typeof(double), typeof(CRT), new UIPropertyMetadata(0.15, PixelShaderConstantCallback(6))); // Lighter default
        public double ScanlineIntensity { get => (double)GetValue(ScanlineIntensityProperty); set => SetValue(ScanlineIntensityProperty, value); }

        public static readonly DependencyProperty ScanlineCountProperty = DependencyProperty.Register("ScanlineCount", typeof(double), typeof(CRT), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(7)));
        public double ScanlineCount { get => (double)GetValue(ScanlineCountProperty); set => SetValue(ScanlineCountProperty, value); }

        public static readonly DependencyProperty PhosphorDecayProperty = DependencyProperty.Register("PhosphorDecay", typeof(double), typeof(CRT), new UIPropertyMetadata(0.2, PixelShaderConstantCallback(8)));
        public double PhosphorDecay { get => (double)GetValue(PhosphorDecayProperty); set => SetValue(PhosphorDecayProperty, value); }

        public static readonly DependencyProperty BloomStrengthProperty = DependencyProperty.Register("BloomStrength", typeof(double), typeof(CRT), new UIPropertyMetadata(0.3, PixelShaderConstantCallback(9)));
        public double BloomStrength { get => (double)GetValue(BloomStrengthProperty); set => SetValue(BloomStrengthProperty, value); }

        public static readonly DependencyProperty TintColorProperty = DependencyProperty.Register("TintColor", typeof(Color), typeof(CRT), new UIPropertyMetadata(Color.FromArgb(200, 0, 255, 100), PixelShaderConstantCallback(10)));
        public Color TintColor { get => (Color)GetValue(TintColorProperty); set => SetValue(TintColorProperty, value); }

        public static readonly DependencyProperty BrightnessProperty = DependencyProperty.Register("Brightness", typeof(double), typeof(CRT), new UIPropertyMetadata(1.1, PixelShaderConstantCallback(11)));
        public double Brightness { get => (double)GetValue(BrightnessProperty); set => SetValue(BrightnessProperty, value); }

        public static readonly DependencyProperty ContrastProperty = DependencyProperty.Register("Contrast", typeof(double), typeof(CRT), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(12)));
        public double Contrast { get => (double)GetValue(ContrastProperty); set => SetValue(ContrastProperty, value); }

        public static readonly DependencyProperty NoiseIntensityProperty = DependencyProperty.Register("NoiseIntensity", typeof(double), typeof(CRT), new UIPropertyMetadata(0.04, PixelShaderConstantCallback(13))); // Lower default
        public double NoiseIntensity { get => (double)GetValue(NoiseIntensityProperty); set => SetValue(NoiseIntensityProperty, value); }

        public static readonly DependencyProperty VignetteStrengthProperty = DependencyProperty.Register("VignetteStrength", typeof(double), typeof(CRT), new UIPropertyMetadata(0.3, PixelShaderConstantCallback(14))); // Lighter default
        public double VignetteStrength { get => (double)GetValue(VignetteStrengthProperty); set => SetValue(VignetteStrengthProperty, value); }

        public static readonly DependencyProperty ChromaticAberrationProperty = DependencyProperty.Register("ChromaticAberration", typeof(double), typeof(CRT), new UIPropertyMetadata(0.5, PixelShaderConstantCallback(15))); // Subtle default
        public double ChromaticAberration { get => (double)GetValue(ChromaticAberrationProperty); set => SetValue(ChromaticAberrationProperty, value); }

        public static readonly DependencyProperty RefreshLinePositionProperty = DependencyProperty.Register("RefreshLinePosition", typeof(double), typeof(CRT), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(16)));
        public double RefreshLinePosition { get => (double)GetValue(RefreshLinePositionProperty); set => SetValue(RefreshLinePositionProperty, value); }

        public static readonly DependencyProperty BurnInIntensityProperty = DependencyProperty.Register("BurnInIntensity", typeof(double), typeof(CRT), new UIPropertyMetadata(0.05, PixelShaderConstantCallback(17))); // Minimal default
        public double BurnInIntensity { get => (double)GetValue(BurnInIntensityProperty); set => SetValue(BurnInIntensityProperty, value); }

        public static readonly DependencyProperty PixelGridIntensityProperty = DependencyProperty.Register("PixelGridIntensity", typeof(double), typeof(CRT), new UIPropertyMetadata(0.1, PixelShaderConstantCallback(18)));
        public double PixelGridIntensity { get => (double)GetValue(PixelGridIntensityProperty); set => SetValue(PixelGridIntensityProperty, value); }

        public static readonly DependencyProperty FlickerIntensityProperty = DependencyProperty.Register("FlickerIntensity", typeof(double), typeof(CRT), new UIPropertyMetadata(0.1, PixelShaderConstantCallback(19))); // Stable default
        public double FlickerIntensity { get => (double)GetValue(FlickerIntensityProperty); set => SetValue(FlickerIntensityProperty, value); }

        public static readonly DependencyProperty InterferenceSpeedProperty = DependencyProperty.Register("InterferenceSpeed", typeof(double), typeof(CRT), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(20)));
        public double InterferenceSpeed { get => (double)GetValue(InterferenceSpeedProperty); set => SetValue(InterferenceSpeedProperty, value); }

        public static readonly DependencyProperty DustDensityProperty = DependencyProperty.Register("DustDensity", typeof(double), typeof(CRT), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(21))); // Off by default
        public double DustDensity { get => (double)GetValue(DustDensityProperty); set => SetValue(DustDensityProperty, value); }

        public static readonly DependencyProperty SmokeDensityProperty = DependencyProperty.Register("SmokeDensity", typeof(double), typeof(CRT), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(22))); // Off by default
        public double SmokeDensity { get => (double)GetValue(SmokeDensityProperty); set => SetValue(SmokeDensityProperty, value); }

        public static readonly DependencyProperty SparkIntensityProperty = DependencyProperty.Register("SparkIntensity", typeof(double), typeof(CRT), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(23))); // Off by default
        public double SparkIntensity { get => (double)GetValue(SparkIntensityProperty); set => SetValue(SparkIntensityProperty, value); }

        public static readonly DependencyProperty ParticleSpeedProperty = DependencyProperty.Register("ParticleSpeed", typeof(double), typeof(CRT), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(24)));
        public double ParticleSpeed { get => (double)GetValue(ParticleSpeedProperty); set => SetValue(ParticleSpeedProperty, value); }

        // --- NEW CONTROLS ---
        public static readonly DependencyProperty NoiseScaleProperty = DependencyProperty.Register("NoiseScale", typeof(double), typeof(CRT), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(25)));
        public double NoiseScale { get => (double)GetValue(NoiseScaleProperty); set => SetValue(NoiseScaleProperty, value); }

        public static readonly DependencyProperty ScanlineSpeedProperty = DependencyProperty.Register("ScanlineSpeed", typeof(double), typeof(CRT), new UIPropertyMetadata(2.0, PixelShaderConstantCallback(26)));
        public double ScanlineSpeed { get => (double)GetValue(ScanlineSpeedProperty); set => SetValue(ScanlineSpeedProperty, value); }
        

        // --- PRESETS ---
        public void ApplyFalloutPreset()
        {
            TintColor = Color.FromArgb(200, 0, 255, 100);
            DistortionStrength = 0.1;
            ScanlineIntensity = 0.2;
            BloomStrength = 0.4;
            NoiseIntensity = 0.08;
            VignetteStrength = 0.4;
            ChromaticAberration = 1.0;
            BurnInIntensity = 0.1;
            Brightness = 1.1;
            DustDensity = 0.1; // Gentle dust
            ScanlineSpeed = 1.5;
        }

        public void ApplyCleanPreset()
        {
            TintColor = Color.FromArgb(255, 255, 255, 255);
            DistortionStrength = 0.02;
            ScanlineIntensity = 0.05;
            BloomStrength = 0.1;
            NoiseIntensity = 0.02;
            VignetteStrength = 0.15;
            ChromaticAberration = 0.2;
            BurnInIntensity = 0.0;
            Brightness = 1.0;
            ScanlineSpeed = 0.5;
            DustDensity = 0.0;
        }
    }
}