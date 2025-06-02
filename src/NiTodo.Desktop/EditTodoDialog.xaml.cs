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
    public partial class EditTodoDialog : Window
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
            RenderTags(); // 渲染標籤
        }

        private void EditTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TodoItem.Content = ContentEditor.Text.Trim();
        }

        private void PlannedDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TodoItem.PlannedDate = PlannedDatePicker.SelectedDate;
        }

        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            var tag = TagInputTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(tag) && !TodoItem.Tags.Contains(tag))
            {
                AddTag(tag);
                TagInputTextBox.Text = "";
            }
        }
        private void AddTag(string tag)
        {
            if (TodoItem.Tags.Add(tag)) // HashSet.Add() return true 表示成功加入
            {
                RenderTags();
            }
        }

        private void DeleteTag_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag && TodoItem.Tags.Remove(tag))
            {
                RenderTags();
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
                var deleteBtn = new Button
                {
                    Content = "X",
                    Width = 16,
                    Height = 16,
                    FontSize = 10,
                    Margin = new Thickness(5, 0, 0, 0),
                    Tag = tag
                };
                deleteBtn.Click += DeleteTag_Click;

                panel.Children.Add(tagText);
                panel.Children.Add(deleteBtn);
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
