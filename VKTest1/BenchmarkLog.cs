using System.Text.Json.Serialization;

namespace VKTest1
{
    public enum BenchmarkType { CPU, GPU }

    public class BenchmarkLog
    {
        [JsonPropertyName("TimeStamp")] public long TimeStamp { get; set; }
        [JsonPropertyName("BenckMarkVersion")] public int BenchmarkVersion { get; set; }
        [JsonPropertyName("FPSAvg")] public double FpsAvg { get; set; }
        [JsonPropertyName("FPSMax")] public double FpsMax { get; set; }
        [JsonPropertyName("FPSMin")] public double FpsMin { get; set; }
        [JsonPropertyName("FPS95")] public double Fps95 { get; set; }
        [JsonPropertyName("CPUAvg")] public double CpuAvg { get; set; }
        [JsonPropertyName("GPUAvg")] public double GpuAvg { get; set; }
        [JsonPropertyName("GPULeak")] public double GpuLeak { get; set; }
        [JsonPropertyName("VideoMem")] public double VideoMem { get; set; }
        [JsonPropertyName("GameVer")] public string GameVer { get; set; } = "";
        [JsonPropertyName("SysVer")] public string SysVer { get; set; } = "";
        [JsonPropertyName("CPUModel")] public string CpuModel { get; set; } = "";
        [JsonPropertyName("GPUModel")] public string GpuModel { get; set; } = "";
        [JsonPropertyName("GpuDriverVer")] public string GpuDriverVer { get; set; } = "";
        [JsonPropertyName("VideoMemSize")] public string VideoMemSize { get; set; } = "";
        [JsonPropertyName("SysMem")] public string SysMem { get; set; } = "";
        [JsonPropertyName("Rtx")] public int Rtx { get; set; }
        [JsonPropertyName("Dlss")] public int Dlss { get; set; }
        [JsonPropertyName("InsertFrame")] public int InsertFrame { get; set; }
        [JsonPropertyName("Dx12")] public int Dx12 { get; set; }
        [JsonPropertyName("ScreenResolution")] public string ScreenResolution { get; set; } = "";
        [JsonPropertyName("ScreenMode")] public int ScreenMode { get; set; }
        [JsonPropertyName("QualityLevel")] public int QualityLevel { get; set; }
        [JsonPropertyName("ImageQuality")] public int ImageQuality { get; set; }
        [JsonPropertyName("ViewDistance")] public int ViewDistance { get; set; }
        [JsonPropertyName("AntiAliasing")] public int AntiAliasing { get; set; }
        [JsonPropertyName("PostProcessing")] public int PostProcessing { get; set; }
        [JsonPropertyName("ShadowQuality")] public int ShadowQuality { get; set; }
        [JsonPropertyName("TextureQuality")] public int TextureQuality { get; set; }
        [JsonPropertyName("MaterialQuality")] public int MaterialQuality { get; set; }
        [JsonPropertyName("VegetationQuality")] public int VegetationQuality { get; set; }
        [JsonPropertyName("MotionBlur")] public int MotionBlur { get; set; }
        [JsonIgnore] public DateTime Date { get; set; }
        [JsonIgnore] public string FilePath { get; set; } = "";
        [JsonIgnore] public BenchmarkType Type { get; set; }
    }
}
