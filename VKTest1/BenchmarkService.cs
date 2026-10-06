using Microsoft.Win32;
using System.Diagnostics;

namespace VKTest1
{
    public interface IBenchmarkService
    {
        string? ExecutablePath { get; }
        bool IsFound { get; }
        bool TryLocate();
        void SetExecutablePath(string path);
        bool IsSteamRunning();

        Task<BenchmarkResult> RunAsync(CancellationToken token = default);
    }
    public class BenchmarkService : IBenchmarkService
    {
        private const string RelativeExePath =
            @"steamapps\common\Black Myth Wukong Benchmark Tool\b1_benchmark.exe";

        private static readonly string[] PossibleExeNames =
        {
            "b1_benchmark.exe",
            "b1-Win64-Shipping.exe",
            "BlackMythWukongBenchmark.exe"
        };

        public string? ExecutablePath { get; private set; }
        public bool IsFound => !string.IsNullOrEmpty(ExecutablePath) && File.Exists(ExecutablePath);

        public BenchmarkService()
        {
            TryLocate();
        }

        public bool TryLocate()
        {
            var steamPath = Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Valve\Steam",
                "SteamPath", null) as string;

            if (string.IsNullOrEmpty(steamPath))
                return false;

            var libraries = CollectSteamLibraries(steamPath);

            foreach (var lib in libraries)
            {
                var gameFolder = Path.Combine(lib,
                    @"steamapps\common\Black Myth Wukong Benchmark Tool");

                if (!Directory.Exists(gameFolder))
                    continue;

                var standard = Path.Combine(lib, RelativeExePath);
                if (File.Exists(standard))
                {
                    ExecutablePath = standard;
                    return true;
                }

                foreach (var name in PossibleExeNames)
                {
                    var candidate = Path.Combine(gameFolder, name);
                    if (File.Exists(candidate))
                    {
                        ExecutablePath = candidate;
                        return true;
                    }
                }

                var found = Directory.GetFiles(gameFolder, "*benchmark*.exe",
                    SearchOption.TopDirectoryOnly);
                if (found.Length > 0)
                {
                    ExecutablePath = found[0];
                    return true;
                }
            }

            return false;
        }

        public void SetExecutablePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Путь пуст", nameof(path));
            if (!File.Exists(path))
                throw new FileNotFoundException("Файл не найден", path);

            ExecutablePath = path;
        }
        public bool IsSteamRunning()
        {
            try
            {
                return Process.GetProcessesByName("steam").Any();
            }
            catch
            {
                return false;
            }
        }
        public async Task<BenchmarkResult> RunAsync(CancellationToken token = default)
        {
            if (!IsFound)
                throw new InvalidOperationException(
                    "Бенчмарк не найден. Укажите путь вручную через SetExecutablePath().");

            var startInfo = new ProcessStartInfo
            {
                FileName = ExecutablePath!,
                WorkingDirectory = Path.GetDirectoryName(ExecutablePath!)!,
                UseShellExecute = false
            };

            var result = new BenchmarkResult
            {
                StartedAt = DateTime.Now
            };

            using var process = new Process { StartInfo = startInfo };

            if (!process.Start())
                throw new InvalidOperationException("Не удалось запустить процесс бенчмарка.");

            try
            {
                await process.WaitForExitAsync(token);
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited)
                {
                    try { process.Kill(entireProcessTree: true); }
                    catch {  }
                }
                throw;
            }

            result.ExitCode = process.ExitCode;
            result.FinishedAt = DateTime.Now;
            return result;
        }

        private static List<string> CollectSteamLibraries(string steamPath) 
        {
            var libraries = new List<string> { steamPath };

            var vdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(vdf)) return libraries;

            foreach (var line in File.ReadAllLines(vdf))
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("\"path\"")) continue;

                var parts = trimmed.Split('"', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                    libraries.Add(parts[1].Replace(@"\\", @"\"));
            }

            return libraries;
        }
    }
}
