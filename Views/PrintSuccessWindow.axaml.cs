using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace PrintblocProject;

[DoNotNotify]
public partial class PrintSuccessWindow : Window
{
    private int JobId;
    private string Status;
    private string Message;
    private int counter;
    private string fileExtension;
    public PrintSuccessWindow(int JobId, string Status, string Message, int counter, string fileExtension = null)
    {
        this.InitializeComponent();
        this.JobId = JobId;
        this.Status = Status;
        this.Message = Message;
        this.counter = counter;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}