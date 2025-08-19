using NiTodo.App;
using System;
using System.Windows;
using System.Windows.Input;

namespace NiTodo.Desktop
{
    public partial class QuickAddTodoWindow : Window
    {
        private readonly NiTodoApp _app;
        public QuickAddTodoWindow(NiTodoApp app)
        {
            InitializeComponent();
            _app = app;
            this.Loaded += (s, e) => {
                this.Activate();
                this.Topmost = true;
                TodoInput.Focus();
                TodoInput.SelectAll();
            };
            this.PreviewKeyDown += QuickAddTodoWindow_PreviewKeyDown;
        }

        private void QuickAddTodoWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.Close();
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var content = TodoInput.Text.Trim();
            if (!string.IsNullOrEmpty(content))
            {
                _app.CreateTodo(content);
                this.Close();
            }
            else
            {
                MessageBox.Show("請輸入待辦事項內容。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
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
            string content = TodoInput.Text.Trim();

            // 確認不是 placeholder
            if (string.IsNullOrWhiteSpace(content) || content == (string)TodoInput.Tag)
                return;

            _app.CreateTodo(content);
            this.Close();
        }
    }
}
