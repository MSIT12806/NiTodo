using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Extensions.DependencyInjection;
using NiSchedule.App;
using NiScheduleApp;
using NiScheduleApp.Interfaces;
using NiScheduleApp.ValueObjects.ScheduleFrequencies;

namespace NiTodo.Desktop
{
    public partial class ScheduleWindow : Window
    {
        private readonly NiSchedule.App.NiScheduleApp _app;

        public ScheduleWindow()
        {
            InitializeComponent();
            _app = AppServices.ServiceProvider.GetRequiredService<NiSchedule.App.NiScheduleApp>();

            LoadList();

            // 確保整個視窗載入完成後再更新面板，避免 InitializeComponent 期間 SelectionChanged 先觸發導致命名元素尚未建立
            Loaded += (s, e) => UpdatePanelsBySelection();
        }

        private void LoadList()
        {
            ScheduleList.ItemsSource = null;
            ScheduleList.ItemsSource = _app.GetAllSchedules();
        }

        private void FrequencyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 如果視窗還沒載入完成，先略過，避免命名元素仍為 null
            if (!IsLoaded) return;
            UpdatePanelsBySelection();
        }

        private void UpdatePanelsBySelection()
        {
            var tag = (FrequencyCombo.SelectedItem as ComboBoxItem)?.Tag as string;
            DailyPanel.Visibility = tag == "Daily" ? Visibility.Visible : Visibility.Collapsed;
            WeeklyPanel.Visibility = tag == "Weekly" ? Visibility.Visible : Visibility.Collapsed;
            MonthlyPanel.Visibility = tag == "Monthly" ? Visibility.Visible : Visibility.Collapsed;
            YearlyPanel.Visibility = tag == "Yearly" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var name = TaskNameTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("請輸入任務名稱");
                return;
            }

            ScheduleFrequency freq;
            try
            {
                var sel = (FrequencyCombo.SelectedItem as ComboBoxItem)?.Tag as string;
                if (sel == "Daily")
                {
                    var hour = ParseInt(HourTextBox.Text, 0, 23, "小時必須在 0~23");
                    var min = ParseInt(MinuteTextBox.Text, 0, 59, "分鐘必須在 0~59");
                    freq = new DailyScheduleFrequency (hour, min);
                }
                else if (sel == "Weekly")
                {
                    var days = new HashSet<DayOfWeek>();
                    if (SunCheck.IsChecked == true) days.Add(DayOfWeek.Sunday);
                    if (MonCheck.IsChecked == true) days.Add(DayOfWeek.Monday);
                    if (TueCheck.IsChecked == true) days.Add(DayOfWeek.Tuesday);
                    if (WedCheck.IsChecked == true) days.Add(DayOfWeek.Wednesday);
                    if (ThuCheck.IsChecked == true) days.Add(DayOfWeek.Thursday);
                    if (FriCheck.IsChecked == true) days.Add(DayOfWeek.Friday);
                    if (SatCheck.IsChecked == true) days.Add(DayOfWeek.Saturday);
                    if (days.Count == 0) throw new Exception("請至少選擇一個星期幾");
                    var hour = ParseInt(W_HourTextBox.Text, 0, 23, "小時必須在 0~23");
                    var min = ParseInt(W_MinuteTextBox.Text, 0, 59, "分鐘必須在 0~59");
                    freq = new WeeklyScheduleFrequency { DaysOfWeek = days, dailySchedule = new DailyScheduleFrequency (hour, min) };
                }
                else if (sel == "Monthly")
                {
                    var day = ParseInt(M_DayTextBox.Text, 1, 31, "日期必須在 1~31");
                    var hour = ParseInt(M_HourTextBox.Text, 0, 23, "小時必須在 0~23");
                    var min = ParseInt(M_MinuteTextBox.Text, 0, 59, "分鐘必須在 0~59");
                    freq = new MonthlyScheduleFrequency { Days = new HashSet<int> { day }, dailySchedule = new DailyScheduleFrequency (hour, min) };
                }
                else // Yearly
                {
                    var months = GetCheckedMonths();
                    if (months.Count == 0) throw new Exception("請至少選擇一個月份");
                    var day = ParseInt(Y_DayTextBox.Text, 1, 31, "日期必須在 1~31");
                    var hour = ParseInt(Y_HourTextBox.Text, 0, 23, "小時必須在 0~23");
                    var min = ParseInt(Y_MinuteTextBox.Text, 0, 59, "分鐘必須在 0~59");
                    freq = new YearlyScheduleFrequency
                    {
                        Months = months,
                        monthlySchedule = new MonthlyScheduleFrequency { Days = new HashSet<int> { day }, dailySchedule = new DailyScheduleFrequency (hour, min) }
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            var item = new ScheduleItem
            {
                Id = Guid.NewGuid().ToString("N"),
                Content = name,
                ScheduleFrequency = freq
            };

            _app.CreateSchedule(item);
            LoadList();
            ClearForm();
        }

        private void DeleteSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id)
            {
                _app.DeleteSchedule(id);
                LoadList();
            }
        }

        private static int ParseInt(string? text, int min, int max, string error)
        {
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                throw new Exception(error);
            if (v < min || v > max) throw new Exception(error);
            return v;
        }

        private HashSet<int> GetCheckedMonths()
        {
            var months = new HashSet<int>();
            for (int i = 1; i <= 12; i++)
            {
                var cb = FindName($"M{i}") as CheckBox;
                if (cb?.IsChecked == true) months.Add(i);
            }
            return months;
        }

        private void ClearForm()
        {
            TaskNameTextBox.Text = string.Empty;
            HourTextBox.Text = string.Empty;
            MinuteTextBox.Text = string.Empty;
            W_HourTextBox.Text = string.Empty;
            W_MinuteTextBox.Text = string.Empty;
            M_DayTextBox.Text = string.Empty;
            M_HourTextBox.Text = string.Empty;
            M_MinuteTextBox.Text = string.Empty;
            Y_DayTextBox.Text = string.Empty;
            Y_HourTextBox.Text = string.Empty;
            Y_MinuteTextBox.Text = string.Empty;
            for (int i = 1; i <= 12; i++)
            {
                var cb = FindName($"M{i}") as CheckBox;
                if (cb != null) cb.IsChecked = false;
            }
            SunCheck.IsChecked = MonCheck.IsChecked = TueCheck.IsChecked = WedCheck.IsChecked = ThuCheck.IsChecked = FriCheck.IsChecked = SatCheck.IsChecked = false;
            FrequencyCombo.SelectedIndex = 0; // Daily
            UpdatePanelsBySelection();
        }
    }

    public class ScheduleFrequencyToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case DailyScheduleFrequency d:
                    return $"每日 {d.Hour:00}:{d.Minute:00}";
                case WeeklyScheduleFrequency w:
                    var days = string.Join("/", w.DaysOfWeek.OrderBy(x => (int)x).Select(ToChtWeek));
                    return $"每週({days}) {w.dailySchedule.Hour:00}:{w.dailySchedule.Minute:00}";
                case MonthlyScheduleFrequency m:
                    var day = m.Days.Any() ? m.Days.Min() : 1;
                    return $"每月{day}日 {m.dailySchedule.Hour:00}:{m.dailySchedule.Minute:00}";
                case YearlyScheduleFrequency y:
                    var month = y.Months.Any() ? y.Months.Min() : 1;
                    var d2 = y.monthlySchedule.Days.Any() ? y.monthlySchedule.Days.Min() : 1;
                    return $"每年{month}月{d2}日 {y.monthlySchedule.dailySchedule.Hour:00}:{y.monthlySchedule.dailySchedule.Minute:00}";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        private static string ToChtWeek(DayOfWeek d) => d switch
        {
            DayOfWeek.Sunday => "日",
            DayOfWeek.Monday => "一",
            DayOfWeek.Tuesday => "二",
            DayOfWeek.Wednesday => "三",
            DayOfWeek.Thursday => "四",
            DayOfWeek.Friday => "五",
            DayOfWeek.Saturday => "六",
            _ => "?"
        };
    }
}
