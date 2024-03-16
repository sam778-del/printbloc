using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using PrintblocProject.Helpers;
using PrintblocProject.Model;
using PropertyChanged;
using System.Threading.Tasks;


namespace PrintblocProject
{
    [DoNotNotify]
    public partial class ProcessingWindow : Window
    {
        private PrintJobModel printJob;

        public ProcessingWindow(PrintJobModel printJob = null)
        {
            this.InitializeComponent();
            this.printJob = printJob;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Activate();
        }

        private async Task ProcessPrintJobAsync()
        {
            await Task.Run(async () =>
            {
                SignalRBackgroundService signalRBackgroundService = new SignalRBackgroundService();
                await signalRBackgroundService.SendToPrinter(printJob);

                string status = "processing";
                string message = "Processing your print";

                // Assuming AccountManager.updatePrintJob is also an asynchronous operation
                await AccountManager.updatePrintJob(printJob.Id, status, message, 0, "None");
            });
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
