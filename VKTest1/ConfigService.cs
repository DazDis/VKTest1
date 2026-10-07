using Microsoft.Win32;
using System.Globalization;
using System.Text.RegularExpressions;

namespace VKTest1
{
    public interface IConfigService
    {
        string ConfigPath { get; }
        bool IsConfigFound { get; }
        void SetConfigPath(string path);
        void ApplyProfile(BenchmarkConfig profile);
        void RestoreBackup();
    }

    public class ConfigService : IConfigService
    {
        private const string MainSection = "/Script/GSGameSettings.GSGameUserSettings";
        private const string ScalabilitySection = "ScalabilityGroups";
        private const string RayTracingSection = "RayTracing";

        private const string ConfigRelativePath =
            @"steamapps\common\Black Myth Wukong Benchmark Tool\b1\Saved\Config\Windows\GameUserSettings.ini";

        public string ConfigPath { get; private set; } = "";
        public bool IsConfigFound => !string.IsNullOrEmpty(ConfigPath) && File.Exists(ConfigPath);

        private string BackupPath => ConfigPath + ".bak";

        public ConfigService()
        {
            TryFindConfig();
        }

        public void SetConfigPath(string path)
        {
            ConfigPath = path;
        }

        private void TryFindConfig()
        {
            var steamPath = Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Valve\Steam",
                "SteamPath", null) as string;

            if (string.IsNullOrEmpty(steamPath)) return;

            var libraries = new List<string> { steamPath };
            var vdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (File.Exists(vdf))
            {
                foreach (var line in File.ReadAllLines(vdf))
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("\"path\""))
                    {
                        var parts = trimmed.Split('"', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                            libraries.Add(parts[1].Replace(@"\\", @"\"));
                    }
                }
            }

            foreach (var lib in libraries)
            {
                var candidate = Path.Combine(lib, ConfigRelativePath);
                if (File.Exists(candidate))
                {
                    ConfigPath = candidate;
                    return;
                }
            }
        }

        public void ApplyProfile(BenchmarkConfig profile)
        {
            if (!IsConfigFound)
                throw new FileNotFoundException("GameUserSettings.ini не найден");

            if (!File.Exists(BackupPath))
                File.Copy(ConfigPath, BackupPath);

            var dict = ParseIni(File.ReadAllLines(ConfigPath));

            Set(dict, MainSection, "DesiredScreenWidth", profile.Width.ToString());
            Set(dict, MainSection, "DesiredScreenHeight", profile.Height.ToString());
            Set(dict, MainSection, "ResolutionSizeX", profile.Width.ToString());
            Set(dict, MainSection, "ResolutionSizeY", profile.Height.ToString());
            Set(dict, MainSection, "bUseVSync", profile.VSync ? "True" : "False");
            Set(dict, MainSection, "FrameRateLimit", "0.000000");
            Set(dict, MainSection, "FullscreenMode", profile.ScreenMode.ToString());

            if (dict.TryGetValue(MainSection, out var main) &&
                main.TryGetValue("UISettingData", out var uiData))
            {
                main["UISettingData"] = UpdateUiSettingData(uiData, profile);
            }

            Set(dict, ScalabilitySection, "sg.ResolutionQuality",
                profile.ResolutionQuality.ToString(CultureInfo.InvariantCulture));

            foreach (var key in new[]
            {
                "sg.ViewDistanceQuality", "sg.AntiAliasingQuality",
                "sg.ShadowQuality", "sg.GlobalIlluminationQuality",
                "sg.ReflectionQuality", "sg.PostProcessQuality",
                "sg.TextureQuality", "sg.EffectsQuality",
                "sg.FoliageQuality", "sg.ShadingQuality"
            })
            {
                Set(dict, ScalabilitySection, key, profile.QualityLevel.ToString());
            }

            Set(dict, ScalabilitySection, "sg.RayTracingQuality",
                profile.RayTracing ? "3" : "0");

            Set(dict, RayTracingSection, "r.RayTracing.EnableInGame",
                profile.RayTracing ? "True" : "False");

            File.WriteAllLines(ConfigPath, SerializeIni(dict));
        }

        public void RestoreBackup()
        {
            if (File.Exists(BackupPath))
                File.Copy(BackupPath, ConfigPath, overwrite: true);
        }

        private static Dictionary<string, Dictionary<string, string>> ParseIni(IEnumerable<string> lines)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            string section = "";
            result[section] = new();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0) continue;

                if (trimmed[0] == '[' && trimmed[^1] == ']')
                {
                    section = trimmed.Substring(1, trimmed.Length - 2).Trim(); 
                    if (!result.ContainsKey(section))
                        result[section] = new();
                    continue;
                }

                var eq = trimmed.IndexOf('=');
                if (eq > 0)
                {
                    var key = trimmed.Substring(0, eq);
                    var value = trimmed.Substring(eq + 1);
                    result[section][key] = value;
                }
            }
            return result;
        }

        private static List<string> SerializeIni(Dictionary<string, Dictionary<string, string>> dict)
        {
            var result = new List<string>();
            foreach (var (section, kv) in dict)
            {
                if (section.Length > 0)
                    result.Add($"[{section}]");

                foreach (var (key, value) in kv)
                    result.Add($"{key}={value}");

                result.Add("");
            }
            return result;
        }

        private static void Set(Dictionary<string, Dictionary<string, string>> dict,
                                string section, string key, string value)
        {
            if (!dict.TryGetValue(section, out var s))
            {
                s = new();
                dict[section] = s;
            }
            s[key] = value;
        }

        private static string UpdateUiSettingData(string uiData, BenchmarkConfig p)
        {
            var replace = new Dictionary<string, string>
            {
                ["ScreenMode"] = p.ScreenMode.ToString(),
                ["Vsync"] = p.VSync ? "1" : "0",
                ["Dlss"] = p.Dlss ? "1" : "0",
                ["Dx12"] = "1",
                ["MotionBlur"] = p.MotionBlur.ToString(),
                ["InsertFrame"] = p.FrameGen ? "1" : "0",
                ["Rtx"] = p.RayTracing ? "1" : "0",
                ["RtxLevel"] = p.RayTracing ? "3" : "0",
                ["QualityLevel"] = p.QualityLevel.ToString(),
                ["ViewDistance"] = p.QualityLevel.ToString(),
                ["AntiAliasing"] = p.QualityLevel.ToString(),
                ["PostProcessing"] = p.QualityLevel.ToString(),
                ["ShadowQuality"] = p.QualityLevel.ToString(),
                ["TextureQuality"] = p.QualityLevel.ToString(),
                ["FxQuality"] = p.QualityLevel.ToString(),
                ["MaterialQuality"] = p.QualityLevel.ToString(),
                ["VegetationQuality"] = p.QualityLevel.ToString(),
                ["GlobalIllumination"] = p.QualityLevel.ToString(),
                ["ReflectionQuality"] = p.QualityLevel.ToString()
            };

            foreach (var (key, value) in replace)
            {
                var pattern = $@"\(""{Regex.Escape(key)}"",\s*""[^""]*""\)";
                uiData = Regex.Replace(uiData, pattern, $"(\"{key}\", \"{value}\")");
            }
            return uiData;
        }
    }
}
