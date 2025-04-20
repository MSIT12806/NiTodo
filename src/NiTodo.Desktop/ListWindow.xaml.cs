using DomainInfra;
using Microsoft.Extensions.DependencyInjection;
using NiTodo.App;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NiTodo.Desktop
{
    public class RefreshWindowEventHandler : IDomainEventHandler
    {
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
            var list = service.GetShouldShow();

            // 把每一個待辦項目加進畫面
            foreach (var todo in list)
            {
                AddToPanel(todo);
            }

            // 重新放一個標題或提示
            TodoListPanel.Children.Add(new TextBlock
            {
                Text = "雙擊左鍵以新增代辦事項",
                FontSize = 20,
                Margin = new Thickness(0, 0, 0, 10)
            });
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

        private void TodoListPanel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || e.ClickCount != 2)
            {
                return;
            }

            string newTodo = Microsoft.VisualBasic.Interaction.InputBox("輸入代辦事項：", "新增代辦", "");
            if (!string.IsNullOrWhiteSpace(newTodo))
            {
                var service = App.ServiceProvider.GetRequiredService<TodoService>();
                service.CreateTodo(newTodo);
            }
        }
    }
}
