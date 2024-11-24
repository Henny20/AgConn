using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AgConn.CrossPlatform.Views;

public partial class EthernetView : UserControl
{
    public EthernetView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

