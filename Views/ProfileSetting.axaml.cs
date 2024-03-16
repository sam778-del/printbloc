using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace PrintblocProject;

[DoNotNotify]
public partial class ProfileSetting : UserControl
{
    public ProfileSetting()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}