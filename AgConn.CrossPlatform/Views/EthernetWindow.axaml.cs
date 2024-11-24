using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AgConn.CrossPlatform.Views;

public partial class EthernetWindow : Window
{
    public EthernetWindow()
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

