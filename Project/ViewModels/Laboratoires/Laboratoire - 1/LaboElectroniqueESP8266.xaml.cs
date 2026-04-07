using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.ViewModels.Laboratoires.Laboratoire___1;

public partial class LaboElectroniqueESP8266 : UserControl {
    private string _buffer = "";
    private bool _isConnected;


    private SerialPort _serialPort;

    public LaboElectroniqueESP8266() {
        InitializeComponent();
        ListerPortsCom();
    }


    private void ListerPortsCom() {
        CbxPorts.Items.Clear();
        string[] ports = SerialPort.GetPortNames();

        foreach (var port in ports) CbxPorts.Items.Add(port);

        if (CbxPorts.Items.Count > 0) CbxPorts.SelectedIndex = 0;
    }


    private void BtnConnect_Click(object sender, RoutedEventArgs e) {
        if (!_isConnected) {
            if (CbxPorts.SelectedItem == null) return;

            var portName = CbxPorts.SelectedItem.ToString();

            try {
                _serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                _isConnected = true;
                BtnConnect.Content = "DISCONNECT";
                CbxPorts.IsEnabled = false;
            }
            catch (Exception ex) {
                MessageBox.Show($"Erreur de connexion : {ex.Message}", "Erreur Système", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        else {
            DeconnecterPortSerie();
        }
    }

    private void DeconnecterPortSerie() {
        if (_serialPort != null && _serialPort.IsOpen) _serialPort.Close();

        _isConnected = false;
        BtnConnect.Content = "CONNECT";
        CbxPorts.IsEnabled = true;


        TxtTemperature.Text = "--.- °C";
        TxtHumidite.Text = "--.- %";
        TxtLuminosite.Text = "---- LUX";
    }


    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e) {
        try {
            string data = _serialPort.ReadExisting();

            foreach (var c in data)
                if (c == '\n') {
                    var trameComplete = _buffer.Trim();
                    _buffer = "";


                    Dispatcher.Invoke(() => TraiterTrame(trameComplete));
                }
                else if (c != '\r') {
                    _buffer += c;
                }
        }
        catch (Exception) { }
    }


    private void TraiterTrame(string trame) {
        if (string.IsNullOrEmpty(trame) || !trame.StartsWith("T:")) return;

        try {
            var sections = trame.Split(',');

            if (sections.Length == 3) {
                var tempStr = sections[0].Replace("T:", "");
                var humStr = sections[1].Replace("H:", "");
                var lumStr = sections[2].Replace("L:", "");

                TxtTemperature.Text = $"{tempStr} °C";
                TxtHumidite.Text = $"{humStr} %";
                TxtLuminosite.Text = $"{lumStr} LUX";
            }
        }
        catch { }
    }


    private void BtnCommand_Click(object sender, RoutedEventArgs e) {
        if (_isConnected && _serialPort != null && _serialPort.IsOpen) {
            var btn = sender as Button;
            if (btn != null && btn.Tag != null) {
                var commande = btn.Tag.ToString();
                _serialPort.Write(commande);
            }
        }
    }
}