using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace LaboratoireProgrammation.Assets.shaders.wrappers
{
    public class smoke : ShaderEffect
    {
        private static PixelShader _pixelShader = new PixelShader();

        static smoke()
        {
            // Path looks correct based on your previous messages
            _pixelShader.UriSource = new Uri("pack://application:,,,/LaboratoireProgrammation;component/Assets/shaders/effects/smoke.ps");
        }

        public smoke()
        {
            this.PixelShader = _pixelShader;
            
            // Standard ShaderEffect initialization
            this.UpdateShaderValue(TimeProperty);
            this.UpdateShaderValue(SmokeColorProperty);
            this.UpdateShaderValue(IntensityProperty);
            this.UpdateShaderValue(DensityProperty);
            this.UpdateShaderValue(DirectionProperty);
            this.UpdateShaderValue(SpeedProperty);
            this.UpdateShaderValue(DispersionProperty);
            this.UpdateShaderValue(CoverageProperty);
        }

        // --- Time ---
        public double Time
        {
            get { return (double)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }
        // Note: PixelShaderConstantCallback is inherited from the base class ShaderEffect.
        // We do NOT need to define it ourselves.
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(double), typeof(smoke), 
                new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0)));

        // --- Smoke Color ---
        public Color SmokeColor
        {
            get { return (Color)GetValue(SmokeColorProperty); }
            set { SetValue(SmokeColorProperty, value); }
        }
        public static readonly DependencyProperty SmokeColorProperty =
            DependencyProperty.Register("SmokeColor", typeof(Color), typeof(smoke), 
                new UIPropertyMetadata(Colors.WhiteSmoke, PixelShaderConstantCallback(1)));

        // --- Intensity ---
        public double Intensity
        {
            get { return (double)GetValue(IntensityProperty); }
            set { SetValue(IntensityProperty, value); }
        }
        public static readonly DependencyProperty IntensityProperty =
            DependencyProperty.Register("Intensity", typeof(double), typeof(smoke), 
                new UIPropertyMetadata(1.5, PixelShaderConstantCallback(2)));

        // --- Density ---
        public double Density
        {
            get { return (double)GetValue(DensityProperty); }
            set { SetValue(DensityProperty, value); }
        }
        public static readonly DependencyProperty DensityProperty =
            DependencyProperty.Register("Density", typeof(double), typeof(smoke), 
                new UIPropertyMetadata(0.8, PixelShaderConstantCallback(3)));

        // --- Direction ---
        public Point Direction
        {
            get { return (Point)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }
        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(Point), typeof(smoke), 
                new UIPropertyMetadata(new Point(0.2, -0.5), PixelShaderConstantCallback(4)));

        // --- Speed ---
        public double Speed
        {
            get { return (double)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }
        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.Register("Speed", typeof(double), typeof(smoke), 
                new UIPropertyMetadata(0.3, PixelShaderConstantCallback(5)));

        // --- Dispersion ---
        public double Dispersion
        {
            get { return (double)GetValue(DispersionProperty); }
            set { SetValue(DispersionProperty, value); }
        }
        public static readonly DependencyProperty DispersionProperty =
            DependencyProperty.Register("Dispersion", typeof(double), typeof(smoke), 
                new UIPropertyMetadata(3.0, PixelShaderConstantCallback(6)));

        // --- Coverage ---
        public double Coverage
        {
            get { return (double)GetValue(CoverageProperty); }
            set { SetValue(CoverageProperty, value); }
        }
        public static readonly DependencyProperty CoverageProperty =
            DependencyProperty.Register("Coverage", typeof(double), typeof(smoke), 
                new UIPropertyMetadata(1.0, PixelShaderConstantCallback(7)));

        // ===========================================================================
        // FIX: The manual 'private static PropertyChangedCallback...' method was REMOVED.
        // The base class (ShaderEffect) provides this method automatically.
        // Including it manually causes the infinite loop / StackOverflow.
        // ===========================================================================
    }
}