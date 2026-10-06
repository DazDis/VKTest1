namespace VKTest1
{
    public class BenchmarkConfig
    {
        public string Name { get; set; } = "Профиль";
        public BenchmarkType Type { get; set; } = BenchmarkType.CPU;

        public int Width { get; set; } = 1280;
        public int Height { get; set; } = 720;
        public int ScreenMode { get; set; } = 1; 
        public bool VSync { get; set; } = false;
        public bool Dlss { get; set; } = false;
        public bool FrameGen { get; set; } = false;
        public bool RayTracing { get; set; } = false;
        public int QualityLevel { get; set; } = 0; 
        public double ResolutionQuality { get; set; } = 50.0;

        public static BenchmarkConfig CreateCpuDefault() => new()
        {
            Name = "CPU-тест",
            Type = BenchmarkType.CPU,
            Width = 1280,
            Height = 720,
            QualityLevel = 0,
            ResolutionQuality = 50.0,
            Dlss = false,
            FrameGen = false,
            RayTracing = false
        };

        public static BenchmarkConfig CreateGpuDefault() => new()
        {
            Name = "GPU-тест",
            Type = BenchmarkType.GPU,
            Width = 1920,
            Height = 1080,
            QualityLevel = 3,
            ResolutionQuality = 100.0,
            Dlss = false,
            FrameGen = false,
            RayTracing = true
        };
    }
}
