using System;
using System.Windows;
using System.Windows.Threading;

namespace NiTodo.Desktop
{
    public partial class ToastWindow : Window
    {
        public ToastWindow(string message)
        {
            InitializeComponent();
            MessageTextBlock.Text = message;

            // 設定位置在右下角
            var desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - Width - 10;
            Top = desktopWorkingArea.Bottom - Height - 10;

            // 5 秒後自動關閉
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                Close();
            };
            timer.Start();//這個要移除掉?
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 可加入淡入動畫（可選）
        }
    }
    public static class ToastManager
    {
        private static List<ToastWindow> _activeToasts = new();

        public static void ShowToast(string message)
        {
            var toast = new ToastWindow(message);

            // 計算要顯示的位置
            double offset = _activeToasts.Count * (toast.Height + 10); // 每個間隔10px
            var workArea = SystemParameters.WorkArea;

            toast.Left = workArea.Right - toast.Width - 10;
            toast.Top = workArea.Bottom - toast.Height - 10 - offset;

            _activeToasts.Add(toast);

            // 移除已消失的 toast
            toast.Closed += (s, e) =>
            {
                _activeToasts.Remove(toast);
                RearrangeToasts();
            };

            toast.Show();
        }

        private static void RearrangeToasts()
        {
            for (int i = 0; i < _activeToasts.Count; i++)
            {
                var toast = _activeToasts[i];
                double offset = i * (toast.Height + 10);
                toast.Top = SystemParameters.WorkArea.Bottom - toast.Height - 10 - offset;
            }
        }
    }
}
