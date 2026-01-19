using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NeuroBitMonitor.Models;
using NeuroBitMonitor.Services;

namespace NeuroBitMonitor.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ThinkGearService _bciService;

        // --- Logging Fields ---
        private StreamWriter _logWriter;
        private readonly object _logLock = new object(); // Dedicated lock object for thread safety
        private bool _isLoggingEnabled;
        private string _statusLogText = "Ready.";

        // --- Backing Fields for Data ---
        private int _attention;
        private int _meditation;
        private int _signalQuality = 200;
        private int _blinkStrength;
        private string _connectionStatus = "Ready";
        private PointCollection _rawWavePoints;

        // --- Options ---
        public bool IsBlinkEnabled { get; set; } = true;
        public bool IsRawEnabled { get; set; } = true;
        public int ThresholdAttention { get; set; } = 80;
        public int ThresholdMeditation { get; set; } = 80;

        // Spectral Bands
        private int _delta, _theta, _lowAlpha, _highAlpha, _lowBeta, _highBeta, _lowGamma, _highGamma;

        public RelayCommand ConnectCommand { get; set; }
        public RelayCommand DisconnectCommand { get; set; }
        public RelayCommand ToggleLogCommand { get; set; }

        public MainViewModel()
        {
            _bciService = new ThinkGearService();
            _bciService.DataReceived += OnPacketReceived;
            _bciService.StatusChanged += (s) => ConnectionStatus = s;

            ConnectCommand = new RelayCommand(o => _bciService.Connect());
            DisconnectCommand = new RelayCommand(o => _bciService.Disconnect());
            ToggleLogCommand = new RelayCommand(o => ToggleLogging());

            // Initialize Graph Line
            RawWavePoints = new PointCollection();
            for (int i = 0; i < 300; i++) RawWavePoints.Add(new Point(i, 50));
        }

        private void OnPacketReceived(MindWavePacket packet)
        {
            // ---------------------------------------------------------
            // 1. FAST PROCESSING (Background Thread) - LOGGING
            // ---------------------------------------------------------
            // We lock on a dedicated object to prevent crashing if the file closes while writing
            if (IsLoggingEnabled && packet.RawEeg != 0)
            {
                lock (_logLock)
                {
                    if (_logWriter != null)
                    {
                        // CSV Format: RawValue, Timestamp
                        _logWriter.WriteLine($"{packet.RawEeg},{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                    }
                }
            }

            // ---------------------------------------------------------
            // 2. UI UPDATES (Main Thread)
            // ---------------------------------------------------------
            Application.Current.Dispatcher.Invoke(() =>
            {
                SignalQuality = packet.PoorSignalLevel;

                // Feature: Raw Graph
                if (IsRawEnabled && packet.RawEeg != 0)
                {
                    UpdateRawGraph(packet.RawEeg);
                }

                // Feature: Blink
                if (IsBlinkEnabled && packet.BlinkStrength > 0)
                {
                    BlinkStrength = packet.BlinkStrength;
                }

                // Feature: eSense & Power Bands
                if (packet.ESense != null)
                {
                    Attention = packet.ESense.Attention;
                    Meditation = packet.ESense.Meditation;

                    Delta = packet.EegPower.delta;
                    Theta = packet.EegPower.theta;
                    LowAlpha = packet.EegPower.lowAlpha;
                    HighAlpha = packet.EegPower.highAlpha;
                    LowBeta = packet.EegPower.lowBeta;
                    HighBeta = packet.EegPower.highBeta;
                    LowGamma = packet.EegPower.lowGamma;
                    HighGamma = packet.EegPower.highGamma;
                }
            });
        }

        private void UpdateRawGraph(int rawVal)
        {
            var points = new PointCollection(RawWavePoints);
            points.RemoveAt(0);

            // Scaling: Raw values are approx -2048 to +2048. Map to 0-100 Canvas.
            double y = 50 - (rawVal / 8.0);
            if (y < 0) y = 0;
            if (y > 100) y = 100;

            points.Add(new Point(300, y));

            for (int i = 0; i < points.Count; i++) points[i] = new Point(i, points[i].Y);

            RawWavePoints = points;
            OnPropertyChanged(nameof(RawWavePoints));
        }

        private void ToggleLogging()
        {
            lock (_logLock) // Ensure we don't switch files while a write is happening
            {
                if (IsLoggingEnabled)
                {
                    // --- STOP LOGGING ---
                    IsLoggingEnabled = false;
                    try
                    {
                        _logWriter?.Flush();
                        _logWriter?.Dispose(); // Dispose closes the stream
                    }
                    catch (Exception ex)
                    {
                        StatusLogText = $"Error saving: {ex.Message}";
                    }
                    finally
                    {
                        _logWriter = null;
                    }

                    if (!StatusLogText.StartsWith("Error"))
                        StatusLogText = "Log Saved to Documents.";
                }
                else
                {
                    // --- START LOGGING ---
                    try
                    {
                        // Save to "My Documents" folder so it's easy to find
                        string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string filename = $"RawEEG_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        string fullPath = Path.Combine(folder, filename);

                        _logWriter = new StreamWriter(fullPath, true);
                        _logWriter.AutoFlush = true; // Ensure data is written even if app crashes

                        // Write Header
                        _logWriter.WriteLine("RawValue,Timestamp");

                        IsLoggingEnabled = true;
                        StatusLogText = $"Recording: {filename}";
                    }
                    catch (Exception ex)
                    {
                        StatusLogText = $"File Error: {ex.Message}";
                        IsLoggingEnabled = false;
                    }
                }
            }

            OnPropertyChanged(nameof(IsLoggingEnabled));
            OnPropertyChanged(nameof(StatusLogText));
        }

        // --- Property Bindings ---
        public string ConnectionStatus { get => _connectionStatus; set { _connectionStatus = value; OnPropertyChanged(); } }
        public int SignalQuality { get => _signalQuality; set { _signalQuality = value; OnPropertyChanged(); } }

        public int Attention { get => _attention; set { _attention = value; OnPropertyChanged(); } }
        public int Meditation { get => _meditation; set { _meditation = value; OnPropertyChanged(); } }
        public int BlinkStrength { get => _blinkStrength; set { _blinkStrength = value; OnPropertyChanged(); } }

        public PointCollection RawWavePoints { get => _rawWavePoints; set { _rawWavePoints = value; OnPropertyChanged(); } }

        public bool IsLoggingEnabled { get => _isLoggingEnabled; set { _isLoggingEnabled = value; OnPropertyChanged(); } }
        public string StatusLogText { get => _statusLogText; set { _statusLogText = value; OnPropertyChanged(); } }

        // Spectral Properties
        public int Delta { get => _delta; set { _delta = value; OnPropertyChanged(); } }
        public int Theta { get => _theta; set { _theta = value; OnPropertyChanged(); } }
        public int LowAlpha { get => _lowAlpha; set { _lowAlpha = value; OnPropertyChanged(); } }
        public int HighAlpha { get => _highAlpha; set { _highAlpha = value; OnPropertyChanged(); } }
        public int LowBeta { get => _lowBeta; set { _lowBeta = value; OnPropertyChanged(); } }
        public int HighBeta { get => _highBeta; set { _highBeta = value; OnPropertyChanged(); } }
        public int LowGamma { get => _lowGamma; set { _lowGamma = value; OnPropertyChanged(); } }
        public int HighGamma { get => _highGamma; set { _highGamma = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // --- Helper Class ---
    public class RelayCommand : ICommand
    {
        private Action<object> _action;
        public RelayCommand(Action<object> action) => _action = action;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _action(parameter);
        public event EventHandler CanExecuteChanged;
    }
}