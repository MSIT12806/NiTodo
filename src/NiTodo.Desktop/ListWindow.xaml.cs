﻿using DomainInfra;
using Microsoft.Extensions.DependencyInjection;
using NiTodo.App;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

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
        TodoService service = App.ServiceProvider.GetRequiredService<TodoService>();
        private List<TodoItem> todoList => service.ShowTodo().ToList();
        private List<TodoItem> todoListForShow => service
            .ShowTodo(ShowCompletedCheckBox.IsChecked ?? false)
            .Where(i => (ShowTodayCheckBox.IsChecked ?? false) ? IsToday(i) : true).ToList();

        private static bool IsToday(TodoItem i)
        {
            // 如果 PlannedDate 為 null，則視為今天
            return ((i.PlannedDate ?? DateTime.Today).Date == DateTime.Today);
        }

        private HashSet<string> checkedTags = new HashSet<string>();
        private readonly DispatcherTimer _highlightTimer = new DispatcherTimer();
        public ListWindow()
        {
            InitializeComponent();

            // 註冊事件處理器
            var domainEventDispatcher = App.ServiceProvider.GetRequiredService<DomainEventDispatcher>();
            var refreshWindowHandler = new RefreshWindowEventHandler(this);
            domainEventDispatcher.Register(refreshWindowHandler);

            RefreshWindow(); // 初始化畫面

            // 啟動定時器
            _highlightTimer.Interval = TimeSpan.FromSeconds(5);
            _highlightTimer.Tick += HighlightDueTodos;
            _highlightTimer.Start();
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

        #region Render Window
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

            // 把每一個待辦項目加進畫面
            if (checkedTags.Count > 0)
            {
                foreach (var todo in todoListForShow.Where(i => i.Tags.Any(t => checkedTags.Contains(t))))
                {
                    AddToPanel(todo);
                }
            }
            else
            {
                foreach (var todo in todoListForShow)
                {
                    AddToPanel(todo);
                }
            }

            RenderTagFilters();
        }

        private void RenderTagFilters()
        {
            // 清掉舊的 tag checkbox（保留固定的前兩個）
            while (FilterPanel.Children.Count > 2)
                FilterPanel.Children.RemoveAt(2);

            var tags = todoListForShow
                .SelectMany(t => t.Tags)
                .Distinct()
                .OrderBy(t => t);

            foreach (var tag in tags)
            {
                var cb = new CheckBox
                {
                    Content = tag,
                    Tag = tag,
                    Margin = new Thickness(5, 0, 0, 0)
                };
                cb.IsChecked = checkedTags.Contains(tag);
                cb.Checked += FilterChanged;
                cb.Unchecked += FilterChanged;
                FilterPanel.Children.Add(cb);
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
            // 綁定 CheckBox
            checkBox.IsChecked = todo.IsCompleted;
            checkBox.Checked += async (s, e) => await OnTodoItemChecked(todo);

            var todoTextBlock = new TextBlock
            {
                Text = todo.Content,
                FontSize = 16,
                Margin = new Thickness(0, 5, 0, 5)
            };
            todoTextBlock.MouseDown += (s, e) =>
            {
                // double click
                if (e.ClickCount == 2)
                {
                    Clipboard.SetText(todo.Content);
                    ToastManager.ShowToast($"{todo.Content} 已複製");
                }
            };
            if (todo.IsCompleted)
            {
                // 綁定刪除線
                todoTextBlock.TextDecorations = TextDecorations.Strikethrough;
            }

            var dateTimeBlock = new TextBlock
            {
                Text = todo.PlannedDate.HasValue ? todo.PlannedDate.Value.ToString("yyyy-MM-dd HH:mm") : "",
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 10,
                Foreground = Brushes.Gray
            };

            // 加入「編輯」按鈕
            var editButton = new Button
            {
                Content = "編輯",
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(5, 2, 5, 2),
            };
            editButton.Click += (s, e) => EditTodoItem(todo);

            // 加入所有元素到 stack
            stack.Children.Add(checkBox);
            stack.Children.Add(todoTextBlock);
            if (todo.PlannedDate.HasValue && todo.PlannedDate.Value.Date == DateTime.Today)
            {
                stack.Children.Add(dateTimeBlock);
            }
            stack.Children.Add(editButton);

            TodoListPanel.Children.Add(stack);

            var now = DateTime.Now;
            if (todo.WillExpireInNext(10))
            {
                stack.Background = new SolidColorBrush(Color.FromRgb(255, 255, 0)); // LightYellow
            }
            if (todo.IsExpired())
            {
                stack.Background = new SolidColorBrush(Color.FromRgb(255, 182, 193)); // LightPink
            }
        }
        #endregion

        #region Search Area
        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var exceptCheckContent = new string[] { "已完成", "今天" };
            if (exceptCheckContent.Contains(checkBox.Content as string) == false)
            {
                if (checkBox.IsChecked == true)
                {
                    checkedTags.Add(checkBox.Content as string);
                }
                else
                {
                    checkedTags.Remove(checkBox.Content as string);
                }
            }

            RefreshWindow();
        }

        #endregion 
        private async Task OnTodoItemChecked(TodoItem item)
        {
            var service = App.ServiceProvider.GetRequiredService<TodoService>();
            service.CompleteTodo(item.Id);
        }

        private void EditTodoItem(TodoItem todo)
        {
            var inputDialog = new EditTodoDialog(todo);
            if (inputDialog.ShowDialog() == true)
            {
                service.UpdateTodo(todo);
                RefreshWindow();
            }
        }

        private void HighlightDueTodos(object sender, EventArgs e)
        {
            RefreshWindow();

            foreach (var todo in todoList)
            {
                var now = DateTime.Now;
                if (todo.IsExpired() && todo.WasExpiredBefore(1))
                    ToastManager.ShowToast($"{todo.Content} 已到期！");
            }
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

        #region 新增待辦項目

        private void AddTodoButton_Click(object sender, RoutedEventArgs e)
        {
            AddTodoItem();
        }
        private void NewTodoTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddTodoItem();
                e.Handled = true; // 避免系統音效或額外事件
            }
        }

        private void AddTodoItem()
        {
            string content = NewTodoTextBox.Text.Trim();

            // 確認不是 placeholder
            if (string.IsNullOrWhiteSpace(content) || content == (string)NewTodoTextBox.Tag)
                return;

            var service = App.ServiceProvider.GetRequiredService<TodoService>();
            service.CreateTodo(content);

            NewTodoTextBox.Text = ""; // 清空輸入框
        }

        #endregion
    }
}
