using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using LaboratoireProgrammation.Utils;

namespace LaboratoireProgrammation;

public class GameRefactor {
    
    public string? Title = null;
    public string? ExecutablePath = null;
    public string? TexturePath = null;
    public bool? IsLiked = false;
    public static bool IsDisplayed;
    public static Image? gameImage = new Image();
    
    public static int ImageMinIndex = 1;
    public static int ImageIndex = 1;
    public static int ImageMaxIndex = GameList.fileCount;
    public static GameRefactor currentGame;
    
    private static Grid? mainGrid;
    private static Canvas? mouseCanvas;

    private static Point mouseOffset;

    public GameRefactor(string title, string exe, string tex, bool liked) {
        Title = title;
        ExecutablePath = exe;
        TexturePath = tex;
        IsLiked = liked;
        IsDisplayed = false;
        
        gameImage = new Image {
            Source = new BitmapImage(new Uri($"pack://application:,,,/Assets/Games/{GameList.fileNames[ImageIndex - 1]}")),
            Height = Utils.SizeUtils.getHeight() * 0.9,
        };
        gameImage.MouseDown += OnImageMouseDown;

        
        mainGrid = Application.Current.MainWindow.FindName("MainGrid") as Grid;
        mouseCanvas = Application.Current.MainWindow.FindName("MouseCanvas") as Canvas;

        Application.Current.MainWindow.MouseMove += OnMouseMove;

        currentGame = this;
    }
    
    public static void LoadGameImage() {
        if (mainGrid != null && gameImage != null && !IsDisplayed) mainGrid.Children.Add(gameImage);
        IsDisplayed = true;

    }
    
    public static void UnloadGameImage() {
        if (mainGrid != null && gameImage != null && IsDisplayed) mainGrid.Children.RemoveAt(mainGrid.Children.IndexOf(gameImage));
        IsDisplayed = false;

    }
    
    public static void ReloadGameImage() { UnloadGameImage(); LoadGameImage(); }
    
    private static void OnImageMouseDown(object sender, MouseButtonEventArgs e) {
        if (mouseCanvas == null || gameImage == null) return;

        // Get the click position relative to the image
        mouseOffset = e.GetPosition(gameImage);

        // Move image to mouse canvas (detach from its parent)
        var parent = LogicalTreeHelper.GetParent(gameImage) as Panel; parent?.Children.Remove(gameImage);

        if (!mouseCanvas.Children.Contains(gameImage))
            mouseCanvas.Children.Add(gameImage);

        // Capture the mouse so dragging stays smooth
        gameImage.CaptureMouse();
    }

    private static void OnMouseMove(object sender, MouseEventArgs e) {
        if (mouseCanvas == null || gameImage == null || !gameImage.IsMouseCaptured)
            return;

        var pos = e.GetPosition(mouseCanvas);

        Canvas.SetLeft(gameImage, pos.X - mouseOffset.X);
        Canvas.SetTop(gameImage, pos.Y - mouseOffset.Y);
    }

    private static void OnImageMouseUp(object sender, MouseButtonEventArgs e) {
        if (gameImage == null) return;
        gameImage.ReleaseMouseCapture();
        
        var parent = LogicalTreeHelper.GetParent(gameImage) as Panel;
        parent?.Children.Add(gameImage);
        
    }

    public static void previousGame() {
        ImageIndex--;
        UnloadGameImage();
        if (ImageIndex < ImageMinIndex) ImageIndex = ImageMaxIndex;
        
        Console.WriteLine($"Going for index {ImageIndex - 1}");
        
        gameImage = new Image {
            Source = new BitmapImage(new Uri($"pack://application:,,,/Assets/Games/{GameList.fileNames[ImageIndex - 1]}")),
            Height = Utils.SizeUtils.getHeight() * 0.9,
        };
        
        LoadGameImage();
    }
    
    public static void nextGame() {
        ImageIndex++;
        UnloadGameImage();
        if (ImageIndex > ImageMaxIndex) ImageIndex = ImageMinIndex;
        
        Console.WriteLine($"Going for index {ImageIndex - 1}");
        
        gameImage = new Image {
            Source = new BitmapImage(new Uri($"pack://application:,,,/Assets/Games/{GameList.fileNames[ImageIndex - 1]}")),
            Height = Utils.SizeUtils.getHeight() * 0.9,
        };
        
        LoadGameImage();
        
    }
}