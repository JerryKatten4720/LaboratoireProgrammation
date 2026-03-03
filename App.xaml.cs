using System.Windows;
using System.Windows.Media.Animation; // Don't forget this namespace!

namespace LaboratoireProgrammation;

public partial class App : Application {
    static App() {
        Timeline.DesiredFrameRateProperty.OverrideMetadata(
            typeof(Timeline),
            new FrameworkPropertyMetadata { DefaultValue = 165 }
        );
    }
}