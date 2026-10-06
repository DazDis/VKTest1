using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VKTest1
{
    public interface IBenchmarkAutomationService
    {
        Task<bool> StartBenchmarkTestAsync(CancellationToken token = default);
        Task WaitForBenchmarkExitAsync(CancellationToken token = default);
        void KillBenchmarkProcesses();
        Task CloseBenchmarkAsync(CancellationToken token = default);
        Task ClickSteamLaunchDialogAsync(double screenX, double screenY, CancellationToken token = default);
    }
    public class BenchmarkAutomationService : IBenchmarkAutomationService
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy,
                                               uint dwData, UIntPtr dwExtraInfo);

        private const int SW_RESTORE = 9;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const int ButtonOffsetX = 220;
        private const int ButtonOffsetY = 490;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        private const double ButtonRelativeX = 0.1146;
        private const double ButtonRelativeY = 0.4537;


        public async Task<bool> StartBenchmarkTestAsync(CancellationToken token = default)
        {
            var hwnd = await WaitForWindowAsync(token);
            if (hwnd == IntPtr.Zero) return false;

            ShowWindow(hwnd, SW_RESTORE);
            SetForegroundWindow(hwnd);

            await Task.Delay(30000, token);
            SendKeys.SendWait(" ");
            await Task.Delay(2000, token);

            await ClickAtAsync(hwnd, ButtonRelativeX, ButtonRelativeY, token);
            await Task.Delay(2000, token);

            SendKeys.SendWait("{ENTER}");
            return true;
        }

        private async Task<IntPtr> WaitForWindowAsync(CancellationToken token)
        {
            var processNames = new[]
            {
                "b1",
                "BootstrapPackagedGame",
                "b1-Win64-Shipping"
            };

            for (int i = 0; i < 60; i++)
            {
                token.ThrowIfCancellationRequested();

                foreach (var name in processNames)
                {
                    var proc = Process.GetProcessesByName(name)
                        .FirstOrDefault(p =>
                        {
                            try { return p.MainWindowHandle != IntPtr.Zero; }
                            catch { return false; }
                        });

                    if (proc != null)
                        return proc.MainWindowHandle;
                }

                await Task.Delay(1000, token);
            }

            return IntPtr.Zero;
        }

        private async Task ClickAtAsync(IntPtr hwnd, double relativeX, double relativeY,
                                        CancellationToken token)
        {
            if (!GetWindowRect(hwnd, out var rect))
                throw new InvalidOperationException("Не удалось получить размеры окна.");

            int w = rect.Right - rect.Left;
            int h = rect.Bottom - rect.Top;

            int x = rect.Left + (int)(w * relativeX);
            int y = rect.Top + (int)(h * relativeY);

            Debug.WriteLine($"Клик по экранным координатам ({x}, {y}), окно: {w}x{h}");

            ShowWindow(hwnd, SW_RESTORE);
            SetForegroundWindow(hwnd);
            await Task.Delay(500, token);

            SetCursorPos(x, y);
            await Task.Delay(300, token);

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(80, token);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        }
        public async Task WaitForBenchmarkExitAsync(CancellationToken token = default)
        {
            var names = new[] { "b1", "b1-Win64-Shipping", "BootstrapPackagedGame" };

            for (int i = 0; i < 1200; i++)
            {
                token.ThrowIfCancellationRequested();

                bool anyAlive = names.Any(n =>
                {
                    try { return Process.GetProcessesByName(n).Length > 0; }
                    catch { return false; }
                });

                if (!anyAlive) return;

                await Task.Delay(1000, token);
            }

            throw new TimeoutException("Бенчмарк не закрылся за 20 минут.");
        }

        public void KillBenchmarkProcesses()
        {
            var names = new[] { "b1", "b1-Win64-Shipping", "BootstrapPackagedGame" };
            foreach (var name in names)
            {
                try
                {
                    foreach (var p in Process.GetProcessesByName(name))
                    {
                        try { p.Kill(entireProcessTree: true); p.WaitForExit(2000); }
                        catch { }
                    }
                }
                catch { }
            }
        }
        public async Task CloseBenchmarkAsync(CancellationToken token = default)
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                token.ThrowIfCancellationRequested();

                var hwnd = FindBenchmarkWindow();
                if (hwnd == IntPtr.Zero)
                    return;

                ShowWindow(hwnd, SW_RESTORE);
                SetForegroundWindow(hwnd);
                await Task.Delay(500, token);

                SendKeys.SendWait("{ENTER}");
                await Task.Delay(1500, token);

                if (FindBenchmarkWindow() == IntPtr.Zero)
                    return;

                SendKeys.SendWait("%{F4}");
                await Task.Delay(1500, token);

                if (FindBenchmarkWindow() == IntPtr.Zero)
                    return;
            }

            KillBenchmarkProcesses();
        }

        private IntPtr FindBenchmarkWindow()
        {
            var names = new[] { "b1", "b1-Win64-Shipping", "BootstrapPackagedGame" };

            foreach (var name in names)
            {
                try
                {
                    var proc = Process.GetProcessesByName(name)
                        .FirstOrDefault(p =>
                        {
                            try { return p.MainWindowHandle != IntPtr.Zero; }
                            catch { return false; }
                        });

                    if (proc != null) return proc.MainWindowHandle;
                }
                catch { }
            }

            return IntPtr.Zero;
        }
        public async Task ClickSteamLaunchDialogAsync(double relativeX, double relativeY, CancellationToken token = default)
        {
            var hwnd = await WaitForSteamDialogAsync(token);
            if (hwnd == IntPtr.Zero) return;

            if (!GetWindowRect(hwnd, out var rect))
                return;

            int w = rect.Right - rect.Left;
            int h = rect.Bottom - rect.Top;

            int x = rect.Left + (int)(w * relativeX);
            int y = rect.Top + (int)(h * relativeY);

            Debug.WriteLine($"[Steam] Window {rect.Left},{rect.Top} {w}x{h}, click at {x},{y}");

            ShowWindow(hwnd, SW_RESTORE);
            SetForegroundWindow(hwnd);
            await Task.Delay(800, token);

            SetCursorPos(x, y);
            await Task.Delay(300, token);

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(80, token);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(500, token);
        }

        private async Task<IntPtr> WaitForSteamDialogAsync(CancellationToken token)
        {
            var candidates = new[] { "steam", "steamwebhelper" };

            for (int i = 0; i < 40; i++)
            {
                token.ThrowIfCancellationRequested();

                foreach (var name in candidates)
                {
                    try
                    {
                        foreach (var p in Process.GetProcessesByName(name))
                        {
                            if (p.MainWindowHandle != IntPtr.Zero)
                            {
                                if (IsWindowVisible(p.MainWindowHandle))
                                    return p.MainWindowHandle;
                            }
                        }
                    }
                    catch { }
                }

                await Task.Delay(500, token);
            }

            return IntPtr.Zero;
        }

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
    }
}