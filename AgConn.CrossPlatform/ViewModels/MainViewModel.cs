using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AgConn.CrossPlatform.Services;
using AgConn.CrossPlatform.Business;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FileSystem;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Windows.Input;
using AgConn.CrossPlatform.Views;
using AgConn.CrossPlatform.Properties;

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;


namespace AgConn.CrossPlatform.ViewModels;

public class MainViewModel : ViewModelBase
{

    private readonly IDialogService _dialogService;
    private readonly IStorageService _storage;
    //private IAgConnService _agConnService;

    // private DispatcherTimer oneSecondLoopTimer = new DispatcherTimer();
    // private DispatcherTimer ntripMeterTimer = new DispatcherTimer();
    AgConnService service = new AgConnService();

    ///
    public ICommand UDPCommand { get; }

    public MainViewModel(IDialogService dialogService, IStorageService storage) //, IAgConnService agConnService)
    {
        this._dialogService = dialogService;
        this._storage = storage;

        //_agConnService = agConnService;

        var interval = TimeSpan.FromMilliseconds(4000);
        Observable
                .Timer(interval, interval)
                .Subscribe(x =>
                {
                    CurrentLat = service.currentLat;
                    CurentLon = service.curentLon;
                    OneToEight = service.oneToEight;
                    NineToSixteen = service.nineToSixteen;
                    SkipCounter = service.skipCounter;
                });



        var canShow = this.WhenAnyValue(x => x.DialogViewModel).Select(x => x == null);
        Show = ReactiveCommand.Create(ShowImpl, canShow);
        var canActivate = this.WhenAnyValue(x => x.DialogViewModel).Select(x => x != null);
        Activate = ReactiveCommand.Create(ActivateImpl, canActivate);
        Close = ReactiveCommand.Create(CloseImpl, canActivate);
        CommSettings = ReactiveCommand.CreateFromTask(ShowDialogImplAsync);
        //UDPSettings = ReactiveCommand.CreateFromTask(DialogUDPSettingsImplAsync);
        DialogNtrip = ReactiveCommand.CreateFromTask(DialogNtripImplAsync);
        DialogEthernet = ReactiveCommand.CreateFromTask(DialogEthernetImplAsync);
        DialogConfirmClose = ReactiveCommand.CreateFromTask(DialogConfirmCloseImplAsync);
        /********
        OpenFile = ReactiveCommand.CreateFromTask(OpenFileImplAsync);
        OpenFiles = ReactiveCommand.CreateFromTask(OpenFilesImplAsync);
        OpenFolder = ReactiveCommand.CreateFromTask(OpenFolderImplAsync);
        OpenFolders = ReactiveCommand.CreateFromTask(OpenFoldersImplAsync);
        SaveFile = ReactiveCommand.CreateFromTask(SaveFileImplAsync);
        ***/
        MessageBox = ReactiveCommand.CreateFromTask(MessageBoxImplAsync);
        MessageBoxMultiple = ReactiveCommand.CreateFromTask(MessageBoxMultipleImplAsync);
        //
        UDPCommand = ReactiveCommand.Create(() =>
        {
            //  SettingsUDP();
        });
        Quit = ReactiveCommand.Create(QuitMain);
        /*********
        oneSecondLoopTimer.Interval = TimeSpan.FromMilliseconds(4000);
        oneSecondLoopTimer.IsEnabled = true;
        //oneSecondLoopTimer.Tick += oneSecondLoopTimer_Tick;
        oneSecondLoopTimer.Start();

        ntripMeterTimer.Interval = TimeSpan.FromMilliseconds(50);
        ntripMeterTimer.IsEnabled = true;
        //ntripMeterTimer.Tick +=  ntripMeterTimer_Tick;
        ntripMeterTimer.Start();
         *********/
    }

    private void QuitMain()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
            service.Close();
        }
    }


    [Reactive]
    public string? Output { get; set; } //not in use

    [Reactive]
    public string CurrentLat { get; set; } = "-53.1234567";

    [Reactive]
    public string CurentLon { get; set; } = "-888.8888888";

    [Reactive]
    public string OneToEight { get; set; }

    [Reactive]
    public string NineToSixteen { get; set; }

    [Reactive]
    public string SkipCounter { get; set; }
    
   [Reactive]
    public string SerialPorts { get; set; }  = "/dev/tty0";



    private CommSettingsViewModel? _dialogViewModel;
    protected CommSettingsViewModel? DialogViewModel
    {
        get => _dialogViewModel;
        set
        {
            if (DialogViewModel != null)
            {
                DialogViewModel.Closed -= Dialog_ViewClosed;
            }
            this.RaiseAndSetIfChanged(ref _dialogViewModel, value);
            if (DialogViewModel != null)
            {
                DialogViewModel.Closed += Dialog_ViewClosed;
            }
        }
    }


    private void Dialog_ViewClosed(object? sender, EventArgs e) => DialogViewModel = null;

    public RxCommandUnit Show { get; }
    public RxCommandUnit CommSettings { get; }
    public RxCommandUnit Close { get; }
    public RxCommandUnit Activate { get; }
    public RxCommandUnit DialogConfirmClose { get; } //UDPView
    public RxCommandUnit DialogNtrip { get; }
    public RxCommandUnit DialogEthernet { get; }
    //public RxCommandUnit OpenFile { get; }
    //public RxCommandUnit OpenFiles { get; }
    //public RxCommandUnit OpenFolder { get; }
    //public RxCommandUnit OpenFolders { get; }
    //public RxCommandUnit SaveFile { get; }
    public RxCommandUnit MessageBox { get; }
    public RxCommandUnit MessageBoxMultiple { get; }
    public RxCommandUnit Quit { get; }


    private void ShowImpl()
    {
        DialogViewModel = _dialogService.CreateViewModel<CommSettingsViewModel>();
        _dialogService.Show(this, DialogViewModel);
    }

    private void ActivateImpl() => _dialogService.Activate(DialogViewModel!);

    private void CloseImpl()
    {
        _dialogService.Close(DialogViewModel!);
        DialogViewModel = null;
    }

    private async Task ShowDialogImplAsync()
    {
        var vm = _dialogService.CreateViewModel<CommSettingsViewModel>();
        await _dialogService.ShowDialogAsync(this, vm);
    }

    private async Task DialogConfirmCloseImplAsync()//actually UDPView
    {
        if (!Settings.Default.setUDP_isOn)
        {   //SettingsEthernet();
            var vm = _dialogService.CreateViewModel<EthernetViewModel>();
            await _dialogService.ShowDialogAsync(this, vm);
        }
        else
        {
            //SettingsUDP()
            var vm = _dialogService.CreateViewModel<ConfirmCloseViewModel>();
            await _dialogService.ShowDialogAsync(this, vm);
        }
    }

    private async Task DialogNtripImplAsync()
    {
        var vm = _dialogService.CreateViewModel<NtripViewModel>();
        await _dialogService.ShowDialogAsync(this, vm);
    }

    private async Task DialogEthernetImplAsync()
    {
        var vm = _dialogService.CreateViewModel<EthernetViewModel>();
        await _dialogService.ShowDialogAsync(this, vm);
    }
    /**********
        private async Task OpenFileImplAsync()
        {
            var settings = new OpenFileDialogSettings
            {
                SuggestedStartLocation = await _storage.GetDownloadsFolderAsync()
            };
            var file = await _dialogService.ShowOpenFileDialogAsync(this, settings);
            Output = file?.Path + Environment.NewLine;
            if (file?.Path != null)
            {
                await ScanFileProgressAsync(file);
            }
        }

        private async Task ScanFileProgressAsync(IDialogStorageFile file)
        {
            using var stream = file.OpenReadAsync();
            Output += "File opened." + Environment.NewLine;
            var outputHeader = string.Empty;
            long length = 0;

            IProgress<long> progress = new SynchronousProgress<long>(value =>
            {
                Output = outputHeader + ((float)value / length).ToString("P1") + Environment.NewLine;
            });

            await stream.ContinueWith(
                async t =>
                {
                    using var ms = new MemoryStream();
                    var streamResult = stream.Result;
                    length = streamResult.Length;
                    Output += "Result size: " + streamResult.Length + " starting copy to memory" + Environment.NewLine;
                    outputHeader = Output;
                    await streamResult.CopyToAsync(ms, progress, default, 1024);
                    Output += "Done" + Environment.NewLine;
                    ms.Position = 0;
                });
        }

        private async Task OpenFilesImplAsync()
        {
            var settings = new OpenFileDialogSettings
            {
                SuggestedStartLocation = await _storage.GetDownloadsFolderAsync()
            };
            var files = await _dialogService.ShowOpenFilesDialogAsync(this, settings);
            Output = string.Join(Environment.NewLine, files.Select(x => x?.Path?.ToString() ?? ""));
        }

        private async Task OpenFolderImplAsync()
        {
            var settings = new OpenFolderDialogSettings
            {
                SuggestedStartLocation = await _storage.GetDownloadsFolderAsync()
            };
            var folder = await _dialogService.ShowOpenFolderDialogAsync(this, settings);
            Output = folder?.Path?.ToString();
        }

        private async Task OpenFoldersImplAsync()
        {
            var settings = new OpenFolderDialogSettings
            {
                SuggestedStartLocation = await _storage.GetDownloadsFolderAsync()
            };
            var folders = await _dialogService.ShowOpenFoldersDialogAsync(this, settings);
            Output = string.Join(Environment.NewLine, folders.Select(x => x?.Path?.ToString() ?? ""));
        }

        private async Task SaveFileImplAsync()
        {
            var settings = new SaveFileDialogSettings
            {
                SuggestedStartLocation = await _storage.GetDownloadsFolderAsync()
            };
            var file = await _dialogService.ShowSaveFileDialogAsync(this, settings);
            Output = file?.Path?.ToString();
        }
        *****/

    private async Task MessageBoxImplAsync()
    {
        var result = await _dialogService.ShowMessageBoxAsync(this, "Do you want it?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
        Output = result.ToString();
    }

    private async Task MessageBoxMultipleImplAsync()
    {
        var t1 = _dialogService.ShowMessageBoxAsync(this, "First message box", "Go", MessageBoxButton.YesNo, MessageBoxImage.Exclamation).ConfigureAwait(false);
        await Task.Delay(1000).ConfigureAwait(false);
        var t2 = _dialogService.ShowMessageBoxAsync(this, "Second message box", "Again", MessageBoxButton.YesNo, MessageBoxImage.Exclamation).ConfigureAwait(false);
        await Task.Delay(1000).ConfigureAwait(false);
        var t3 = _dialogService.ShowMessageBoxAsync(this, "Third message box", "Once More!", MessageBoxButton.YesNo, MessageBoxImage.Exclamation).ConfigureAwait(false);
        var r1 = await t1;
        var r2 = await t2;
        var r3 = await t3;
        Output = r1 + Environment.NewLine + r2 + Environment.NewLine + r3;
    }
}
