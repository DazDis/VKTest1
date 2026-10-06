namespace VKTest1
{
    public class BenchmarkResult
    {
        public int ExitCode { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }
        public TimeSpan Duration => FinishedAt - StartedAt;
        public bool Success => ExitCode == 0;
    }
}
