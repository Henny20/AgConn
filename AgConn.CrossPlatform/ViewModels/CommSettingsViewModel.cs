using System.Reactive.Linq;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;
using System.IO.Ports;
using static AgConn.CrossPlatform.RegisteredServices;

namespace AgConn.CrossPlatform.ViewModels;

public class CommSettingsViewModel : ViewModelBase,
                                     IModalDialogViewModel,
                                     ICloseable,
                                     IViewClosed
{
    public CommSettingsViewModel()
    {
        // Observable.Timer(TimeSpan.FromSeconds(1),
        // TimeSpan.FromSeconds(1)).Subscribe(
        //   (_) =>
        //   {
        //        this.RaisePropertyChanged(nameof(CommSettings));
        //     });
        Close = ReactiveCommand.Create(CloseImpl);

        BaudRates =
            new string[] { "4800", "9600", "19200", "38400", "57600", "115200" };
        RTCMBaudRates = new string[]{"4800",  "9600",   "19200",  "38400",
                                 "57600", "115200", "128000", "256000"};
        // Ports = new string[]
        //    {"/dev/ttyACM0", "/dev/tty1", "/dev/tnt1"};

        if (OperatingSystem.IsAndroid())
        {
            Ports = UsbService.GetPortNames();
        }
        else
        {
            Ports = SerialPort.GetPortNames();
        }
    }

    public string[] Ports { get; }
    public string[] BaudRates { get; }
    public string[] RTCMBaudRates { get; }
    // public DateTime CommSettings => DateTime.Now;
    public bool _serialState;

    public bool SerialState
    {
        get => _serialState;
        set
        {
            _serialState = value;
            this.RaisePropertyChanged();
        }
    }

    public bool? DialogResult { get; }
    = true;
    public event EventHandler? RequestClose;
    public event EventHandler? Closed;

    public RxCommandUnit Close { get; }

    private void CloseImpl() { RequestClose?.Invoke(this, EventArgs.Empty); }

    public void OnClosed() => Closed?.Invoke(this, EventArgs.Empty);
}
