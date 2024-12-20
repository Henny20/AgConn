using System.Reactive.Linq;
﻿using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using AgConn.CrossPlatform.Properties;

using static AgConn.CrossPlatform.RegisteredServices;

namespace AgConn.CrossPlatform.ViewModels;

public class NtripViewModel : ViewModelBase, IModalDialogViewModel, ICloseable, IViewLoaded, IViewClosed
{
   
    private readonly IDialogService _dialogService;
    private IAgConnService _agConnService;
    
    private bool ntripStatusChanged = false;
    
    [Reactive]
    public ObservableCollection<string> Items { get; set; }
    
    [Reactive]
    public ObservableCollection<string> PacketSizes { get; set; }
    // {
    //   get => _items; 
    //   set => this.RaiseAndSetIfChanged(ref _items, value); 
    //}

   // AgConnService service = new AgConnService();

    public NtripViewModel(IDialogService dialogService, IAgConnService agConnService)
    {
        _dialogService = dialogService; 
        _agConnService = agConnService;
        
        Items = new ObservableCollection<string>();
        PacketSizes = new ObservableCollection<string> { "64", "128", "256" };;
        
        var interval = TimeSpan.FromMilliseconds(500);
        Observable
                .Timer(interval, interval)
                .Subscribe(x =>
                {
                    CurrentLat = _agConnService.currentLat;
                    CurrentLon = _agConnService.curentLon;
                });

        Close = ReactiveCommand.Create(CloseImpl);
        GetSourceTable = ReactiveCommand.Create(GetSourceTableImpl);
        GetIP = ReactiveCommand.Create(GetIPImpl);
        PassPassword = ReactiveCommand.Create(PassPasswordImpl);
        PassUsername = ReactiveCommand.Create(PassUsernameImpl);
        SetManualPosition = ReactiveCommand.Create(SetManualPositionImpl);
        
         MessageBox = ReactiveCommand.CreateFromTask(MessageBoxImplAsync);
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
    public string HTTP { get; set; } = string.Empty;
    [Reactive]
    public string GGAManual { get; set; } = string.Empty;
    [Reactive]
    public string PacketSize { get; set; } 

    [Reactive]
    public string? Output { get; set; } 

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
    public RxCommandUnit SetManualPosition { get; }
    
     public RxCommandUnit MessageBox { get; }

    public void OnLoaded()
    {
        IsNTRIPOn = Properties.Settings.Default.setNTRIP_isOn;

        ///if (!IsNTRIPOn) Control1 = false; 
   
         string hostName = Dns.GetHostName(); // Retrieve the Name of HOST
         HostName = hostName;

         IPAddress[] ipaddress = Dns.GetHostAddresses(hostName);
         GetIP4AddressList();
    

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
        
        if (Properties.Settings.Default.setNTRIP_isGGAManual) GGAManual = "Use Manual Fix";
        else GGAManual = "Use GPS Fix";

        if (Properties.Settings.Default.setNTRIP_isHTTP10) HTTP = "1.0";
        else HTTP = "1.1";
        
        PacketSize = _agConnService.packetSizeNTRIP.ToString();
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

    private void CloseImpl()
    {
         Properties.Settings.Default.setNTRIP_casterIP = CasterIP;
            Properties.Settings.Default.setNTRIP_casterPort = (int)CasterPort;
            Properties.Settings.Default.setNTRIP_sendToUDPPort = (int)SendToUDPPort;

            Properties.Settings.Default.setNTRIP_isOn = IsNTRIPOn;

            if (IsNTRIPOn)
            {
                Properties.Settings.Default.setRadio_isOn = _agConnService.isRadio_RequiredOn = false;
                Properties.Settings.Default.setPass_isOn = _agConnService.isSerialPass_RequiredOn = false;
            }

            Properties.Settings.Default.setNTRIP_userName = Username;
            Properties.Settings.Default.setNTRIP_userPassword = UserPassword;
            Properties.Settings.Default.setNTRIP_mount = Mount;

            Properties.Settings.Default.setNTRIP_sendGGAInterval = (int)GGAInterval;
            Properties.Settings.Default.setNTRIP_manualLat = (double)Latitude;
            Properties.Settings.Default.setNTRIP_manualLon = (double)Longitude;

            Properties.Settings.Default.setNTRIP_casterURL = EnterURL;
            Properties.Settings.Default.setNTRIP_isGGAManual = GGAManual == "Use Manual Fix";
            Properties.Settings.Default.setNTRIP_isHTTP10 = HTTP == "1.0";
            Properties.Settings.Default.setNTRIP_isTCP = Usetcp;

            Properties.Settings.Default.setNTRIP_sendToSerial = ToSerial;
            Properties.Settings.Default.setNTRIP_sendToUDP = ToUDP;

            _agConnService.isSendToSerial = ToSerial;
            _agConnService.isSendToUDP = ToUDP;

            _agConnService.packetSizeNTRIP = Convert.ToInt32(PacketSize);
            Properties.Settings.Default.setNTRIP_packetSize = Convert.ToInt32(PacketSize);

            if (Properties.Settings.Default.setNTRIP_isOn && Properties.Settings.Default.setRadio_isOn)
            {
               // mf.TimedMessageBox(2000, "Radio also enabled", "Disable the Radio NTRIP");
               Console.WriteLine("Radio also enabled" + "Disable the Radio NTRIP");
                Properties.Settings.Default.setRadio_isOn = false;
            }

            Properties.Settings.Default.Save();
          
            if (!ntripStatusChanged)
            {
                  RequestClose?.Invoke(this, EventArgs.Empty);
                _agConnService.ConfigureNTRIP();
            }
            else
            {
              //  Application.Restart();
             //   Environment.Exit(0);
              RequestClose?.Invoke(this, EventArgs.Empty);
            }
          
       }
    
    private async Task MessageBoxImplAsync()
    {
         string aap = "";
         foreach (var w in dataList){                
             aap += w + Environment.NewLine;
         }
         var result = await _dialogService.ShowMessageBoxAsync(this, aap, "NTRIP SERVERS", MessageBoxButton.Ok);
         Output = result.ToString();
    }
    
    private void SetManualPositionImpl()
    {
        //  Latitude = (decimal)_agConnService.latitd; //latitude;  TODO
         // Longitude = (decimal)_agConnService.longtd; //longitude; TODO
    }
    
      private readonly List<string> dataList = new List<string>();


    private void GetSourceTableImpl()
    { 
         IPAddress casterIP = IPAddress.Parse(CasterIP.Trim()); //Select correct Address
            int casterPort = (int)CasterPort; //Select correct port (usually 80)

            Socket sckt;
            dataList?.Clear();

            try
            {
                sckt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    Blocking = true
                };
                sckt.Connect(new IPEndPoint(casterIP, casterPort));

                string msg = "GET / HTTP/1.0\r\n" + "User-Agent: NTRIP iter.dk\r\n" +
                                    "Accept: */*\r\nConnection: close\r\n" + "\r\n";

                //Send request
                byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);
                sckt.Send(data);
                int bytes = 0;
                byte[] bytesReceived = new byte[1024];
                string page = String.Empty;
                Thread.Sleep(200);

                do
                {
                    bytes = sckt.Receive(bytesReceived, bytesReceived.Length, SocketFlags.None);
                    page += Encoding.ASCII.GetString(bytesReceived, 0, bytes);
                }
                while (bytes > 0);

                if (page.Length > 0)
                {
                    string[] words = page.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < words.Length; i++)
                    {
                        string[] words2 = words[i].Split(';');

                        if (words2[0] == "STR")
                        {
                            dataList.Add(words2[1].Trim().ToString() + "," + words2[9].ToString() + "," + words2[10].ToString()
                          + "," + words2[3].Trim().ToString() + "," + words2[6].Trim().ToString()
                                );
                        }
                    }
                }
                foreach (var w in dataList){                
                     Console.WriteLine(w);
                }
            }
            catch (SocketException)
            {
                //mf.TimedMessageBox(2000, "Socket Exception", "Invalid IP:Port");
                Console.WriteLine("Socket Exception" + " Invalid IP:Port");
                return;
            }
            catch (Exception)
            {
               // mf.TimedMessageBox(2000, "Exception", "Get Source Table Error");
                Console.WriteLine("Exception" + " Get Source Table Error");
                return;
            }

            if (dataList.Count > 0)
            {
                string syte = "http://monitor.use-snip.com/?hostUrl=" + CasterIP + "&port=" + CasterPort.ToString();
              //  using (FormSource form = new FormSource(this, dataList, mf.latitude, mf.longitude, syte)) TODO
             //   {
             //       form.ShowDialog(this); TODO
             //   }
            }
            else
            {
                //mf.TimedMessageBox(2000, "Error", "No Source Data");
                Console.WriteLine("Error" + "  No Source Data");
            }

    }

    private void GetIPImpl()
    {

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
                            _agConnService.broadCasterIP = addr.ToString().Trim();
                            Properties.Settings.Default.setNTRIP_casterIP = _agConnService.broadCasterIP;
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
    
     public Boolean CheckIPValid(String strIP)
        {
            // Return true for COM Port
            if (strIP.Contains("COM"))
            {
                return true;
            }

            //  Split string by ".", check that array length is 3
            string[] arrOctets = strIP.Split('.');

            //at least 4 groups in the IP
            if (arrOctets.Length != 4) return false;

            //  Check each substring checking that the int value is less than 255 and that is char[] length is !> 2
            const Int16 MAXVALUE = 255;
            Int32 temp; // Parse returns Int32
            foreach (String strOctet in arrOctets)
            {
                //check if at least 3 digits but not more OR 0 length
                if (strOctet.Length > 3 || strOctet.Length == 0) return false;

                //make sure all digits
                if (!int.TryParse(strOctet, out int temp2)) return false;

                //make sure not more then 255
                temp = int.Parse(strOctet);
                if (temp > MAXVALUE || temp < 0) return false;
            }
            return true;
        }

        private void tboxCasterIP_Validating(object sender, CancelEventArgs e)
        {
            if (!CheckIPValid(CasterIP))
            {
                CasterIP = "127.0.0.1";
               // tboxCasterIP.Focus();
               // mf.TimedMessageBox(2000, "Invalid IP Address", "Set to Default Local 127.0.0.1");
            }
        }
        

    private void PassPasswordImpl()
    {
       UserPassword = "";
         
    }

    private void PassUsernameImpl()
    {
       Username = "";
    }

    public void OnClosed() => Closed?.Invoke(this, EventArgs.Empty);
}
