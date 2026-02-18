using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace LaboratoireProgrammation.Assets.shaders.wrappers;

public class smoke : ShaderEffect {
    private static readonly PixelShader _pixelShader = new();

    // Note: PixelShaderConstantCallback is inherited from the base class ShaderEffect.
    // We do NOT need to define it ourselves.
    public static readonly DependencyProperty TimeProperty =
        DependencyProperty.Register("Time", typeof(double), typeof(smoke),
            new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0)));

    public static readonly DependencyProperty SmokeColorProperty =
        DependencyProperty.Register("SmokeColor", typeof(Color), typeof(smoke),
            new UIPropertyMetadata(Colors.WhiteSmoke, PixelShaderConstantCallback(1)));

    public static readonly DependencyProperty IntensityProperty =
        DependencyProperty.Register("Intensity", typeof(double), typeof(smoke),
            new UIPropertyMetadata(1.5, PixelShaderConstantCallback(2)));

    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register("Density", typeof(double), typeof(smoke),
            new UIPropertyMetadata(0.8, PixelShaderConstantCallback(3)));

    public static readonly DependencyProperty DirectionProperty =
        DependencyProperty.Register("Direction", typeof(Point), typeof(smoke),
            new UIPropertyMetadata(new Point(0.2, -0.5), PixelShaderConstantCallback(4)));

    public static readonly DependencyProperty SpeedProperty =
        DependencyProperty.Register("Speed", typeof(double), typeof(smoke),
            new UIPropertyMetadata(0.3, PixelShaderConstantCallback(5)));

    public static readonly DependencyProperty DispersionProperty =
        DependencyProperty.Register("Dispersion", typeof(double), typeof(smoke),
            new UIPropertyMetadata(3.0, PixelShaderConstantCallback(6)));

    public static readonly DependencyProperty CoverageProperty =
        DependencyProperty.Register("Coverage", typeof(double), typeof(smoke),
            new UIPropertyMetadata(1.0, PixelShaderConstantCallback(7)));
    
    public static readonly DependencyProperty InputProperty = 
        ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(smoke), 0);

    static smoke() {
        // Path looks correct based on your previous messages
        _pixelShader.UriSource =
            new Uri("pack://application:,,,/LaboratoireProgrammation;component/Assets/shaders/effects/smoke.ps");
    }

    public smoke() {
        PixelShader = _pixelShader;

        // Standard ShaderEffect initialization
        UpdateShaderValue(InputProperty);
        UpdateShaderValue(TimeProperty);
        UpdateShaderValue(SmokeColorProperty);
        UpdateShaderValue(IntensityProperty);
        UpdateShaderValue(DensityProperty);
        UpdateShaderValue(DirectionProperty);
        UpdateShaderValue(SpeedProperty);
        UpdateShaderValue(DispersionProperty);
        UpdateShaderValue(CoverageProperty);
    }

    // --- Time ---
    public double Time {
        get => (double)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    // --- Smoke Color ---
    public Color SmokeColor {
        get => (Color)GetValue(SmokeColorProperty);
        set => SetValue(SmokeColorProperty, value);
    }

    // --- Intensity ---
    public double Intensity {
        get => (double)GetValue(IntensityProperty);
        set => SetValue(IntensityProperty, value);
    }

    // --- Density ---
    public double Density {
        get => (double)GetValue(DensityProperty);
        set => SetValue(DensityProperty, value);
    }

    // --- Direction ---
    public Point Direction {
        get => (Point)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    // --- Speed ---
    public double Speed {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    // --- Dispersion ---
    public double Dispersion {
        get => (double)GetValue(DispersionProperty);
        set => SetValue(DispersionProperty, value);
    }

    // --- Coverage ---
    public double Coverage {
        get => (double)GetValue(CoverageProperty);
        set => SetValue(CoverageProperty, value);
    }
    
    public Brush Input {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    // ===========================================================================
    // FIX: The manual 'private static PropertyChangedCallback...' method was REMOVED.
    // The base class (ShaderEffect) provides this method automatically.
    // Including it manually causes the infinite loop / StackOverflow.
    // ===========================================================================
}