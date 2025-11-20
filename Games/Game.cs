using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LaboratoireProgrammation.Utils;

namespace LaboratoireProgrammation.Games;
public class Game {
    
    private double positionX;
    private double positionY;
    
    public string Title { get; set; }
    public string Description { get; set; }
    public float Rate { get; set; }
    public string Path { get; set; }
    public bool IsLiked { get; set; }
    
    
    private static int MinimumGameIndex = 1;
    private int GameIndex = 1;
    private static int MaximumGameIndex;
    
    public static Canvas? DisplayCanvas;
    public static Image? DisplayedImage = new Image();

    public Game(string title, string desc, float rate, string path) {
        positionX = 0;
        positionY = 0;
        Title = title;
        Description = desc;
        Rate = rate;
        Path = path;
        IsLiked = false;
        
        DisplayCanvas = Application.Current.MainWindow.FindName("DisplayCanvas") as Canvas;
    }
    
    public void LoadGameImage() {
        Logs.Debug("Loading Game Image : Initialized...");
        if (DisplayCanvas != null && DisplayedImage != null) DisplayCanvas.Children.Add(DisplayedImage);
    }
    
    public static void UnloadGameImage() {
        Logs.Debug("Unloading Game Image : Initialized...");
        if (DisplayCanvas != null && DisplayedImage != null) DisplayCanvas.Children.Remove(DisplayedImage);
    }
    
    public void SwitchToPreviousImage() {
        GameIndex--;
        UnloadGameImage();
        if (GameIndex < MinimumGameIndex) GameIndex = MaximumGameIndex;
        
        //Console.WriteLine($"Going for index {GameIndex - 1}");
        
        DisplayedImage = new Image {
            Source = new BitmapImage(new Uri($"pack://application:,,,/Assets/Games/{GameList.fileNames[GameIndex - 1]}")),
            Height = Utils.SizeUtils.getHeight() * 0.9,
        };
        
        LoadGameImage();
    }
    
}