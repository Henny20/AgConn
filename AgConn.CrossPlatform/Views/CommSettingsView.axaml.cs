using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AgConn.CrossPlatform.Views;

public partial class CommSettingsView : UserControl
{
    public CommSettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

