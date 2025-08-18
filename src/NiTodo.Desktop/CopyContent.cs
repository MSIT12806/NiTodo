using NiTodo.App.Interfaces;

namespace NiTodo.Desktop
{
    public class CopyContent : ICopyContent
    {
        /// <summary>
        /// 複製內容到剪貼簿
        /// </summary>
        /// <param name="content">要複製的內容</param>
        public void Copy(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Content cannot be empty.", nameof(content));
            }
            System.Windows.Clipboard.SetText(content);
        }
    }
}