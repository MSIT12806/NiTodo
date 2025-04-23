﻿using DomainInfra;
using Microsoft.Extensions.DependencyInjection;
using NiTodo.App;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NiTodo.Desktop
{
    public class RefreshWindowEventHandler : IDomainEventHandler
    {
        //TODO: 這可能要跟框架分手，因為 這個類應該是 App Layer 的一部分
        private readonly ListWindow _listWindow;
        public RefreshWindowEventHandler(ListWindow listWindow)
        {
            _listWindow = listWindow;
        }
        public void Handle(IDomainEvent domainEvent)
        {
            _listWindow.RefreshWindow();
        }
        public bool IsThisEvent(IDomainEvent domainEvent)
        {
            return domainEvent is TodoCreatedEvent
                || domainEvent is TodoCompletedEvent
                || domainEvent is TodoCompletedAfterFiveSecondsEvent;
        }
    }
    /// <summary>
    /// ListWindow.xaml 的互動邏輯
    /// </summary>
    public partial class ListWindow : Window
    {
        public ListWindow()
        {
            InitializeComponent();

            // 註冊事件處理器
            var domainEventDispatcher = App.ServiceProvider.GetRequiredService<DomainEventDispatcher>();
            var refreshWindowHandler = new RefreshWindowEventHandler(this);
            domainEventDispatcher.Register(refreshWindowHandler);

            RefreshWindow(); // 初始化畫面
        }

        #region 自訂標題列
        /// <summary>
        /// 拖曳移動視窗
        /// </summary>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /// <summary>
        /// 最小化視窗
        /// </summary>
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 關閉視窗
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Opacity = e.NewValue;
        }

        private void TopmostCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Topmost = true;
        }

        private void TopmostCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Topmost = false;
        }
        #endregion

        public void RefreshWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                // 如果現在不是在 UI 執行緒，就切回 UI 執行緒執行
                Dispatcher.Invoke(RefreshWindow);
                return;
            }

            // 清空目前 StackPanel 裡的動態內容（保留第一個提示文字）
            TodoListPanel.Children.Clear();
            var service = App.ServiceProvider.GetRequiredService<TodoService>();
            var list = service.ShowTodo();

            // 把每一個待辦項目加進畫面
            foreach (var todo in list)
            {
                AddToPanel(todo);
            }
        }

        private void AddToPanel(TodoItem todo)
        {
            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };

            var checkBox = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var todoTextBlock = new TextBlock
            {
                Text = todo.Content,
                FontSize = 16,
                Margin = new Thickness(0, 5, 0, 5)
            };

            // 綁定 CheckBox
            checkBox.IsChecked = todo.IsCompleted;
            checkBox.Checked += async (s, e) => await OnTodoItemChecked(todo);

            // 綁定刪除線
            if (todo.IsCompleted)
            {
                todoTextBlock.TextDecorations = TextDecorations.Strikethrough;
            }

            stack.Children.Add(checkBox);
            stack.Children.Add(todoTextBlock);

            TodoListPanel.Children.Add(stack);
        }
        private async Task OnTodoItemChecked(TodoItem item)
        {
            var service = App.ServiceProvider.GetRequiredService<TodoService>();
            service.CompleteTodo(item.Id);
        }

        #region 用來模擬 placeholder 行為
        private void NewTodoTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Text == "" && tb.Tag is string placeholder)
            {
                tb.Foreground = Brushes.Black;
                tb.Text = "";
            }
        }

        private void NewTodoTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = "";
                tb.Foreground = Brushes.Gray;
                tb.Text = (string)tb.Tag;
            }
        }
        #endregion

        private void AddTodoButton_Click(object sender, RoutedEventArgs e)
        {
            string content = NewTodoTextBox.Text.Trim();

            // 確認不是 placeholder
            if (string.IsNullOrWhiteSpace(content) || content == (string)NewTodoTextBox.Tag)
                return;

            var service = App.ServiceProvider.GetRequiredService<TodoService>();
            service.CreateTodo(content);

            NewTodoTextBox.Text = ""; // 清空輸入框
        }
    }
}
