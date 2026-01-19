using Newtonsoft.Json;

namespace NeuroBitMonitor.Models
{
    public class MindWavePacket
    {
        [JsonProperty("poorSignalLevel")]
        public int PoorSignalLevel { get; set; } = 200;

        [JsonProperty("eSense")]
        public ESense ESense { get; set; }

        [JsonProperty("eegPower")]
        public EegPower EegPower { get; set; }

        [JsonProperty("blinkStrength")]
        public int BlinkStrength { get; set; }

        [JsonProperty("rawEeg")]
        public int RawEeg { get; set; }
    }

    public class ESense
    {
        [JsonProperty("attention")]
        public int Attention { get; set; }
        [JsonProperty("meditation")]
        public int Meditation { get; set; }
    }

    public class EegPower
    {
        public int delta { get; set; }
        public int theta { get; set; }
        public int lowAlpha { get; set; }
        public int highAlpha { get; set; }
        public int lowBeta { get; set; }
        public int highBeta { get; set; }
        public int lowGamma { get; set; }
        public int highGamma { get; set; }
    }
}