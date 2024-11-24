using System.ComponentModel;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Avalonia.Threading;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using static AgConn.CrossPlatform.RegisteredServices;

namespace AgConn.CrossPlatform.ViewModels;

public class ConfirmCloseViewModel : ViewModelBase, IModalDialogViewModel, IViewClosing, IViewLoaded, ICloseable
{
    private readonly IDialogService _dialogService;
    public event EventHandler? RequestClose;
    public bool? DialogResult => true;
    //hennie
    AgConnService service = new AgConnService();
    private DispatcherTimer timer1 = new DispatcherTimer();
    //used to send communication check pgn= C8 or 200
    private byte[] sendIPToModules = { 0x80, 0x81, 0x7F, 201, 5, 201, 201, 192, 168, 5, 0x47 };

    private byte[] ipCurrent = { 192, 168, 5 };
    private byte[] ipNew = { 192, 168, 5 };

    public ConfirmCloseViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        Close = ReactiveCommand.Create(CloseImpl);
        SendSubnet = ReactiveCommand.Create(SendSubnetImpl);
        SerialMonitor = ReactiveCommand.CreateFromTask(SerialMonitorImplAsync);
        /***
        if (OperatingSystem.IsAndroid()) { 
           Hostname = "pipo"; //NetService.getHostName();
            Console.WriteLine("HOSTNAME" + Hostname);
        }
        else {
           Hostname = Dns.GetHostName(); //unavailable on the current platform
          } 
          ****/
        timer1.Interval = TimeSpan.FromMilliseconds(500);
        timer1.IsEnabled = true;
        timer1.Tick += timer1_Tick;
        timer1.Start();
    }

    [Reactive]
    public string Text { get; set; } = string.Empty;

    [Reactive]
    public string FirstIP { get; set; } = "192";
    [Reactive]
    public string SecndIP { get; set; } = "168";
    [Reactive]
    public string ThirdIP { get; set; } = "5";

    [Reactive]
    public string MachineIP { get; set; } = "..";
    [Reactive]
    public string GPSIP { get; set; } = "..";
    [Reactive]
    public string IMU_IP { get; set; } = "...";
    [Reactive]
    public string SteerIP { get; set; } = "..";

    [Reactive]
    public string NewSubnet { get; set; } = "192 . 168 . 123";
    [Reactive]
    public string SubTimer { get; set; } = "-";
    [Reactive]
    public string Nets { get; set; } = "";
    [Reactive]
    public string Hostname { get; set; } = "Hostname";
    [Reactive]
    public string NetworkHelp { get; set; } = "";

    [Reactive]
    public bool Up { get; set; }
    [Reactive]
    public bool AllowSpin { get; set; }
    [Reactive]
    public bool AutoSet { get; set; }


    public RxCommandUnit Close { get; }
    public RxCommandUnit SerialMonitor { get; }
    public RxCommandUnit SendSubnet { get; }

    public void OnLoaded()
    {
        //Text = "This dialog requires close confirmation.";
        if (OperatingSystem.IsLinux())
        {
            ScanNetwork();
        }
        if (OperatingSystem.IsAndroid())
        {
            Hostname = NetService.getHostName();
            foreach (IPAddress ipa in NetService.GetAllLocalValidIp4Addresses())
            {
                //Nets = string.Join( ",", NetService.GetAllLocalValidIp4Addresses());
                Nets += ipa + System.Environment.NewLine;
            }
        }
    }
    
    private async Task SerialMonitorImplAsync()
    {
       // var vm = _dialogService.CreateViewModel<SerialMonitorViewModel>();
       // await _dialogService.ShowDialogAsync(this, vm);
    }

    public void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
    }

    private void CloseImpl()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public async Task OnClosingAsync(CancelEventArgs e)
    {
        var result = await _dialogService.ShowMessageBoxAsync(this, "Do you want to close it?", "Confirmation", MessageBoxButton.YesNo);
        e.Cancel = result == false;
    }

    private int tickCounter = 0;

    private void timer1_Tick(object sender, EventArgs e)
    {
        if (!service.scanReply.isNewData)
        {
            service.ipAutoSet[0] = 99;
            service.ipAutoSet[1] = 99;
            service.ipAutoSet[2] = 99;
            AutoSet = false; 
            Console.WriteLine("Autoset disabled");
        }
        else
        {
            AutoSet = true; 
            Console.WriteLine("Autoset enabled");
        }

        if (service.scanReply.isNewSteer)
        {
            SteerIP = service.scanReply.steerIP;
            service.scanReply.isNewSteer = false;
            NewSubnet = service.scanReply.subnetStr;
        }

        if (service.scanReply.isNewMachine)
        {
            MachineIP = service.scanReply.machineIP;
            service.scanReply.isNewMachine = false;
            NewSubnet = service.scanReply.subnetStr;
        }

        if (service.scanReply.isNewIMU)
        {
            IMU_IP = service.scanReply.IMU_IP;
            service.scanReply.isNewIMU = false;
            NewSubnet = service.scanReply.subnetStr;
        }

        if (service.scanReply.isNewGPS)
        {
            GPSIP = service.scanReply.GPS_IP;
            service.scanReply.isNewGPS = false;
            NewSubnet = service.scanReply.subnetStr;
        }

        if (tickCounter == 4)
        {
            /****TODO?
                if (service.btnSteer.BackColor == Color.LimeGreen) lblBtnSteer.BackColor = Color.LimeGreen;
                else lblBtnSteer.BackColor = Color.Red;

                if (service.btnMachine.BackColor == Color.LimeGreen) lblBtnMachine.BackColor = Color.LimeGreen;
                else lblBtnMachine.BackColor = Color.Red;

                if (service.btnGPS.BackColor == Color.LimeGreen) lblBtnGPS.BackColor = Color.LimeGreen;
                else lblBtnGPS.BackColor = Color.Red;

                if (service.btnIMU.BackColor == Color.LimeGreen) lblBtnIMU.BackColor = Color.LimeGreen;
                else lblBtnIMU.BackColor = Color.Red;
                **********/
        }

        if (tickCounter > 5)
        {
            ScanNetwork();
            tickCounter = 0;
            SubTimer = "Scanning";
        }
        else
        {
            SubTimer = "-";
        }
        tickCounter++;
    }

    private void ScanNetwork()
    {
        Nets = "";
        //  Hostname = NetService.getHostName();

        SteerIP = MachineIP = GPSIP = IMU_IP = NewSubnet = "";
        service.scanReply.isNewData = false;

        bool isSubnetMatchCard = false;

        byte[] scanModules = { 0x80, 0x81, 0x7F, 202, 3, 202, 202, 5, 0x47 };

        //Send out 255x4 to each installed network interface
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.Supports(NetworkInterfaceComponent.IPv4))
            {
                foreach (var info in nic.GetIPProperties().UnicastAddresses)
                {
                    // Only InterNetwork and not loopback which have a subnetmask
                    if (info.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(info.Address))
                    {
                        Socket scanSocket;
                        try
                        {
                            //create list of interface properties
                            if ((Up == true && nic.OperationalStatus == OperationalStatus.Up) || Up == false)
                            {
                               IPInterfaceStatistics properties = null;
                                if (!OperatingSystem.IsAndroid())
                                {
                                    properties = nic.GetIPStatistics();
                                }
                                Nets +=
                                        info.Address + "  - " + nic.OperationalStatus + System.Environment.NewLine;

                                Nets += nic.Name.ToString() + Environment.NewLine;
                                //unavailable on platform
                                if (!OperatingSystem.IsAndroid())
                                {
                                    Nets += "Sent: " + (/**properties.NonUnicastPacketsSent 
                                       + **/properties.UnicastPacketsSent).ToString()
                                       + "   Recd: " + (properties.NonUnicastPacketsReceived
                                        + properties.UnicastPacketsReceived).ToString() + System.Environment.NewLine + System.Environment.NewLine;
                                }
                            }

                            if (nic.OperationalStatus == OperationalStatus.Up
                                && info.IPv4Mask != null)
                            {
                                byte[] data = info.Address.GetAddressBytes();
                                if (data[0] == ipCurrent[0] && data[1] == ipCurrent[1] && data[2] == ipCurrent[2])
                                {
                                    isSubnetMatchCard = true;
                                }

                                //send scan reply out each network interface
                                scanSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                                scanSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                                scanSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                                scanSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, true);

                                try
                                {
                                    scanSocket.Bind(new IPEndPoint(info.Address, 9999));
                                    scanSocket.SendTo(scanModules, 0, scanModules.Length, SocketFlags.None, service.epModuleSet);
                                }
                                catch (Exception ex)
                                {
                                    Console.Write("Bind Error = ");
                                    Console.WriteLine(ex.ToString());
                                }

                                scanSocket.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Write("nic Loop = ");
                            Console.WriteLine(ex.ToString());
                            /****
                            System.PlatformNotSupportedException: The information requested is unavailable on the current platform.
                            at System.Net.NetworkInformation.AndroidNetworkInterface.GetIPStatistics()
                            at AgConn.CrossPlatform.ViewModels.ConfirmCloseViewModel.ScanNetwork()   **********/

                        }
                    }
                }
            }
        }

        if (isSubnetMatchCard)
        {
            // lblNetworkHelp.BackColor = System.Drawing.Color.LightGreen; TODO
            // lblNoAdapter.Visible = false; TODO
        }
        else
        {
            //lblNetworkHelp.BackColor = System.Drawing.Color.Salmon; TODO
            // lblNoAdapter.Visible = true; TODO
        }
    }

    private void SendSubnetImpl()
    {
        {
            sendIPToModules[7] = ipNew[0];
            sendIPToModules[8] = ipNew[1];
            sendIPToModules[9] = ipNew[2];

            //loop thru all interfaces
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.Supports(NetworkInterfaceComponent.IPv4) && nic.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var info in nic.GetIPProperties().UnicastAddresses)
                    {
                        // Only InterNetwork and not loopback which have a subnetmask
                        if (info.Address.AddressFamily == AddressFamily.InterNetwork &&
                            !IPAddress.IsLoopback(info.Address) &&
                            info.IPv4Mask != null)
                        {
                            Socket scanSocket;
                            try
                            {
                                if (nic.OperationalStatus == OperationalStatus.Up
                                    && info.IPv4Mask != null)
                                {
                                    scanSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                                    scanSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                                    scanSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                                    scanSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, true);

                                    try
                                    {
                                        scanSocket.Bind(new IPEndPoint(info.Address, 9999));
                                        scanSocket.SendTo(sendIPToModules, 0, sendIPToModules.Length, SocketFlags.None, service.epModuleSet);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.Write("Bind Error = ");
                                        Console.WriteLine(ex.ToString());
                                    }

                                    scanSocket.Dispose();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.Write("nic Loop = ");
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                }
            }

            Properties.Settings.Default.etIP_SubnetOne = ipCurrent[0] = ipNew[0];
            Properties.Settings.Default.etIP_SubnetTwo = ipCurrent[1] = ipNew[1];
            Properties.Settings.Default.etIP_SubnetThree = ipCurrent[2] = ipNew[2];

            Properties.Settings.Default.Save();

            service.epModule = new IPEndPoint(IPAddress.Parse(
                Properties.Settings.Default.etIP_SubnetOne.ToString() + "." +
                Properties.Settings.Default.etIP_SubnetTwo.ToString() + "." +
                Properties.Settings.Default.etIP_SubnetThree.ToString() + ".255"), 8888);

            NetworkHelp =
                Properties.Settings.Default.etIP_SubnetOne.ToString() + " . " +
                Properties.Settings.Default.etIP_SubnetTwo.ToString() + " . " +
                Properties.Settings.Default.etIP_SubnetThree.ToString();
        }

        //  pboxSendSteer.Visible = false; TODO
        // btnSerialCancel.Image = Properties.Resources.back_button; TODO
    }
}
