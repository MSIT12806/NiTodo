using DomainInfra;
using Microsoft.Extensions.DependencyInjection;
using NiTodo.App;
using System.Windows;

namespace NiTodo.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            RegisterServices();

            var mainWindow = new ListWindow();
            mainWindow.Show();
        }

        private void RegisterServices()
        {
            var serviceCollection = new ServiceCollection();
            // 註冊服務
            serviceCollection.AddSingleton<DomainEventDispatcher>();
            serviceCollection.AddSingleton<ICopyContent, CopyContent>();
            serviceCollection.AddSingleton<ITodoRepository, FileTodoRepository>();
            serviceCollection.AddTransient<TodoService>();

            //
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }

}
