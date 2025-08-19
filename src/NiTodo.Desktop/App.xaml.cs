using DomainInfra;
using Microsoft.Extensions.DependencyInjection;
using NiTodo.App;
using NiTodo.App.Interfaces;
using System.Windows;

namespace NiTodo.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public static class AppServices
    {
        public static IServiceProvider ServiceProvider { get; set; }
    }

    public partial class App : Application
    {
        private void RegisterServices()
        {
            var serviceCollection = new ServiceCollection();
            // 註冊服務
            serviceCollection.AddSingleton<DomainEventDispatcher>();
            serviceCollection.AddSingleton<ICopyContent, CopyContent>();
            serviceCollection.AddSingleton<ITodoRepository, FileTodoRepository>();
            serviceCollection.AddTransient<NiTodoApp>();
            AppServices.ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }

}
