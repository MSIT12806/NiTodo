using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Extensions.DependencyInjection;
using NiTodo.App;

namespace NiTodo.Desktop
{
    public partial class App : Application
    {
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;
        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private HwndSource _source;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            RegisterServices();
            var mainWindow = new ListWindow();
            mainWindow.Show();
            Application.Current.MainWindow = mainWindow;
            RegisterAppHotKey();
            mainWindow.Activated += (s, ev) => RegisterAppHotKey();
            mainWindow.Closed += (s, ev) => UnregisterAppHotKey();

        }

        private void RegisterAppHotKey()
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow == null) return;
            var helper = new WindowInteropHelper(mainWindow);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            // Ctrl+I
            RegisterHotKey(helper.Handle, HOTKEY_ID, 0x0002, 0x49); // MOD_CONTROL=0x2, VK_I=0x49
        }

        private void UnregisterAppHotKey()
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow == null) return;
            var helper = new WindowInteropHelper(mainWindow);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                ShowQuickAddWindow();
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void ShowQuickAddWindow()
        {
            var app = AppServices.ServiceProvider.GetRequiredService<NiTodoApp>();
            var win = new QuickAddTodoWindow(app);
            win.ShowDialog();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_source != null)
            {
                _source.RemoveHook(HwndHook);
                var mainWindow = Current.MainWindow;
                if (mainWindow != null)
                {
                    var helper = new WindowInteropHelper(mainWindow);
                    UnregisterHotKey(helper.Handle, HOTKEY_ID);
                }
            }
            base.OnExit(e);
        }
    }
}
