using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using AgConn.CrossPlatform.Properties;

using static AgConn.CrossPlatform.RegisteredServices;

namespace AgConn.CrossPlatform.ViewModels;

public class NtripViewModel : ViewModelBase, IModalDialogViewModel, ICloseable, IViewLoaded, IViewClosed
{
    private ObservableCollection<string> _items;

    [Reactive]
    public ObservableCollection<string> Items { get; set; }
    // {
    //   get => _items; 
    //   set => this.RaiseAndSetIfChanged(ref _items, value); 
    //}

    AgConnService service = new AgConnService();

    public NtripViewModel()
    {
        Items = new ObservableCollection<string>();

        Close = ReactiveCommand.Create(CloseImpl);
        GetSourceTable = ReactiveCommand.Create(GetSourceTableImpl);
        GetIP = ReactiveCommand.Create(GetIPImpl);
        PassPassword = ReactiveCommand.Create(PassPasswordImpl);
        PassUsername = ReactiveCommand.Create(PassUsernameImpl);
    }

    [Reactive]
    public string Username { get; set; } = string.Empty;
    [Reactive]
    public string UserPassword { get; set; } = string.Empty;
    [Reactive]
    public string Mount { get; set; } = string.Empty;
    [Reactive]
    public string HostName { get; set; } = string.Empty;
    [Reactive]
    public decimal Latitude { get; set; } = 0.0m;
    [Reactive]
    public decimal Longitude { get; set; } //= 0.0;
    [Reactive]
    public string CurrentLat { get; set; } = string.Empty;
    [Reactive]
    public string CurrentLon { get; set; } = string.Empty;
    [Reactive]
    public string CasterIP { get; set; } = string.Empty;
    [Reactive]
    public string EnterURL { get; set; } = string.Empty;

    [Reactive]
    public int GGAInterval { get; set; } = 0;
    [Reactive]
    public int CasterPort { get; set; } = 2101;
    [Reactive]
    public int SendToUDPPort { get; set; } = 0;

    [Reactive]
    public bool IsNTRIPOn { get; set; }
    [Reactive]
    public bool ToSerial { get; set; }
    [Reactive]
    public bool ToUDP { get; set; }
    [Reactive]
    public bool Usetcp { get; set; }
    [Reactive]
    public bool Control1 { get; set; }



    public bool? DialogResult { get; } = true;
    public event EventHandler? RequestClose;
    public event EventHandler? Closed;

    public RxCommandUnit Close { get; }
    public RxCommandUnit GetSourceTable { get; }
    public RxCommandUnit GetIP { get; }
    public RxCommandUnit PassPassword { get; }
    public RxCommandUnit PassUsername { get; }

    public void OnLoaded()
    {
        IsNTRIPOn = Properties.Settings.Default.setNTRIP_isOn;

        //if (!IsNTRIPOn) tabControl1.Enabled = false; TODO
        if (!OperatingSystem.IsAndroid())
        {
            string hostName = Dns.GetHostName(); // Retrieve the Name of HOST
            HostName = hostName;

            IPAddress[] ipaddress = Dns.GetHostAddresses(hostName);
            GetIP4AddressList();
        }
        if (OperatingSystem.IsAndroid())
        {
            HostName = NetService.getHostName();
            GetAllLocalValidIp4Addresses();
        }



        ToSerial = Properties.Settings.Default.setNTRIP_sendToSerial;
        ToUDP = Properties.Settings.Default.setNTRIP_sendToUDP;
        SendToUDPPort = Properties.Settings.Default.setNTRIP_sendToUDPPort;

        EnterURL = Properties.Settings.Default.setNTRIP_casterURL;

        CasterIP = Properties.Settings.Default.setNTRIP_casterIP;
        CasterPort = Properties.Settings.Default.setNTRIP_casterPort;

        Username = Properties.Settings.Default.setNTRIP_userName;
        UserPassword = Properties.Settings.Default.setNTRIP_userPassword;
        Mount = Properties.Settings.Default.setNTRIP_mount;

        GGAInterval = Properties.Settings.Default.setNTRIP_sendGGAInterval;

        Latitude = Convert.ToDecimal(Properties.Settings.Default.setNTRIP_manualLat);
        // Latitude = (decimal)Properties.Settings.Default.setNTRIP_manualLat;
        Longitude = Convert.ToDecimal(Properties.Settings.Default.setNTRIP_manualLon);
        CurrentLat = Properties.Settings.Default.setNTRIP_manualLat.ToString();
        CurrentLon = Properties.Settings.Default.setNTRIP_manualLon.ToString();

        Usetcp = Properties.Settings.Default.setNTRIP_isTCP;
        /****TODO 
        if (Properties.Settings.Default.setNTRIP_isGGAManual) cboxGGAManual.Text = "Use Manual Fix";
        else GGAManual = "Use GPS Fix";

        if (Properties.Settings.Default.setNTRIP_isHTTP10) cboxHTTP.Text = "1.0";
        else HTTP = "1.1";
        *********/
        //PacketSize = service.packetSizeNTRIP.ToString();TODO
    }

    public void GetIP4AddressList()
    {

        Items.Clear();

        foreach (IPAddress IPA in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (IPA.AddressFamily == AddressFamily.InterNetwork)
            {
                Items.Add(IPA.ToString());
            }
        }

    }

    public void GetAllLocalValidIp4Addresses()
    {

        Items.Clear();

        foreach (IPAddress ipa in NetService.GetAllLocalValidIp4Addresses())
        {
            Items.Add(ipa.ToString());

        }

    }

    private void CloseImpl()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void GetSourceTableImpl()
    {
    }

    private void GetIPImpl()
    {

       // if (OperatingSystem.IsAndroid()) { Console.WriteLine("Not implemented yet"); }
      //  else
       // {
            string actualIP = EnterURL.Trim();
            try
            {
                IPAddress[] addresslist = Dns.GetHostAddresses(actualIP);
                if (addresslist != null)
                {
                    CasterIP = "";
                    foreach (var addr in addresslist)
                    {
                        if (addr.AddressFamily == AddressFamily.InterNetwork)
                        {
                            CasterIP = addr.ToString().Trim();
                            service.broadCasterIP = addr.ToString().Trim();
                            Properties.Settings.Default.setNTRIP_casterIP = service.broadCasterIP;
                            Properties.Settings.Default.Save();
                            break;
                        }
                    }
                    // mf.TimedMessageBox(2500, "IP Located", "Verified: " + actualIP); TODO
                    Console.WriteLine("Verified: " + actualIP);
                }
                else
                {
                    //  mf.YesMessageBox("Can't Find: " + actualIP);TODO
                    Console.WriteLine("Can't Find: " + actualIP);
                }
            }
            catch (Exception)
            {
                // mf.YesMessageBox("Can't Find: " + actualIP);
                Console.WriteLine("Can't Find: " + actualIP);
            }
       // }

    }

    private void PassPasswordImpl()
    {
    }

    private void PassUsernameImpl()
    {
    }

    public void OnClosed() => Closed?.Invoke(this, EventArgs.Empty);
}
