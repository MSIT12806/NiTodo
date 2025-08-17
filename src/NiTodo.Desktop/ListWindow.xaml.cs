using DomainInfra;
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
        NiTodoApp niTodoApp = App.ServiceProvider.GetRequiredService<NiTodoApp>();
        private List<TodoItem> todoListForShow => niTodoApp
            .ShowTodo(ShowCompletedCheckBox.IsChecked ?? false, ShowTodayCheckBox.IsChecked ?? false);


        private SortMode _currentSort = SortMode.Content;

        private static bool IsToday(TodoItem i)
        {
            // 如果 PlannedDate 為 null，則視為今天
            return ((i.PlannedDate ?? DateTime.Today).Date == DateTime.Today);
        }

        // 三態標籤篩選：Ignore -> 不管, Include -> 需包含, Exclude -> 不可包含
        private enum TagFilterState { Ignore, Include, Exclude }
        private readonly Dictionary<string, TagFilterState> _tagStates = new();
        private readonly DispatcherTimer _highlightTimer = new DispatcherTimer();
        // 選取狀態：以 TodoId 保存，避免重繪後遺失
        private string _selectedTodoId;
        private readonly Brush _selectedBrush = Brushes.DarkGray;
        private readonly Dictionary<string, Grid> _todoGridMap = new();
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
            if (TodoListPanel == null)
                return; // 尚未初始化 XAML 元件時避免 NullReference
            if (!Dispatcher.CheckAccess())
            {
                // 如果現在不是在 UI 執行緒，就切回 UI 執行緒執行
                Dispatcher.Invoke(RefreshWindow);
                return;
            }

            // 清空目前 StackPanel 裡的動態內容
            TodoListPanel.Children.Clear();
            _todoGridMap.Clear();

            // 準備集合
            IEnumerable<TodoItem> items = todoListForShow;
            var includeTags = _tagStates.Where(kv => kv.Value == TagFilterState.Include).Select(kv => kv.Key).ToList();
            var excludeTags = _tagStates.Where(kv => kv.Value == TagFilterState.Exclude).Select(kv => kv.Key).ToHashSet();
            if (includeTags.Any())
                items = items.Where(i => i.Tags.Any(t => includeTags.Contains(t)));
            if (excludeTags.Any())
                items = items.Where(i => !i.Tags.Any(t => excludeTags.Contains(t)));

            // 排序
            items = _currentSort switch
            {
                SortMode.Content => items.OrderBy(i => i.Content, StringComparer.CurrentCultureIgnoreCase),
                SortMode.Planned => items.OrderBy(i => i.PlannedDate.HasValue ? 0 : 1) // 無預計時間放後面
                                           .ThenBy(i => i.PlannedDate),
                SortMode.Created => items.OrderBy(i => i.CreatedAt.HasValue ? 0 : 1)
                                           .ThenBy(i => i.CreatedAt),
                _ => items
            };

            foreach (var todo in items)
            {
                AddToPanel(todo);
            }

            ReapplySelection();

            RenderTagFilters();
        }
        private void RenderTagFilters()
        {
            if (FilterPanel == null)
                return;
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
                    Margin = new Thickness(5, 0, 0, 0),
                    IsThreeState = true
                };
                if (_tagStates.TryGetValue(tag, out var state))
                {
                    cb.IsChecked = state switch
                    {
                        TagFilterState.Include => true,
                        TagFilterState.Exclude => null, // Indeterminate 顯示為排除
                        _ => false
                    };
                }
                cb.Click += TagCheckBox_Click;
                FilterPanel.Children.Add(cb);
            }
        }
        private void AddToPanel(TodoItem todo)
        {
            // 建立容器 Grid：左邊固定寬度給 CheckBox，右邊內容區可換行
            var grid = new Grid
            {
                Margin = new Thickness(0, 5, 0, 5)
            };
            grid.DataContext = todo; // 後續可透過 DataContext 取得 todo
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // checkbox
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // content
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // edit button

            var checkBox = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 3, 8, 0)
            };
            checkBox.IsChecked = todo.IsCompleted;
            checkBox.Checked += async (s, e) => await OnTodoItemChecked(todo);
            Grid.SetColumn(checkBox, 0);

            // 文字 + 日期再用一個 StackPanel，文字可換行
            var contentPanel = new StackPanel { Orientation = Orientation.Vertical };

            var todoTextBlock = new TextBlock
            {
                Text = todo.Content,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 2)
            };
            todoTextBlock.MouseDown += (s, e) =>
            {
                if (e.ClickCount == 2)
                {
                    niTodoApp.CopyContent(todo);
                    ToastManager.ShowToast($"{todo.GetContentWithoutPrefix()} 已複製");
                }
            };
            if (todo.IsCompleted)
            {
                todoTextBlock.TextDecorations = TextDecorations.Strikethrough;
            }

            contentPanel.Children.Add(todoTextBlock);
            if (todo.PlannedDate.HasValue)
            {
                var dateTimeBlock = new TextBlock
                {
                    Text = todo.PlannedDate.Value.ToString("yyyy-MM-dd HH:mm"),
                    FontSize = 10,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 0, 0, 0)
                };
                contentPanel.Children.Add(dateTimeBlock);
            }
            Grid.SetColumn(contentPanel, 1);

            var editButton = new Button
            {
                Content = "編輯",
                Margin = new Thickness(8, 0, 0, 0),
                Padding = new Thickness(5, 2, 5, 2),
                VerticalAlignment = VerticalAlignment.Top
            };
            editButton.Click += (s, e) => EditTodoItem(todo);
            Grid.SetColumn(editButton, 2);

            grid.Children.Add(checkBox);
            grid.Children.Add(contentPanel);
            grid.Children.Add(editButton);

            TodoListPanel.Children.Add(grid);
            _todoGridMap[todo.Id] = grid;

            if (todo.WillExpireInNext(10))
            {
                grid.Background = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            }
            if (todo.IsExpired())
            {
                grid.Background = new SolidColorBrush(Color.FromRgb(255, 182, 193));
            }

            // 記住原本背景（可能是 null / 黃 / 粉）供取消選取時還原
            grid.Tag = grid.Background;

            // 點擊選取
            grid.MouseLeftButtonDown += (s, e) => SelectTodo(todo.Id);
        }
        #endregion

        #region Search Area
        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            // 處理固定的二態篩選 (已完成 / 今天)
            RefreshWindow();
        }

        private void TagCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox cb)
                return;
            var tag = cb.Content as string;
            if (string.IsNullOrWhiteSpace(tag))
                return;

            TagFilterState state = TagFilterState.Ignore;
            if (cb.IsChecked == true)
                state = TagFilterState.Include;
            else if (cb.IsChecked == null)
                state = TagFilterState.Exclude;

            if (state == TagFilterState.Ignore)
                _tagStates.Remove(tag);
            else
                _tagStates[tag] = state;

            RefreshWindow();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem is ComboBoxItem cbi)
            {
                var tag = cbi.Tag as string;
                _currentSort = tag switch
                {
                    "Content" => SortMode.Content,
                    "Created" => SortMode.Created,
                    "Planned" => SortMode.Planned,
                    _ => SortMode.Content
                };
                RefreshWindow();
            }
        }

        #endregion 
        private async Task OnTodoItemChecked(TodoItem item)
        {
            var service = App.ServiceProvider.GetRequiredService<NiTodoApp>();
            service.CompleteTodo(item.Id);
        }
        private void EditTodoItem(TodoItem todo)
        {
            var inputDialog = new EditTodoDialog(todo);
            if (inputDialog.ShowDialog() == true)
            {
                niTodoApp.UpdateTodo(todo);
                RefreshWindow();
            }
        }
        private void HighlightDueTodos(object sender, EventArgs e)
        {
            RefreshWindow();
            var now = DateTime.Now;
            foreach (var todo in niTodoApp.GetAllTodos())
            {
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

            var service = App.ServiceProvider.GetRequiredService<NiTodoApp>();
            service.CreateTodo(content);

            NewTodoTextBox.Text = ""; // 清空輸入框
        }

        private void SelectTodo(string todoId)
        {
            if (string.IsNullOrEmpty(todoId))
                return;
            if (_selectedTodoId == todoId)
                return;

            // 還原前一個
            if (!string.IsNullOrEmpty(_selectedTodoId) && _todoGridMap.TryGetValue(_selectedTodoId, out var oldGrid))
            {
                oldGrid.Background = oldGrid.Tag as Brush;
            }

            _selectedTodoId = todoId;
            if (_todoGridMap.TryGetValue(todoId, out var grid))
            {
                grid.Background = _selectedBrush;
            }
        }

        private void ReapplySelection()
        {
            if (string.IsNullOrEmpty(_selectedTodoId))
                return;
            if (_todoGridMap.TryGetValue(_selectedTodoId, out var grid))
            {
                // 確保 grid 原背景 Tag 正確（AddToPanel 已設定）
                grid.Background = _selectedBrush;
            }
        }

        #endregion
    }
}
