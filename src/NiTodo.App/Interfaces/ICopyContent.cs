namespace NiTodo.App.Interfaces
{
    public interface ICopyContent
    {
        /// <summary>
        /// 複製內容到剪貼簿
        /// </summary>
        /// <param name="content">要複製的內容</param>
        void Copy(string item);
    }
}