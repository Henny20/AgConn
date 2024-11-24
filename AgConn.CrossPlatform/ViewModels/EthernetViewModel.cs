using System.ComponentModel;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using AgConn.CrossPlatform.Properties;

namespace AgConn.CrossPlatform.ViewModels;

public class EthernetViewModel : ViewModelBase, IModalDialogViewModel, ICloseable, IViewClosed
{
    public EthernetViewModel()
    {
        SerialCancel = ReactiveCommand.Create(CloseImpl);
    }
    
    [Reactive]
    public int? FourthIP { get; set; } = 255;
    [Reactive]
    public int? ThirdIP { get; set; } = 255;
    [Reactive]
    public int? SecndIP { get; set; } = 255;
    [Reactive]
    public int? FirstIP { get; set; } = 127;
    [Reactive]
    public bool IsSendNMEAToUDP { get; set; } = false;
    [Reactive]
    public bool IsUDPOn { get; set; } = false;
    

    public bool? DialogResult { get; } = true;
    public event EventHandler? RequestClose;
    public event EventHandler? Closed;

    public RxCommandUnit SerialCancel { get; }
    
    public void OnLoaded()
    {
        IsUDPOn = Properties.Settings.Default.setUDP_isOn;
        IsSendNMEAToUDP = Properties.Settings.Default.setUDP_isSendNMEAToUDP;
        
        FirstIP = Properties.Settings.Default.eth_loopOne;
        SecndIP = Properties.Settings.Default.eth_loopTwo;
        ThirdIP = Properties.Settings.Default.eth_loopThree;
        FourthIP = Properties.Settings.Default.eth_loopFour;
    }   

    private void CloseImpl()
    {
        Properties.Settings.Default.eth_loopOne = (byte)FirstIP;
        Properties.Settings.Default.eth_loopTwo = (byte)SecndIP;
        Properties.Settings.Default.eth_loopThree = (byte)ThirdIP;
        Properties.Settings.Default.eth_loopFour = (byte)FourthIP;

        Properties.Settings.Default.setUDP_isOn = IsUDPOn;
        Properties.Settings.Default.setUDP_isSendNMEAToUDP = IsSendNMEAToUDP;

        Properties.Settings.Default.Save();
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public void OnClosed() => Closed?.Invoke(this, EventArgs.Empty);
}
