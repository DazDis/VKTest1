using System.Diagnostics;

namespace VKTest1
{
    public partial class Form1 : Form
    {
        private readonly IBenchmarkService _launcher;
        private readonly IConfigService _configService;
        private readonly ILogService _logService;
        private readonly IBenchmarkAutomationService _automation;

        private readonly System.Threading.CancellationTokenSource _cts = new();

        public Form1()
        {
            InitializeComponent();

            _launcher = new BenchmarkService();
            _configService = new ConfigService();
            _logService = new LogService();
            _automation = new BenchmarkAutomationService();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await RunAllTestsAsync();
        }

        private async Task RunAllTestsAsync()
        {
            if (!_launcher.IsFound)
            {
                MessageBox.Show(
                    "Бенчмарк не найден.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!_launcher.IsSteamRunning())
            {
                var result = MessageBox.Show(
                    "Steam не запущен. \r\n\r\n" +
                    "Запустить Steam сейчас?",
                    "Steam не найден",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "steam://open/main",
                            UseShellExecute = true
                        });

                        Text = "Ожидание запуска Steam...";
                        for (int i = 0; i < 60; i++)
                        {
                            await Task.Delay(1000, _cts.Token);
                            if (_launcher.IsSteamRunning()) break;
                        }

                        if (!_launcher.IsSteamRunning())
                        {
                            MessageBox.Show(
                                "Steam не запустился за отведённое время.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Close();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Не удалось запустить Steam: {ex.Message}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    Close();
                    return;
                }
            }
            try
            {
                var cpuLog = await RunSingleTestAsync(BenchmarkConfig.CreateCpuDefault());
                if (cpuLog != null)
                {
                    ShowTestResult(rtbTest1, cpuLog);
                    ShowSystemInfo(rtbSystem, cpuLog);
                }

                var gpuLog = await RunSingleTestAsync(BenchmarkConfig.CreateGpuDefault());
                if (gpuLog != null)
                    ShowTestResult(rtbTest2, gpuLog);

                _configService.RestoreBackup();
                Text = "Бенчмарк автомайзер — готово";
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<BenchmarkLog?> RunSingleTestAsync(BenchmarkConfig profile)
        {
            var startedAt = DateTime.Now;

            Text = $"Подготовка: {profile.Name}";
            _configService.ApplyProfile(profile);
            await Task.Delay(3000, _cts.Token);

            Text = $"Запуск: {profile.Name}";
            await _launcher.RunAsync(_cts.Token);
            await _automation.ClickSteamLaunchDialogAsync(0.56, 0.62, _cts.Token);
            Text = "Ожидание окна бенчмарка...";
            var ok = await _automation.StartBenchmarkTestAsync(_cts.Token);
            if (!ok)
            {
                MessageBox.Show("Окно бенчмарка не появилось.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            Text = $"Идёт тест: {profile.Name}...";
            var log = await _logService.WaitForNewLogAsync(
                startedAt, TimeSpan.FromMinutes(15), _cts.Token);

            if (log == null)
            {
                MessageBox.Show($"Лог после теста «{profile.Name}» не появился.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _automation.KillBenchmarkProcesses();
                return null;
            }

            Text = $"Закрытие бенчмарка: {profile.Name}...";
            await _automation.CloseBenchmarkAsync(_cts.Token);

            await _automation.WaitForBenchmarkExitAsync(_cts.Token);

            _automation.KillBenchmarkProcesses();

            return log;
        }

        private static void ShowTestResult(RichTextBox box, BenchmarkLog log)
        {
            box.Clear();
            box.AppendText($"Дата: {log.Date:dd.MM.yyyy HH:mm}\r\n");
            box.AppendText("───────── FPS ─────────\r\n");
            box.AppendText($"FPS Avg:    {log.FpsAvg:F1}\r\n");
            box.AppendText($"FPS Min:    {log.FpsMin:F1}\r\n");
            box.AppendText($"FPS Max:    {log.FpsMax:F1}\r\n");
            box.AppendText($"FPS 95th:   {log.Fps95:F1}\r\n");
            box.AppendText("─────── НАГРУЗКА ─-─────\r\n");
            box.AppendText($"CPU Avg:    {log.CpuAvg:F0}%\r\n");
            box.AppendText($"GPU Avg:    {log.GpuAvg:F0}%\r\n");
            box.AppendText($"VRAM Peak:  {log.VideoMem:F2} GB\r\n");
            box.AppendText("─────── НАСТРОЙКИ ──────\r\n");
            box.AppendText($"Разрешение:   {log.ScreenResolution}\r\n");
            box.AppendText($"QualityLevel: {log.QualityLevel}\r\n");
            box.AppendText($"ImageQuality: {log.ImageQuality}\r\n");
            box.AppendText($"ViewDistance: {log.ViewDistance}\r\n");
            box.AppendText($"AntiAliasing: {log.AntiAliasing}\r\n");
            box.AppendText($"PostProcess:  {log.PostProcessing}\r\n");
            box.AppendText($"Shadow:       {log.ShadowQuality}\r\n");
            box.AppendText($"Texture:      {log.TextureQuality}\r\n");
            box.AppendText($"Material:     {log.MaterialQuality}\r\n");
            box.AppendText($"Vegetation:   {log.VegetationQuality}\r\n");
            box.AppendText($"MotionBlur:   {log.MotionBlur}\r\n");
            box.AppendText($"RTX:          {(log.Rtx == 1 ? "вкл" : "выкл")}\r\n");
            box.AppendText($"DLSS:         {log.Dlss}\r\n");
            box.AppendText($"Frame Gen:    {(log.InsertFrame == 1 ? "вкл" : "выкл")}\r\n");
            box.AppendText($"DX12:         {(log.Dx12 == 1 ? "вкл" : "выкл")}\r\n");
        }

        private static void ShowSystemInfo(RichTextBox box, BenchmarkLog log)
        {
            box.Clear();
            box.AppendText($"CPU:      {log.CpuModel}\r\n");
            box.AppendText($"GPU:      {log.GpuModel}\r\n");
            box.AppendText($"Драйвер:  {log.GpuDriverVer}\r\n");
            box.AppendText($"VRAM:     {log.VideoMemSize}\r\n");
            box.AppendText($"RAM:      {log.SysMem}\r\n");
            box.AppendText("──────────────────────\r\n");
            box.AppendText($"ОС:       {log.SysVer}\r\n");
            box.AppendText($"Игра:     {log.GameVer}\r\n");
            box.AppendText($"Экран:    {log.ScreenResolution}\r\n");
            box.AppendText($"Режим:    {log.ScreenMode}\r\n");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_cts.IsCancellationRequested)
                _cts.Cancel();

            base.OnFormClosing(e);
        }
    }
}

