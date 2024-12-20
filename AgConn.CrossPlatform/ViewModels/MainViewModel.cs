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
      
     public string log;

    public ICommand UDPCommand { get; }

    public MainViewModel(IDialogService dialogService, IStorageService storage, IAgConnService agConnService)
    {
        this._dialogService = dialogService;
        this._storage = storage;

        _agConnService = agConnService;
               
        var interval = TimeSpan.FromMilliseconds(4000);
        Observable
                .Timer(interval, interval)
                .Subscribe(x =>
                {
                    CurrentLat = _agConnService.currentLat;
                    CurentLon = _agConnService.curentLon;
                   OneToEight = _agConnService.oneToEight;
                 NineToSixteen = _agConnService.nineToSixteen;
                                    
                });


        Close = ReactiveCommand.Create(CloseImpl);
        CommSettings = ReactiveCommand.CreateFromTask(ShowDialogImplAsync);
      
        DialogNtrip = ReactiveCommand.CreateFromTask(DialogNtripImplAsync);
        DialogEthernet = ReactiveCommand.CreateFromTask(DialogEthernetImplAsync);
        DialogUDP = ReactiveCommand.CreateFromTask(DialogUDPImplAsync);
      // UDPMonitor = ReactiveCommand.CreateFromTask(UDPMonitorImplAsync);
         UDPMonitor = ReactiveCommand.CreateFromTask(MessageBoxImplAsync);
       
        //MenuItem4 = ReactiveCommand.CreateFromTask(DialogGPSDataImplAsync);
         MenuItem4 = ReactiveCommand.CreateFromTask(GPSDataImplAsync);
         EthernetSetup = ReactiveCommand.CreateFromTask(EthernetSetupImplAsync);
      
        MessageBox = ReactiveCommand.CreateFromTask(MessageBoxImplAsync);
        //example
        UDPCommand = ReactiveCommand.Create(() =>
        {
            //  SettingsUDP();
        });
	//
        Quit = ReactiveCommand.Create(QuitMain);
        RelayTest = ReactiveCommand.Create(RelayTestImpl);
      
        _agConnService.Init();
             
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
                await _agConnService.LoadUDPNetwork();
            }
    }
 
    [Reactive]
    public string? Output { get; set; } 

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



    public RxCommandUnit Show { get; }
    public RxCommandUnit CommSettings { get; }
    public RxCommandUnit Close { get; }
    public RxCommandUnit Activate { get; }
    public RxCommandUnit DialogUDP{ get; } 
    public RxCommandUnit DialogNtrip { get; }
    public RxCommandUnit DialogEthernet { get; }
  
    public RxCommandUnit MessageBox { get; }
    public RxCommandUnit MessageBoxMultiple { get; }
    public RxCommandUnit Quit { get; }
      public RxCommandUnit RelayTest { get; }
      
       public RxCommandUnit UDPMonitor { get; }
         public RxCommandUnit MenuItem4 { get; }
         public RxCommandUnit EthernetSetup { get; }



 
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
				//    DualHeading = _agConnService.headingTrueDualData.ToString("N2"); 
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
    
}
