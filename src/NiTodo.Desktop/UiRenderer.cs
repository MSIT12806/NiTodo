using NiTodo.App.Interfaces;

namespace NiTodo.Desktop
{
    public class UiRenderer : IUiRenderer
    {
        private readonly ListWindow ListWindow;
        public UiRenderer(ListWindow listWindow)
        {
            ListWindow = listWindow ?? throw new ArgumentNullException(nameof(listWindow));
        }
        public void Render()
        {
            if (ListWindow == null)
            {
                throw new InvalidOperationException("ListWindow is not initialized.");
            }
            ListWindow.RefreshWindow();
        }
    }
}