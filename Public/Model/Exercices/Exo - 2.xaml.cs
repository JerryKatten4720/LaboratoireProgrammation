using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LaboratoireProgrammation.Public.Visual_Utils;

namespace LaboratoireProgrammation.Public.Model.Exercices;

public partial class Exo2 : UserControl {
    private bool _isComplete = false;

    public Exo2() {
        InitializeComponent();
        Folder1.Opacity = 0;
        Folder2.Opacity = 0;
        Folder3.Opacity = 0;
        ProgressBar1.Visibility = Visibility.Hidden;
        ProgressBar2.Visibility = Visibility.Hidden;
    }

    private void SendFilesClick(object sender, RoutedEventArgs e) {
        ProgressBar1.Visibility = Visibility.Visible;
        ProgressBar2.Visibility = Visibility.Visible;
        Dispatcher.Invoke(() => {
            _ = StartBackgroundWork();
            _ = StartBackgroundAnimation();
        });
    }

    private async Task StartBackgroundWork() {
        Random rand = new Random();
        while (!_isComplete) {
            ProgressBar2.Value = 0;
            await Task.Run(async () => {
                for (int i = 0; i < 100; i++) {
                    int rng = rand.Next(10, 80);

                    ProgressBar2.Dispatcher.Invoke(() => { ProgressBar2.Value++; });

                    await Task.Delay(rng);
                }

                if (!_isComplete) {
                    ProgressBar1.Dispatcher.Invoke(() => {
                        ProgressBar1.Value += rand.Next(20, 35);
                        if (ProgressBar1.Value >= 100)
                            _isComplete = true;
                    });
                }
            });
        }
        FinishTransfer();
    }

    private async Task StartBackgroundAnimation() {
        while (!_isComplete) {
            Dispatcher.Invoke(() => {
                Folder2.Opacity = 0;
                Folder3.Opacity = 0;
                Folder1.Opacity = 1;
            });
            await Task.Delay(800);

            Dispatcher.Invoke(() => {
                Folder1.Opacity = 0.05;
                Folder2.Opacity = 1;
            });
            await Task.Delay(800);

            Dispatcher.Invoke(() => {
                Folder1.Opacity = 0;
                Folder2.Opacity = 0.05;
                Folder3.Opacity = 1;
            });
            await Task.Delay(800);
        }
    }

    private void FinishTransfer() {
        ProgressBar2.Foreground = ColorUtils.SuccessBrush;
        
        Folder1.Visibility = Visibility.Hidden;
        Folder2.Visibility = Visibility.Hidden;
        Folder3.Visibility = Visibility.Hidden;

        FilesButton.Content = "TERMINÉ !";
        FilesButton.BorderBrush = ColorUtils.SuccessBrush;
        FilesButton.Foreground = ColorUtils.SuccessBrush;
        FilesButton.IsEnabled = false;
    }
    
}