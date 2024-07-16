using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AgConn.CrossPlatform.Views;

public partial class CommSettingsWindow : Window
{
    public CommSettingsWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

