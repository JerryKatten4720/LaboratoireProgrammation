using System;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.Views.Miscellaneous;

public partial class AssemblyMessageManager : UserControl
{
    private const int MaxLcdChars = 16;

    public AssemblyMessageManager()
    {
        InitializeComponent();
    }

    private void InputLine_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        int remaining = MaxLcdChars - textBox.Text.Length;
        string counterText = $"({remaining} CHARACTÈRES RESTANT)";

        if (textBox.Name == nameof(InputLine1) && CounterLine1 != null)
        {
            CounterLine1.Text = counterText;
        }
        else if (textBox.Name == nameof(InputLine2) && CounterLine2 != null)
        {
            CounterLine2.Text = counterText;
        }
    }

    private async void BtnPush_Click(object sender, RoutedEventArgs e)
    {
        string portName = InputComPort.Text.Trim();
        string textLine1 = InputLine1.Text;
        string textLine2 = InputLine2.Text;

        BtnPush.IsEnabled = false;
        BtnPush.Content = "[ - TX... - ]";

        await TransmitToHardwareAsync(portName, textLine1, textLine2);

        BtnPush.Content = "[ - PUSH - ]";
        BtnPush.IsEnabled = true;
    }

    private async Task TransmitToHardwareAsync(string portName, string line1, string line2)
    {
        if (string.IsNullOrWhiteSpace(portName)) return;

        try
        {
            using SerialPort port = new SerialPort(portName, 9600);
            port.Open();

            await SendHardwareCommandAsync(port, 0x01);
            await SendAsciiStringAsync(port, line1);

            if (!string.IsNullOrEmpty(line2))
            {
                await SendHardwareCommandAsync(port, 0xC0);
                await SendAsciiStringAsync(port, line2);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"TRANSMISSION FAILURE:\n{ex.Message}", "ROB.CO ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task SendHardwareCommandAsync(SerialPort port, byte commandCode)
    {
        byte[] buffer = { commandCode };
        port.Write(buffer, 0, 1);
        await Task.Delay(30);
    }

    private async Task SendAsciiStringAsync(SerialPort port, string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        byte[] buffer = Encoding.ASCII.GetBytes(text);

        foreach (byte b in buffer)
        {
            port.Write(new byte[] { b }, 0, 1);
            await Task.Delay(30);
        }
    }
}