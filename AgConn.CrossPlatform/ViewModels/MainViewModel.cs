using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.IO.Ports;
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
using AgConn.CrossPlatform.Models;

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;


namespace AgConn.CrossPlatform.ViewModels;

public class MainViewModel : ViewModelBase
{

    private readonly IDialogService _dialogService;
    private readonly IStorageService _storage;
    private IAgConnService _agConnService;
      private IUDPService _UDPService;
      
      public string log;

    // private DispatcherTimer oneSecondLoopTimer = new DispatcherTimer();
    // private DispatcherTimer ntripMeterTimer = new DispatcherTimer();
  //  AgConnService service = new AgConnService();

    ///
    public ICommand UDPCommand { get; }

    public MainViewModel(IDialogService dialogService, IStorageService storage, IAgConnService agConnService, IUDPService UDPService)
    {
        this._dialogService = dialogService;
        this._storage = storage;
        GPSData gpsData = new();

        _agConnService = agConnService;
         _UDPService = UDPService;
         
        //  _UDPService.Server("0.0.0.0", 9999);
         //  _UDPService.Server("127.0.0.1", 17777);
           
        var interval = TimeSpan.FromMilliseconds(4000);
        Observable
                .Timer(interval, interval)
                .Subscribe(x =>
                {
                    CurrentLat = _agConnService.currentLat;
                    CurentLon = _agConnService.curentLon;
                   OneToEight = _agConnService.oneToEight;
                 NineToSixteen = _agConnService.nineToSixteen;
                  //  SkipCounter = service.skipCounter;
                  // _UDPService.Client("127.0.0.1", 17777);
                  // log = _UDPService.Send(new byte[] { 0x80, 0x81, 0x7F, 200, 3, 56, 0, 0, 0x47 });
                 //  CurentLon = log;
                   
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
        DialogUDP = ReactiveCommand.CreateFromTask(DialogUDPImplAsync);
      // UDPMonitor = ReactiveCommand.CreateFromTask(UDPMonitorImplAsync);
         UDPMonitor = ReactiveCommand.CreateFromTask(MessageBoxImplAsync);
       
        //MenuItem4 = ReactiveCommand.CreateFromTask(DialogGPSDataImplAsync);
            MenuItem4 = ReactiveCommand.CreateFromTask(GPSDataImplAsync);
         EthernetSetup = ReactiveCommand.CreateFromTask(EthernetSetupImplAsync);
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
        RelayTest = ReactiveCommand.Create(RelayTestImpl);
      
        _agConnService.Init();
       
                       
                                //  if (Settings.Default.setUDP_isOn)
        //    {
         //       //Console.WriteLine("Settings.Default.setUDP_isOn");
          //      _agConnService.LoadUDPNetwork();
          //  }
         
           
    }

    private void QuitMain()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
            _agConnService.Close();
        }
    }
    
     private void RelayTestImpl()
     {
         _agConnService.RelayTest();
      }    
      
     private async Task UDPMonitorImplAsync()
    {
       var vm = _dialogService.CreateViewModel<UDPMonitorViewModel>();
       await _dialogService.ShowDialogAsync(this, vm);
    }
    
      private async Task EthernetSetupImplAsync()
    {
       var vm = _dialogService.CreateViewModel<EthernetViewModel>();
       await _dialogService.ShowDialogAsync(this, vm);
    }
    
     private async Task LoadUDPNetworkAsync() {
          if (Settings.Default.setUDP_isOn)
            {
                //Console.WriteLine("Settings.Default.setUDP_isOn");
               await _agConnService.LoadUDPNetwork();
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
    public string[] SerialPorts { get; set; }  = {"/dev/tty0"};



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
    public RxCommandUnit DialogUDP{ get; } 
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
      public RxCommandUnit RelayTest { get; }
      
       public RxCommandUnit UDPMonitor { get; }
         public RxCommandUnit MenuItem4 { get; }
         public RxCommandUnit EthernetSetup { get; }



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

    private async Task DialogUDPImplAsync()
    {
        if (!Settings.Default.setUDP_isOn)
        {   //SettingsEthernet();
            var vm = _dialogService.CreateViewModel<EthernetViewModel>();
            await _dialogService.ShowDialogAsync(this, vm);
        }
        else
        {
            //SettingsUDP()
            var vm = _dialogService.CreateViewModel<UDPViewModel>();
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
    
     private async Task DialogGPSDataImplAsync()
    {
        var vm = _dialogService.CreateViewModel<GPSDataViewModel>();
        await _dialogService.ShowDialogAsync(this, vm);
    }
    
     private async Task MessageBoxImplAsync()
    {
        string log = "";
        log += _agConnService.logUDPSentence.ToString();
        var result = await _dialogService.ShowMessageBoxAsync(this, log, "Time  IP Address:Port  PGN", MessageBoxButton.Ok);
        Output = result.ToString();
    }
    
       private async Task GPSDataImplAsync()
    {
        string log = "";
        log += "Latitude: " + _agConnService.latitd.ToString("N7") + Environment.NewLine;
		log += "Longitude: " +  _agConnService.longtd.ToString("N7") + Environment.NewLine;
        log += "Quality: " + _agConnService.FixQuality + Environment.NewLine;
		log += "# Sats: " + _agConnService.satellitesData.ToString() + Environment.NewLine;
	    log += "HDOP: " +  _agConnService.hdopData.ToString() + Environment.NewLine;
		log += "Speed: " + _agConnService.speedData.ToString("N1") + Environment.NewLine;
        log += "Roll: " + _agConnService.rollData.ToString("N2") + Environment.NewLine;
				
        log += "Age: " + _agConnService.ageData.ToString("N1") + Environment.NewLine;
        log += "VTG: " + _agConnService.headingTrueData.ToString("N2") + Environment.NewLine;
				//    DualHeading = _agConnService.headingTrueDualData.ToString("N2"); VTG?
        log += "Altitude: " + _agConnService.altitudeData.ToString("N1") + Environment.NewLine;
        log += "IMURoll: " +  _agConnService.imuRollData.ToString() + Environment.NewLine;
	    log += "IMUPitch: " + _agConnService.imuPitchData.ToString() + Environment.NewLine;
	    log += "IMUYawRate: " + _agConnService.imuYawRateData.ToString() + Environment.NewLine;
	    log += "IMUHeading: " + _agConnService.imuHeadingData.ToString() + Environment.NewLine;
	    
	    log += "VTG: " + _agConnService.vtgSentence + Environment.NewLine;
		log += "GGA: " + _agConnService.ggaSentence + Environment.NewLine;
		log += "PAOGI: " + _agConnService.paogiSentence + Environment.NewLine;
		log += "AVR: " + _agConnService.avrSentence + Environment.NewLine;
		log += "HDT: " + _agConnService.hdtSentence + Environment.NewLine;
		log += "HPD: " + _agConnService.hpdSentence + Environment.NewLine;
		log += "PANDA: " + _agConnService.pandaSentence + Environment.NewLine;
		log += "KSXT: " + _agConnService.ksxtSentence + Environment.NewLine;
						   
        var result = await _dialogService.ShowMessageBoxAsync(this, log, "GPS Data", MessageBoxButton.Ok);
        Output = result.ToString();
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

   // private async Task MessageBoxImplAsync()
   // {
  //      var result = await _dialogService.ShowMessageBoxAsync(this, "Do you want it?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
   //     Output = result.ToString();
  //  }

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
