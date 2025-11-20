using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LaboratoireProgrammation;

public class GameList {
    
    public static List<String> gamesImages = new List<String>();
    public static List<String> fileNames = new List<String>();
    
    public static int fileCount = 0;
    private static void LoadFileNames() {
        
        DirectoryInfo dir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/Games"));
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files) {
            fileNames.Add(file.Name);
            Console.WriteLine("Loaded : " + file.Name + " at index " + fileCount);
            fileCount++;
        }
        Console.WriteLine("Loaded " + fileNames.Count + " game(s) filenames");
    }
    
    public static void LoadGames() {
        LoadFileNames();
        
        foreach (string fileName in fileNames) {
            gamesImages.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/Games/" + fileName));
        }
        
        Console.WriteLine("Loaded " + gamesImages.Count + " game(s) images");
    }
}