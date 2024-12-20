//using System;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Linq;
using AgConn.CrossPlatform.Properties;
//using Avalonia;
using Avalonia.Threading;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;

//using static AgConn.CrossPlatform.RegisteredServices;


namespace AgConn.CrossPlatform
{
    public partial class AgConnService
    {


        private DispatcherTimer oneSecondLoopTimer = new DispatcherTimer();
        private DispatcherTimer ntripMeterTimer = new DispatcherTimer();

        /***** 
	    [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWind, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern bool IsIconic(IntPtr handle);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        *******/

        //key event to restore window
        private const int ALT = 0xA4;
        private const int EXTENDEDKEY = 0x1;
        private const int KEYUP = 0x2;

        //Stringbuilder
        public StringBuilder logNMEASentence = new StringBuilder();
        
        public StringBuilder logMonitorSentence = new StringBuilder();
       // public StringBuilder logUDPSentence = new StringBuilder();
          //public bool isLogNMEA, isLogMonitorOn, isUDPMonitorOn, isGPSLogOn, isNTRIPLogOn;
          public bool isLogNMEA, isLogMonitorOn, isGPSLogOn, isNTRIPLogOn;
        
        private StringBuilder sbRTCM = new StringBuilder();

        public bool isKeyboardOn = true;

        public bool isSendToSerial {get; set;}= true;
        public bool isSendToUDP {get; set;} = false;

        //public bool isGPSSentencesOn = false, isSendNMEAToUDP;
        public bool fake = false, isSendNMEAToUDP;

        //timer variables
        public double secondsSinceStart, twoSecondTimer, tenSecondTimer, threeMinuteTimer, pingSecondsStart;

        public string lastSentence;

        //public bool isPluginUsed;
          public bool isNTRIPToggle;

        //usually 256 - send ntrip to serial in chunks
        public int packetSizeNTRIP {get; set; }

        public bool lastHelloGPS, lastHelloAutoSteer, lastHelloMachine, lastHelloIMU;
        public bool isConnectedIMU, isConnectedSteer, isConnectedMachine;

        //is the fly out displayed
        public bool isViewAdvanced = false;
      
        //used to hide the window and not update text fields and most counters
        public bool isAppInFocus = true, isLostFocus;
        public int focusSkipCounter = 310;

        //The base directory where Drive will be stored and fields and vehicles branch from
        public string baseDirectory;

        //current directory of Comm storage
        public string commDirectory, commFileName = "";
        
        //private int smallView = 480; not very important anymore
        //labels GUI
       // public string curentLon = "";
      //  public string currentLat = "";
        public string SerialPorts = "";
        public string IP = "";
        public string skipCounter = "285";
        public string mnt = ""; //mount already exist
        public string fromGPS = "";

        public AgConnService()
        {
        /******
            oneSecondLoopTimer.Interval = TimeSpan.FromMilliseconds(4000);
            oneSecondLoopTimer.IsEnabled = true;
            oneSecondLoopTimer.Tick += oneSecondLoopTimer_Tick;
            oneSecondLoopTimer.Start();

          ntripMeterTimer.Interval = TimeSpan.FromMilliseconds(50);
           ntripMeterTimer.IsEnabled = true;
          ntripMeterTimer.Tick += ntripMeterTimer_Tick;
          ntripMeterTimer.Start();

            //currentLat = "***********";
          
            Init();
            *******/

        }

        // private void FormLoop_Load(object sender, System.EventArgs e)
        public void Init()
        {
        
         oneSecondLoopTimer.Interval = TimeSpan.FromMilliseconds(4000);
            oneSecondLoopTimer.IsEnabled = true;
            oneSecondLoopTimer.Tick += oneSecondLoopTimer_Tick;
            oneSecondLoopTimer.Start();

          ntripMeterTimer.Interval = TimeSpan.FromMilliseconds(50);
           ntripMeterTimer.IsEnabled = true;
          ntripMeterTimer.Tick += ntripMeterTimer_Tick;
          ntripMeterTimer.Start();
          
            // Deals with slash vs backslash
            if (Settings.Default.setF_workingDirectory == "Default")
                baseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AgOpenGPS");
            else baseDirectory = Path.Combine(Settings.Default.setF_workingDirectory, "AgOpenGPS");

            //get the fields directory, if not exist, create
            commDirectory = Path.Combine(baseDirectory, "AgIO" + Path.DirectorySeparatorChar);
            Console.WriteLine("dir is" + commDirectory);

            string dir = Path.GetDirectoryName(commDirectory);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

            if (Settings.Default.setUDP_isOn)
            {
                Console.WriteLine("Settings.Default.setUDP_isOn");
                LoadUDPNetwork();
            }
            else
            {
                /**********TODO
                    label2.IsVisible = false;
                    label3.IsVisible = false;
                    label4.IsVisible = false;
                    label9.IsVisible = false;

                    lblSteerAngle.IsVisible = false;
                    lblWASCounts.IsVisible = false;
                    lblSwitchStatus.IsVisible = false;
                    lblWorkSwitchStatus.IsVisible = false;

                    label10.IsVisible = false;
                    label12.IsVisible = false;
                    lbl1To8.IsVisible = false;
                    lbl9To16.IsVisible = false;

                    btnRelayTest.IsVisible = false;
					btnUDP.Background = new ImmutableSolidColorBrush(Colors.Gainsboro);
                      *************/
                      Console.WriteLine("Settings.Default.setUDP_isOFF");
                   IP = "Off";
                  
            }

            //smallView view
            //this.Width = smallView; 

            LoadLoopback();

            isSendNMEAToUDP = Properties.Settings.Default.setUDP_isSendNMEAToUDP;
            //isPluginUsed = Properties.Settings.Default.setUDP_isUsePluginApp;

            packetSizeNTRIP = Properties.Settings.Default.setNTRIP_packetSize;

            isSendToSerial = Settings.Default.setNTRIP_sendToSerial;
            isSendToUDP = Settings.Default.setNTRIP_sendToUDP;

            //lblMount.Text = Properties.Settings.Default.setNTRIP_mount;
            /*********TODO
            lblGPS1Comm.Text = "---";
            lblIMUComm.Text = "---";
            lblMod1Comm.Text = "---";
            lblMod2Comm.Text = "---";
            ************/
            //set baud and port from last time run
            baudRateGPS = Settings.Default.setPort_baudRateGPS;
            portNameGPS = Settings.Default.setPort_portNameGPS;
            wasGPSConnectedLastRun = Settings.Default.setPort_wasGPSConnected;
            if (wasGPSConnectedLastRun)
            {
                OpenGPSPort();
                if (spGPS.IsOpen) Console.WriteLine("TODO"); //lblGPS1Comm.Text = portNameGPS;
            }

            // set baud and port for rtcm from last time run
            baudRateRtcm = Settings.Default.setPort_baudRateRtcm;
            portNameRtcm = Settings.Default.setPort_portNameRtcm;
            wasRtcmConnectedLastRun = Settings.Default.setPort_wasRtcmConnected;

            if (wasRtcmConnectedLastRun)
            {
                OpenRtcmPort();
            }

            //Open IMU
            portNameIMU = Settings.Default.setPort_portNameIMU;
            wasIMUConnectedLastRun = Settings.Default.setPort_wasIMUConnected;
            if (wasIMUConnectedLastRun)
            {
                OpenIMUPort();
                if (spIMU.IsOpen) Console.WriteLine("TODO"); //lblIMUComm.Text = portNameIMU;
            }


            //same for SteerModule port
            portNameSteerModule = Settings.Default.setPort_portNameSteer;
            wasSteerModuleConnectedLastRun = Settings.Default.setPort_wasSteerModuleConnected;
            if (wasSteerModuleConnectedLastRun)
            {
                OpenSteerModulePort();
                if (spSteerModule.IsOpen) Console.WriteLine("TODO");//lblMod1Comm.Text = portNameSteerModule;
            }

            //same for MachineModule port
            portNameMachineModule = Settings.Default.setPort_portNameMachine;
            wasMachineModuleConnectedLastRun = Settings.Default.setPort_wasMachineModuleConnected;
            if (wasMachineModuleConnectedLastRun)
            {
                OpenMachineModulePort();
                if (spMachineModule.IsOpen) Console.WriteLine("TODO"); //lblMod2Comm.Text = portNameMachineModule;
            }

            ConfigureNTRIP();
            
         
            //TODO  isConnectedIMU = (bool)cboxIsIMUModule.IsChecked == Properties.Settings.Default.setMod_isIMUConnected;
            //TODO  isConnectedSteer = cboxIsSteerModule.IsChecked ?? false == Properties.Settings.Default.setMod_isSteerConnected;
            //TODO   isConnectedMachine = cboxIsMachineModule.IsChecked ?? false == Properties.Settings.Default.setMod_isMachineConnected;

            SetModulesOnOff();

            oneSecondLoopTimer.IsEnabled = true;
            /**** TODO
            pictureBox1.IsVisible = true;
            pictureBox1.BringToFront();
            pictureBox1.Width = 430;
            pictureBox1.Height = 500;
            pictureBox1.Left = 0;
            pictureBox1.Top = 0;
            //pictureBox1.Dock = DockStyle.Fill;
            ******/

            //On or off the module rows
            SetModulesOnOff();
            
        }

        public void SetModulesOnOff()
        {
            /*********TODO
                if (isConnectedIMU)
                {
                    btnIMU.IsVisible = true; 
                    lblIMUComm.IsVisible = true;
                    lblFromMU.IsVisible = true;
                   // cboxIsIMUModule.Content = Properties.Resources.Cancel64;
                }
                else
                {
                    btnIMU.IsVisible = false;
                    lblIMUComm.IsVisible = false;
                    lblFromMU.IsVisible = false;
                    //cboxIsIMUModule.BackgroundImage = Properties.Resources.AddNew; //TODO
                }

                if (isConnectedMachine)
                {
                    btnMachine.IsVisible = true;
                    lblFromMachine.IsVisible = true;
                    lblToMachine.IsVisible = true;
                    lblMod2Comm.IsVisible = true;
                    //cboxIsMachineModule.Content = Properties.Resources.Cancel64; //TODO
                }
                else
                {
                    btnMachine.IsVisible = false;
                    lblFromMachine.IsVisible = false;
                    lblToMachine.IsVisible = false;
                    lblMod2Comm.IsVisible = false;
                   // cboxIsMachineModule.BackgroundImage = Properties.Resources.AddNew; TODO
                }

                if (isConnectedSteer)
                {
                    btnSteer.IsVisible = true;
                    lblFromSteer.IsVisible = true;
                    lblToSteer.IsVisible = true; 
                    lblMod1Comm.IsVisible = true;
                    //cboxIsSteerModule.BackgroundImage = Properties.Resources.Cancel64; TODO
                }
                else
                {
                    btnSteer.IsVisible = false;
                    lblFromSteer.IsVisible = false;
                    lblToSteer.IsVisible = false;
                    lblMod1Comm.IsVisible = false;
                    //cboxIsSteerModule.Content = Properties.Resources.AddNew;
                }
                ***********/

            Properties.Settings.Default.setMod_isIMUConnected = isConnectedIMU;
            Properties.Settings.Default.setMod_isSteerConnected = isConnectedSteer;
            Properties.Settings.Default.setMod_isMachineConnected = isConnectedMachine;

            Properties.Settings.Default.Save();
        }

        public void Close()//FormLoop_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.setPort_wasGPSConnected = wasGPSConnectedLastRun;
            Settings.Default.setPort_wasIMUConnected = wasIMUConnectedLastRun;
            Settings.Default.setPort_wasSteerModuleConnected = wasSteerModuleConnectedLastRun;
            Settings.Default.setPort_wasMachineModuleConnected = wasMachineModuleConnectedLastRun;
            Settings.Default.setPort_wasRtcmConnected = wasRtcmConnectedLastRun;

            Settings.Default.Save();

            if (loopBackSocket != null)
            {
                try
                {
                    loopBackSocket.Shutdown(SocketShutdown.Both);
                }
                finally { loopBackSocket.Close(); }
            }

            if (UDPSocket != null)
            {
                try
                {
                    UDPSocket.Shutdown(SocketShutdown.Both);
                }
                finally { UDPSocket.Close(); }
            }
        }

        private void oneSecondLoopTimer_Tick(object sender, System.EventArgs e)
        {
            if (oneSecondLoopTimer.Interval > TimeSpan.FromMilliseconds(1200))
            {
                //Controls.Remove(pictureBox1); TODO
                //pictureBox1.Dispose();
                oneSecondLoopTimer.Interval = TimeSpan.FromMilliseconds(1000);
                Console.WriteLine("OSLT"); //this.Width = smallView;
                Console.WriteLine("TODO"); //this.Height = 550;
                return;
            }

           // secondsSinceStart = (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds; 

            //Hello Alarm logic
            DoHelloAlarmLogic();

            DoTraffic();

            if (focusSkipCounter != 0)
            {
                //TODO  lblCurentLon.Text = longitude.ToString("N7");
                curentLon = longitude.ToString("N7");
                //TODO lblCurrentLat.Text = latitude.ToString("N7");
                currentLat = latitude.ToString("N7");
            }

            //do all the NTRIP routines
            DoNTRIPSecondRoutine();

            //send a hello to modules
            SendUDPMessage(helloFromAgIO, epModule);
            helloFromAgIO[7] = 0;

            //#region Sleep
            /**** nvt
            //is this the active window
            // isAppInFocus = FormLoop.ActiveForm != null;
            var desktop = Application.Current.ApplicationLifetime
                 as IClassicDesktopStyleApplicationLifetime;
            isAppInFocus = desktop.MainWindow.IsActive != null; //TODO

            //start counting down to minimize
            if (!isAppInFocus && !isLostFocus)
            {
                focusSkipCounter = 310;
                isLostFocus = true;
            }

            // Is active window again
            if (isAppInFocus && isLostFocus)
            {
                isLostFocus = false;
                focusSkipCounter = int.MaxValue;
            }

            if (isLostFocus && focusSkipCounter != 0)
            {
                if (focusSkipCounter == 1)
                {
                    Console.WriteLine("TODO"); // WindowState = WindowState.Minimized;
                }

                focusSkipCounter--;
            }
            
            
            #endregion

            //every couple or so seconds
            if ((secondsSinceStart - twoSecondTimer) > 2)
            {
                TwoSecondLoop();
                twoSecondTimer = secondsSinceStart;
            }

            //every 10 seconds
            if ((secondsSinceStart - tenSecondTimer) > 9.5)
            {
                TenSecondLoop();
                tenSecondTimer = secondsSinceStart;
            }

            //3 minute egg timer
            if ((secondsSinceStart - threeMinuteTimer) > 180)
            {
                ThreeMinuteLoop();
                threeMinuteTimer = secondsSinceStart;
            }

            // 1 Second Loop Part2 
            if (isViewAdvanced)
            {
                if (isNTRIP_RequiredOn)
                {
                    sbRTCM.Append(".");  //stringbuilder RTCM
                    Console.WriteLine("TODO"); //lblMessages.Content = sbRTCM.ToString();
                }
                Console.WriteLine("TODO"); //btnResetTimer.Content = ((int)(180 - (secondsSinceStart - threeMinuteTimer))).ToString();
            }
            ****/
        }

        private void TwoSecondLoop()
        {
            if (isLogNMEA)
            {
                using (StreamWriter writer = new StreamWriter("zAgIO_log.txt", true))
                {
                    writer.Write(logNMEASentence.ToString());
                }
                logNMEASentence.Clear();
            }

            if (focusSkipCounter < 310) /*lbl*/skipCounter/*.Text*/ = focusSkipCounter.ToString();
            else Console.WriteLine("TODO"); skipCounter = "On"; //lblSkipCounter.Text = "On";

        }

        private void TenSecondLoop()
        {
       
                if (focusSkipCounter != 0)//&& WindowState == WindowState.Minimized) TODO
                {
                    focusSkipCounter = 0;
                    isLostFocus = true;
                }
      
            if (focusSkipCounter != 0)
            {
                if (isViewAdvanced && isNTRIP_RequiredOn)
                {
                    try
                    {
                        //add the uniques messages to all the new ones
                        foreach (var item in aList)
                        {
                            rList.Add(item);
                        }

                        //sort and group using Linq
                        sbRTCM.Clear();

                        var g = rList.GroupBy(i => i)
                            .OrderBy(grp => grp.Key);
                        int count = 0;
                        aList.Clear();

                        //Create the text box of unique message numbers
                        foreach (var grp in g)
                        {
                            aList.Add(grp.Key);
                            sbRTCM.AppendLine(grp.Key + " - " + (grp.Count() - 1));
                            count++;
                        }

                        rList?.Clear();

                        //too many messages or trash
                        if (count > 25)
                        {
                            aList?.Clear();
                            sbRTCM.Clear();
                            sbRTCM.Append("Reset..");
                        }

                        Console.WriteLine("TODO"); //lblMessagesFound.Text = count.ToString();
                    }

                    catch
                    {
                        sbRTCM.Clear();
                        sbRTCM.Append("Error");
                    }
                }

                #region Serial update

                if (wasIMUConnectedLastRun)
                {
                    if (!spIMU.IsOpen)
                    {
                        byte[] imuClose = new byte[] { 0x80, 0x81, 0x7C, 0xD4, 2, 1, 0, 83 };

                        //tell AOG IMU is disconnected
                        SendToLoopBackMessageAOG(imuClose);
                        wasIMUConnectedLastRun = false;
                        Console.WriteLine("IMUComm"); //lblIMUComm.Text = "---";
                    }
                }

                if (wasGPSConnectedLastRun)
                {
                    if (!spGPS.IsOpen)
                    {
                        wasGPSConnectedLastRun = false;
                        Console.WriteLine("TODO"); // lblGPS1Comm.Text = "---";
                    }
                }

                if (wasSteerModuleConnectedLastRun)
                {
                    if (!spSteerModule.IsOpen)
                    {
                        wasSteerModuleConnectedLastRun = false;
                        Console.WriteLine("TODO"); //lblMod1Comm.Text = "---";
                    }
                }

                if (wasMachineModuleConnectedLastRun)
                {
                    if (!spMachineModule.IsOpen)
                    {
                        wasMachineModuleConnectedLastRun = false;
                        Console.WriteLine("TODO"); //lblMod2Comm.Text = "---";
                    }
                }

                #endregion
            }
        }
        /************TODO
                private void btnSlide_Click(object sender, RoutedEventArgs e)
                {
                    if (this.Width < 600)
                    {
                        //this.Width = 700;
                        this.Width = 820;
                        isViewAdvanced = true;
                        btnSlide_Image.Source = new Bitmap("Assets/ArrowGrnLeft.png"); //Properties.Resources.ArrowGrnLeft; 
                        sbRTCM.Clear();
                        lblMessages.Content = "Reading...";
                        threeMinuteTimer = secondsSinceStart;
                        lblMessagesFound.Text = "-";
                        aList.Clear();  
                        rList.Clear();

                    }
                    else
                    {
                        this.Width = smallView;
                        isViewAdvanced = false;
                       // btnSlide.Content = Properties.Resources.ArrowGrnRight; 
                        btnSlide_Image.Source = new Bitmap("Assets/ArrowGrnRight.png");
                        aList.Clear();
                        rList.Clear();
                        lblMessages.Content = "Reading...";
                        lblMessagesFound.Text = "-";
                        aList.Clear();
                        rList.Clear();
                    }
                }
        ************/
        private void ThreeMinuteLoop()
        {
            if (isViewAdvanced)
            {
                Console.WriteLine("ThreeMinuteLoop"); //btnSlide.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void DoHelloAlarmLogic()
        {
            bool currentHello;

            if (isConnectedMachine)
            {
                currentHello = traffic.helloFromMachine < 3;

                if (currentHello != lastHelloMachine)
                {
                    if (currentHello) Console.WriteLine("HELLO"); //btnMachine.Background = new ImmutableSolidColorBrush(Colors.LimeGreen);
                    else Console.WriteLine("HELLO2"); //btnMachine.Background = new ImmutableSolidColorBrush(Colors.Red);
                    lastHelloMachine = currentHello;
                    ShowAgIO();
                }
            }

            Console.WriteLine("HELLO3");

            if (isConnectedSteer)
            {
                currentHello = traffic.helloFromAutoSteer < 3;

                if (currentHello != lastHelloAutoSteer)
                {
                    if (currentHello) Console.WriteLine("TODO"); //btnSteer.Background = new ImmutableSolidColorBrush(Colors.LimeGreen);
                    else Console.WriteLine("TODO"); //btnSteer.Background = new ImmutableSolidColorBrush(Colors.Red);
                    lastHelloAutoSteer = currentHello;
                    ShowAgIO();
                }
            }

            if (isConnectedIMU)
            {
                currentHello = traffic.helloFromIMU < 3;

                if (currentHello != lastHelloIMU)
                {
                    if (currentHello) Console.WriteLine("TODO"); //btnIMU.Background = new ImmutableSolidColorBrush(Colors.LimeGreen);
                    else Console.WriteLine("TODO"); //btnIMU.Background = new ImmutableSolidColorBrush(Colors.Red);
                    lastHelloIMU = currentHello;
                    ShowAgIO();
                }
            }

            currentHello = traffic.cntrGPSOut != 0;

            if (currentHello != lastHelloGPS)
            {
                if (currentHello) Console.WriteLine("TODO"); // btnGPS.Background =  new SolidColorBrush(Colors.LimeGreen);
                else Console.WriteLine("TODO"); //btnGPS.Background = new ImmutableSolidColorBrush(Colors.Red);
                lastHelloGPS = currentHello;
                ShowAgIO();
            }
        }

        private void FormLoop_Resize(object sender, RoutedEventArgs e)
        {
            /***********
                if(this.WindowState == WindowState.Minimized)
                {
                    if (isViewAdvanced) Console.WriteLine("TODO"); //btnSlide.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    isLostFocus = true;
                    focusSkipCounter = 0;
                }
                *************/
        }

        private void ShowAgIO()
        {
            /** vragen om ellende
             using var process = Process.Start(
               new ProcessStartInfo
               {
                  FileName = "AAgIO",
                  //ArgumentList = { "hello world" }
               });
             process.WaitForExit();
             ********/


            Process[] processName = Process.GetProcessesByName("AgIO");

            if (processName.Length != 0)
            {
                /********** not Linux
                 // Guard: check if window already has focus.
                 if (processName[0].MainWindowHandle == GetForegroundWindow()) return;

                 // Show window maximized.
                 ShowWindow(processName[0].MainWindowHandle, 9);

                 // Simulate an "ALT" key press.
                 keybd_event((byte)ALT, 0x45, EXTENDEDKEY | 0, 0);

                 // Simulate an "ALT" key release.
                 keybd_event((byte)ALT, 0x45, EXTENDEDKEY | KEYUP, 0);

                 // Show window in forground.
                 SetForegroundWindow(processName[0].MainWindowHandle);
                ************/
                Console.WriteLine("Jammer, maar helaas");
            }

            //{
            //    //Set foreground window
            //    if (IsIconic(processName[0].MainWindowHandle))
            //    {
            //        ShowWindow(processName[0].MainWindowHandle, 9);
            //    }
            //    SetForegroundWindow(processName[0].MainWindowHandle);
            //}
        }

        private void DoTraffic()
        {
            traffic.helloFromMachine++;
            traffic.helloFromAutoSteer++;
            traffic.helloFromIMU++;

            if (focusSkipCounter != 0)
            {

                fromGPS = traffic.cntrGPSOut == 0 ? "--" : (traffic.cntrGPSOut).ToString();

                 //reset all counters
                traffic.cntrGPSOut = 0;

                curentLon = longitude.ToString("N7"); //lblCurentLon.Text = longitude.ToString("N7");
                currentLat = latitude.ToString("N7");//lblCurrentLat.Text = latitude.ToString("N7");
            }
        }

        // Buttons, Checkboxes and Clicks  ***************************************************

        private void RescanPorts()
        {
           string[] ports = null;
            if (OperatingSystem.IsAndroid()) { 
            //ports = UsbService.GetPortNames();
            ports = new string[3] {"device1", "device2", "device3"}; 
           }   
            else {
            ports = System.IO.Ports.SerialPort.GetPortNames();
            }
                
            if (ports.Length == 0)
            {
                Console.WriteLine("TODO"); //lblSerialPorts.Text = "None";
            }
            else
            {
                for (int i = 0; i < ports.Length; i++)
                {
                    Console.WriteLine("TODO"); // lblSerialPorts.Text = ports[i] + " ";
                }
            }
        }
        /*************
                private void deviceManagerToolStripMenuItem_Click(object sender, RoutedEventArgs e)
                {
                    if (OperatingSystem.IsWindows())
                      Process.Start("devmgmt.msc");
                    else
                    {
                       //Console.WriteLine(" Koop een andere fiets");  
                       var list = DeviceList.Local;
                       //var device = list.GetHidDevices().FirstOrDefault();
                       var allDevices = list.GetAllDevices();
                       foreach (HidDevice dev in allDevices)
                       {
                          Console.WriteLine("  " + dev.ToString());
                       }

                    }
                }
        *************/
        private void isSteerModule()
        {
            //TODO isConnectedSteer = cboxIsSteerModule.IsChecked ?? false;
            SetModulesOnOff();
        }

        private void IsMachineModule()
        {
            //TODO isConnectedMachine = cboxIsMachineModule.IsChecked ?? false;
            SetModulesOnOff();
        }

        private void Messages()
        {
            aList?.Clear();
            sbRTCM.Clear();
            sbRTCM.Append("Reset..");
        }

        private void btnResetTimer_Click(object sender, RoutedEventArgs e)
        {
            threeMinuteTimer = secondsSinceStart;
        }
        /*************
                private async void serialPassThroughToolStripMenuItem_Click(object sender, RoutedEventArgs e)
                {
                    if (isRadio_RequiredOn)
                    {
                        var tmb = new TimedMessageBox(2000, "Radio NTRIP ON", "Turn it off before using Serial Pass Thru");
                        tmb.ShowDialog(this);
                        return;
                    }

                    if (isNTRIP_RequiredOn)
                    {
                        var tbox = new TimedMessageBox(2000, "Air NTRIP ON", "Turn it off before using Serial Pass Thru");
                        tbox.ShowDialog(this);
                        return;
                    }


                    var form = new SerialPassView(this);
                    if (await form.ShowDialog<bool>(this) == true)
                    {
                         if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                         {
                             //desktop.ShutdownRequested += This_ShutdownRequested;
                             desktop.MainWindow = new MainWindow();
                         }  
                            ////Clicked Save
                            //Application.Restart();
                         Environment.Exit(0);
                    }

                }
        *************/
        public void RelayTest()
        {
            helloFromAgIO[7] = 1;
        }
        /***********
                private void toolStripMenuItem4_Click(object sender, RoutedEventArgs e)
                { ************/
        /****
        Form f = Application.OpenForms["FormGPSData"];

        if (f != null)
        {
            f.Focus();
            f.Close();
            isGPSSentencesOn = false;
            return;
        }
         *****/
        /*************
           var windows = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Windows;

      foreach (Window w in windows)
      {
          if (w.ToString() == "AAgIO.Views.GPSDataView")
          {
               if (w.IsActive)
               {
                   w.Focus();
                   w.Close();
                   isGPSSentencesOn = false;
                   return;
               }
           }		
       }	
       isGPSSentencesOn = true;

       var view = new GPSDataView(this);
       view.ShowDialog(this);
   }
**************/
        private void lblIP_Click(object sender, RoutedEventArgs e)
        {
            //TODO lblIP.Content = "";

            foreach (IPAddress IPA in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (IPA.AddressFamily == AddressFamily.InterNetwork)
                {
                    string data = IPA.ToString();
                    //TODO lblIP.Content += IPA.ToString() + "\r\n";
                }
            }
        }

        private void toolStripMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            //Save curent Settngs
            //TODO var form = new CommSaverView(this);
            ///form.ShowDialog(this);

        }
        /**********TODO
                private async void toolStripMenuItem2_Click(object sender, RoutedEventArgs e)
                {
                    //Load new settings
                    //using (var form = new FormCommPicker(this))
                    var view = new CommPickerView(this);
                    var result = await view.ShowDialog<DialogResult>(this);


                    if (result == DialogResult.OK)
                    {
                        //Application.Restart();
                        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                        {
                               desktop.Shutdown();
                               desktop.MainWindow = new MainWindow();
                        }
                        Environment.Exit(0);
                    }
           ****************/

        // }

        private void lblNTRIPBytes_Click(object sender, RoutedEventArgs e)
        {
            tripBytes = 0;
        }

        private void cboxIsIMUModule_Click(object sender, RoutedEventArgs e)
        {
            //TODO isConnectedIMU = cboxIsIMUModule.IsChecked ?? false;
            SetModulesOnOff();
        }

        private void btnBringUpCommSettings_Click(object sender, RoutedEventArgs e)
        {
            //TODO SettingsCommunicationGPS();
            RescanPorts();
        }

        private void btnUDP_Click(object sender, RoutedEventArgs e)
        {
            //TODO SettingsUDP();
        }

        private void btnRunAOG_Click(object sender, RoutedEventArgs e)
        {
            //TODO StartAOG();
        }

        private void btnNTRIP_Click(object sender, RoutedEventArgs e)
        {
            //TODO SettingsNTRIP();
        }

        //     private async void btnExit_Click(object sender, RoutedEventArgs e)
        //   {
        //test
        //var result = await  AAgIO.Dialogs.MessageBox.Show(this, "Bevestig", "Afsluiten?", AAgIO.Dialogs.MessageBox.MessageBoxButtons.YesNoCancel);
        //if (result == AAgIO.Dialogs.MessageBox.MessageBoxResult.Yes)
        //      Close();
        //  }
        /***********
                private void pictureBox2_Click(object sender, RoutedEventArgs e)
                { **********/
        /*******
         Form f = Application.OpenForms["FormGPSData"];

         if (f != null)
         {
             f.Focus();
             f.Close();
             isGPSSentencesOn = false;
             return;
         }
         **********/
        /**************
       var windows = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Windows;

       foreach (Window w in windows)
       {
           if (w.ToString() == "AAgIO.Views.GPSDataView")
           {
                if (w.IsActive)
                {
                    w.Focus();
                    w.Close();
                    isGPSSentencesOn = false;
                    return;
                }
            }		
        }	

        isGPSSentencesOn = true;

        var view = new GPSDataView(this);
        view.Show(this);
    }
*************/
        private void btnRadio_Click_1(object sender, RoutedEventArgs e)
        {
            //TODO SettingsRadio();
        }


        ///    private async void btnWindowsShutDown_Click(object sender, RoutedEventArgs e)
        //    {
        // var result3 = callMessageBox("Shutdown Windows For Realz ?" +
        //     "For Sure For Sure ?");
        /**** 
        DialogResult result3 = MessageBox.Show("Shutdown Windows For Realz ?",
            "For Sure For Sure ?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);
        ******/
        /********
        var box = MessageBoxManager.GetMessageBoxStandard(
           new MessageBoxStandardParams
           {
               ButtonDefinitions = ButtonEnum.YesNo,
               ContentTitle = "",
               ContentMessage = "Shutdown Windows For Realz ?  " +
            "For Sure For Sure ?",
               Icon = MsBox.Avalonia.Enums.Icon.Question
           });   
        var result = await box.ShowWindowDialogAsync(this);
        Console.WriteLine("resultaat " + result);
        if (result == ButtonResult.Yes)
        {
          if (OperatingSystem.IsWindows())
            Process.Start("shutdown", "/s /t 0");
        }
    }
    ***********/
        /*********
                private void toolStripGPSData_Click(object sender, RoutedEventArgs e)
                {

                    Form f = Application.OpenForms["FormGPSData"];

                    if (f != null)
                    {
                        f.Focus();
                        f.Close();
                        isGPSSentencesOn = false;
                        return;
                    }
                    **********/
        /********  var windows = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Windows;

          foreach (Window w in windows)
          {
              if (w.ToString() == "AAgIO.Views.GPSDataView")
              {
                   if (w.IsActive)
                   {
                       w.Focus();
                       w.Close();
                       isGPSSentencesOn = false;
                       return;
                   }
               }		
           }	

           isGPSSentencesOn = true;

           var view = new GPSDataView(this);
           view.Show(this);

       }
       **************/
        private void cboxLogNMEA_CheckedChanged(object sender, RoutedEventArgs e)
        {
            //TODO  isLogNMEA = cboxLogNMEA.IsChecked ?? false;
        }

    }

}
