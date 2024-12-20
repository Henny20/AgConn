using Avalonia.Markup.Xaml;
using AgConn.CrossPlatform.Services;
using AgConn.CrossPlatform.ViewModels;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.Logging;
using Splat;

namespace AgConn.CrossPlatform;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var build = Locator.CurrentMutable;
        var loggerFactory = LoggerFactory.Create(builder => builder.AddFilter(logLevel => true).AddDebug());

        build.RegisterLazySingleton(() => (IDialogService)new DialogService(
            new DialogManager(
                viewLocator: new ViewLocator(),
                logger: loggerFactory.CreateLogger<DialogManager>(),
                dialogFactory: new DialogFactory().AddFluent(messageBoxType: FluentMessageBoxType.ContentDialog)),
            viewModelFactory: x => Locator.Current.GetService(x)));

        SplatRegistrations.Register<MainViewModel>();
        SplatRegistrations.Register<CommSettingsViewModel>();
        SplatRegistrations.Register<UDPViewModel>();
        SplatRegistrations.Register<NtripViewModel>();
        SplatRegistrations.Register<UDPMonitorViewModel>();
        SplatRegistrations.Register<EthernetViewModel>();
        SplatRegistrations.Register<GPSDataViewModel>();
        SplatRegistrations.Register<IAgConnService, AgConnService>();
         ///
        SplatRegistrations.Register<IStorageService, StorageService>();
        SplatRegistrations.SetupIOC();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        DialogService.Show(null, MainViewModel);

        base.OnFrameworkInitializationCompleted();
    }

    public static MainViewModel MainViewModel => Locator.Current.GetService<MainViewModel>()!;
    public static CommSettingsViewModel CommSettingsViewModel => Locator.Current.GetService<CommSettingsViewModel>()!;
    public static UDPViewModel UDPViewModel => Locator.Current.GetService<UDPViewModel>()!;
    public static NtripViewModel NtripViewModel => Locator.Current.GetService<NtripViewModel>()!;
    public static UDPMonitorViewModel UDPMonitorViewModel => Locator.Current.GetService<UDPMonitorViewModel>()!;
    public static EthernetViewModel EthernetViewModel => Locator.Current.GetService<EthernetViewModel>()!;
    public static GPSDataViewModel GPSDataViewModel => Locator.Current.GetService<GPSDataViewModel>()!;
    private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;
    
    public static IAgConnService AgConnService => Locator.Current.GetService<IAgConnService>()!;
         
    public static StrongViewLocator ViewLocator { get; private set; } = default!;
}
