using AgConn.CrossPlatform.ViewModels;
using AgConn.CrossPlatform.Views;
using HanumanInstitute.MvvmDialogs.Avalonia;

namespace AgConn.CrossPlatform;

/// <summary>
/// Maps view models to views in Avalonia.
/// </summary>
public class ViewLocator : StrongViewLocator
{
    public ViewLocator()
    {
        ForceSinglePageNavigation = false;
        Register<MainViewModel, MainView, MainWindow>();
        Register<CommSettingsViewModel, CommSettingsView, CommSettingsWindow>();
        Register<UDPViewModel, UDPView, UDPWindow>();
        Register<NtripViewModel, NtripView, NtripWindow>();
        Register<UDPMonitorViewModel, UDPMonitorView, UDPMonitorWindow>();
        Register<EthernetViewModel, EthernetView, EthernetWindow>();
        Register<GPSDataViewModel, GPSDataView, GPSDataWindow>();
    }
}
