using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PrintblocProject.ViewModels;
using PrintblocProject.Views;
using PropertyChanged;
using System.Threading;

namespace PrintblocProject
{
    [DoNotNotify]
    public partial class App : Application
    {
        private static Mutex _mutex = null;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                const string appName = "PrintLoc";
                bool createdNew;
                _mutex = new Mutex(true, appName, out createdNew);
                if(createdNew)
                {
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = new MainWindowViewModel(),
                    };
                }else
                {
                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
                    {
                        desktopApp.MainWindow.Activate();
                    }
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}