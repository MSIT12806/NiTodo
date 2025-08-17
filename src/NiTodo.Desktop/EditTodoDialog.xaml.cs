using NiTodo.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NiTodo.Desktop
{
    /// <summary>
    /// EditTodoDialog.xaml 的互動邏輯
    /// </summary>
    public partial class EditTodoDialog : MahApps.Metro.Controls.MetroWindow
    {
        TodoItem TodoItem { get; set; }

        public EditTodoDialog(TodoItem todoItem )
        {
            InitializeComponent();
            TodoItem = todoItem ?? throw new ArgumentNullException(nameof(todoItem), "TodoItem cannot be null.");

            RenderWindow();

            ContentEditor.Focus();
            ContentEditor.SelectAll();
        }

        private void RenderWindow()
        {
            ContentEditor.Text = TodoItem.Content;
            PlannedDatePicker.SelectedDate = TodoItem.PlannedDate;
            PlannedTimePicker.SelectedDateTime = TodoItem.PlannedDate?.TimeOfDay == TimeSpan.Zero ? null : TodoItem.PlannedDate;
            RenderTags(); // 渲染標籤
        }

        private void EditTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TodoItem.Content = ContentEditor.Text.Trim();
        }

        private void PlannedDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlannedDatePicker.SelectedDate.HasValue)
            {
                // 保留原本時間（若無則設為 00:00）
                var currentTime = TodoItem.PlannedDate?.TimeOfDay ?? TimeSpan.Zero;
                TodoItem.PlannedDate = PlannedDatePicker.SelectedDate.Value.Date + currentTime;
            }
            else
            {
                // 使用者清空日期 => 取消整個預計時間設定
                TodoItem.PlannedDate = null;
                PlannedTimePicker.SelectedDateTime = null; // 同步清空時間
            }
        }

        private void PlannedTimePicker_SelectedDateTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            if (PlannedTimePicker.SelectedDateTime.HasValue)
            {
                // 若目前沒有日期但設定了時間，維持原行為：以今天作為日期
                var currentDate = TodoItem.PlannedDate?.Date ?? DateTime.Today;
                var newTime = PlannedTimePicker.SelectedDateTime.Value.TimeOfDay;
                TodoItem.PlannedDate = currentDate + newTime;
            }
            else
            {
                // 清空時間 => 若還有日期則將時間歸 00:00，並視為「只有日期」；若沒有日期則保持 null
                if (TodoItem.PlannedDate.HasValue)
                {
                    TodoItem.PlannedDate = TodoItem.PlannedDate.Value.Date; // 時間 00:00
                }
            }
        }

        private void RenderTags()
        {
            TagListPanel.Children.Clear();

            foreach (var tag in TodoItem.Tags)
            {
                var border = new Border
                {
                    Margin = new Thickness(2),
                    Padding = new Thickness(5, 2, 5, 2),
                    Background = Brushes.LightBlue,
                    CornerRadius = new CornerRadius(4)
                };

                var panel = new StackPanel { Orientation = Orientation.Horizontal };

                var tagText = new TextBlock { Text = tag };
               
                panel.Children.Add(tagText);
                border.Child = panel;

                TagListPanel.Children.Add(border);
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // 讓呼叫端知道使用者完成了
        }
    }
}
