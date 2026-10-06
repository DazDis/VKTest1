using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VKTest1
{
    public interface ILogService
    {
        string HistoryFolder { get; }
        BenchmarkLog? ReadLatest();
        BenchmarkLog? Read(string filePath);
        IEnumerable<BenchmarkLog> ReadAll();
        Task<BenchmarkLog?> WaitForNewLogAsync(DateTime since, TimeSpan timeout, CancellationToken token);
    }

    public class LogService : ILogService
    {
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        public string HistoryFolder { get; }

        public LogService()
        {
            HistoryFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Temp", "b1", "BenchMarkHistory", "Tool");
        }

        public BenchmarkLog? ReadLatest()
        {
            if (!Directory.Exists(HistoryFolder)) return null;

            var file = Directory.GetFiles(HistoryFolder)
                .Where(f => !f.EndsWith(".bak"))
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();

            return file == null ? null : Read(file);
        }

        public BenchmarkLog? Read(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var log = JsonSerializer.Deserialize<BenchmarkLog>(json, _jsonOptions);
                if (log == null) return null;

                log.FilePath = filePath;
                log.Date = DateTimeOffset.FromUnixTimeSeconds(log.TimeStamp).LocalDateTime;
                return log;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LogService.Read: {ex.Message}");
                return null;
            }
        }

        public IEnumerable<BenchmarkLog> ReadAll()
        {
            if (!Directory.Exists(HistoryFolder)) yield break;

            var files = Directory.GetFiles(HistoryFolder)
                .OrderByDescending(File.GetLastWriteTimeUtc);

            foreach (var file in files)
            {
                var log = Read(file);
                if (log != null) yield return log;
            }
        }
        public async Task<BenchmarkLog?> WaitForNewLogAsync(DateTime since, TimeSpan timeout, CancellationToken token)
        {
            if (!Directory.Exists(HistoryFolder)) return null;

            var sinceUtc = since.ToUniversalTime();
            var deadline = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < deadline)
            {
                token.ThrowIfCancellationRequested();

                string? file = null;
                try
                {
                    file = Directory.GetFiles(HistoryFolder)
                        .Where(f => !f.EndsWith(".bak"))
                        .OrderByDescending(File.GetLastWriteTimeUtc)
                        .FirstOrDefault(f => File.GetLastWriteTimeUtc(f) >= sinceUtc);
                }
                catch { }

                if (file != null)
                {
                    return Read(file);
                }

                await Task.Delay(2000, token);
            }

            return null;
        }
    }
}