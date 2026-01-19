using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NeuroBitMonitor.Models;
using Newtonsoft.Json;

namespace NeuroBitMonitor.Services
{
    public class ThinkGearService
    {
        private TcpClient _client;
        private Stream _stream;
        private bool _isRunning;

        public event Action<MindWavePacket> DataReceived;
        public event Action<string> StatusChanged;

        public void Connect()
        {
            Task.Run(() =>
            {
                try
                {
                    StatusChanged?.Invoke("Connecting to TGC...");
                    _client = new TcpClient("127.0.0.1", 13854);
                    _stream = _client.GetStream();

                    // Config to enable Raw Data + JSON
                    var config = new { enableRawOutput = true, format = "Json" };
                    byte[] cmd = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(config));
                    _stream.Write(cmd, 0, cmd.Length);

                    _isRunning = true;
                    StatusChanged?.Invoke("Connected to Headset");
                    ReadLoop();
                }
                catch (Exception ex)
                {
                    StatusChanged?.Invoke($"Connection Failed: {ex.Message}");
                }
            });
        }

        private void ReadLoop()
        {
            using (var reader = new StreamReader(_stream))
            {
                while (_isRunning && _client.Connected)
                {
                    try
                    {
                        string line = reader.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            var packet = JsonConvert.DeserializeObject<MindWavePacket>(line);
                            if (packet != null) DataReceived?.Invoke(packet);
                        }
                    }
                    catch { /* Ignore JSON parse errors on partial packets */ }
                }
            }
        }

        public void Disconnect()
        {
            _isRunning = false;
            _client?.Close();
            StatusChanged?.Invoke("Disconnected");
        }
    }
}