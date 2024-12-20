using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
﻿using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
//using static Avalonia.Media.BrushExtensions;
using Avalonia.Threading;
using AgConn.CrossPlatform.Classes;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using AgConn.CrossPlatform.Models;

using static AgConn.CrossPlatform.RegisteredServices;

namespace AgConn.CrossPlatform
{
    public class CTraffic
    {
        //public int cntrPGNFromAOG = 0;
        //public int cntrPGNToAOG = 0;

        public int cntrGPSIn = 0;
        public int cntrGPSInBytes = 0;
        public int cntrGPSOut = 0;

        //public int cntrIMUOut = 0;

       // public int cntrSteerIn = 0;
       // public int cntrSteerOut = 0;

      //  public int cntrMachineIn = 0;
       // public int cntrMachineOut = 0;

        public uint helloFromMachine = 99, helloFromAutoSteer = 99, helloFromIMU = 99;
    }
    
    public class CScanReply
    {
        public string steerIP =   "";
        public string machineIP = "";
        public string GPS_IP =    "";
        public string IMU_IP =    "";
        public string subnetStr = "";

        public byte[] subnet = { 0, 0, 0 };

        public bool isNewSteer, isNewMachine, isNewGPS, isNewIMU;

        public bool isNewData = false;
    }
    
   
    public interface IAgConnService
    {
	string nineToSixteen {get; set;}
	string oneToEight {get; set;} 
	string steerAngle {get; set;}
	string WASCounts {get; set;}
	
	string currentLat { get; set; }
	string curentLon { get; set; }
	
	double longtd { get; set; }
	double latitd { get; set; }
	
	Socket loopBackSocket { get; set; }
	Socket UDPSocket { get; set; }
	bool isUDPNetworkConnected { get; set; }
	Task LoadUDPNetwork();
	void Init();
	void Close();
	void RelayTest();
	
	ushort satellitesData { get;  }
	double latitudeSend { get;  }
	double longitudeSend { get;  }
	string FixQuality { get; }
	float hdopData { get; set; }
	float speedData { get; set; }
	
	float rollData { get; }
	short imuRollData { get; }
	short imuPitchData { get; set; }
	short imuYawRateData { get; set; }
	ushort imuHeadingData { get; set; }
	
	float ageData { get; }
	float headingTrueData { get; set; }
	float headingTrueDualData { get; set; }
	
	float  altitudeData { get; set; }
	
	string vtgSentence { get; set; }
	string ggaSentence { get; set; }
	string paogiSentence { get; set; }
	string avrSentence { get; set; }
	string hdtSentence { get; set; }
	string hpdSentence { get; set; }
	string pandaSentence { get; set; }
	string ksxtSentence { get; set; }
	
	bool isGPSSentencesOn { get; set; }
	bool isUDPMonitorOn { get; set; }
	
	//UDPMonitor
	StringBuilder logUDPSentence { get; set; }
	string iets();
	CScanReply scanReply {get; set;}
	byte[] ipAutoSet { get; set; }
	
	//NTRIP
	int packetSizeNTRIP {get; set; }
	string broadCasterIP {get; set;}
	bool isNTRIP_RequiredOn {get; set;} 
	bool isRadio_RequiredOn {get; set;} 
	bool isSerialPass_RequiredOn {get; set;} 
	bool isSendToSerial {get; set;}
	bool isSendToUDP {get; set;} 
	void ConfigureNTRIP();
    }

    public partial class AgConnService : IAgConnService
    {
	
	public double longtd { get; set; } = 0;
	public double latitd { get; set; } = 0;
	
	public string currentLat { get; set; }
	
	public string curentLon { get; set; }
	// test
	public INotifyPropertyChanged Owner { get; set; } = default!;  
	///
	
	public ushort satellitesData { get; set; } = 0;
	
	public double latitudeSend { get; set; }  = 0;
	
	public double longitudeSend { get; set; }  = 0;
	// 
	//public string FixQuality { get; set; }  = "";
	
	public float hdopData { get; set; }  = 0.00f;
	
	public float speedData { get; set; }  = 0.00f;
	
	
	public float rollData { get; set; }  = 0.00f;
	
	public short imuRollData { get; set; }  = 0;
	
	public short imuPitchData { get; set; }  = 0;
	
	public short imuYawRateData { get; set; }  = 0;
	
	public ushort imuHeadingData { get; set; }  = 0;
	
	
	public float ageData { get; set; }  = 0.00f;
	
	public float headingTrueData { get; set; }  = 0.00f;
	
	public float headingTrueDualData { get; set; }  = 0.00f;
	
	
	public float  altitudeData { get; set; }  = 0.00f;
	
	
	public string vtgSentence { get; set; }  = "";
	
	public string ggaSentence { get; set; }  = "";
	
	public string paogiSentence { get; set; }  = "";
	
	public string avrSentence { get; set; }  = "";
	
	public string hdtSentence { get; set; }  = "";
	
	public string hpdSentence { get; set; }  = "";
	
	public string pandaSentence { get; set; }  = "";
	
	public string ksxtSentence { get; set; }  = "";
	public bool isGPSSentencesOn { get; set; } = false;
	
	public bool isUDPMonitorOn { get; set; } = true;
	
	
	public StringBuilder logUDPSentence { get; set; } = new StringBuilder();
	
	

	
        // loopback Socket
        private Socket _loopBackSocket;
        public Socket loopBackSocket
        {
            get => _loopBackSocket;
            set => _loopBackSocket = value;
        }

        private EndPoint endPointLoopBack = new IPEndPoint(IPAddress.Loopback, 0);

        // UDP Socket
        private Socket _UDPSocket;
        public Socket UDPSocket
        {
            get => _UDPSocket;
            set => _UDPSocket = value;
        }
        private EndPoint endPointUDP = new IPEndPoint(IPAddress.Any, 0);

        private bool _isUDPNetworkConnected;
        public bool isUDPNetworkConnected
        {
            get => _isUDPNetworkConnected;
            set => _isUDPNetworkConnected = value;
        }
        //labels
	public string nineToSixteen {get; set;} = "00000000";
	public string oneToEight {get; set;} = "00000000";
	public string steerAngle {get; set;} = "";
	public string WASCounts {get; set;} = "";
	public string switchStatus = "";
	public string workSwitchStatus = "";
	public string ping = "";
	public string pingMachine = "";
	  

       private IPEndPoint epAgOpen = new IPEndPoint(IPAddress.Parse(
            Properties.Settings.Default.eth_loopOne.ToString() + "." +
            Properties.Settings.Default.eth_loopTwo.ToString() + "." +
            Properties.Settings.Default.eth_loopThree.ToString() + "." +
            Properties.Settings.Default.eth_loopFour.ToString()), 15555);
        
        public IPEndPoint epModule = new IPEndPoint(IPAddress.Parse(
                Properties.Settings.Default.etIP_SubnetOne.ToString() + "." +
                Properties.Settings.Default.etIP_SubnetTwo.ToString() + "." +
                Properties.Settings.Default.etIP_SubnetThree.ToString() + ".255"), 8888);
        private IPEndPoint epNtrip;

        public IPEndPoint epModuleSet = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 8888);
        public byte[] ipAutoSet { get; set; } = { 192, 168, 5 };


        //class for counting bytes
        public CTraffic traffic = new CTraffic();
        public CScanReply scanReply { get; set; } = new();
        
        public GPSData gpsData = new();

        //scan results placed here
        public string scanReturn = "Scanning...";

        // Data stream
        private byte[] buffer = new byte[1024];

        //used to send communication check pgn= C8 or 200
        private byte[] helloFromAgIO = { 0x80, 0x81, 0x7F, 200, 3, 56, 0, 0, 0x47 };

        public IPAddress ipCurrent;
        //initialize loopback and udp network
        public async Task LoadUDPNetwork()
        {
            helloFromAgIO[5] = 56;
            // lblIP.Content = ""; TODO
      
            try //udp network
            {
                foreach (IPAddress IPA in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (IPA.AddressFamily == AddressFamily.InterNetwork)
                    {
                        string data = IPA.ToString();
                        //  lblIP.Content += IPA.ToString().Trim() + "\r\n"; TODO
                    }
                }

                // Initialise the socket
                UDPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                UDPSocket.Bind(new IPEndPoint(IPAddress.Any, 9999));
                UDPSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPointUDP,
                    new AsyncCallback(ReceiveDataUDPAsync), null);

                isUDPNetworkConnected = true;
                // btnUDP.Background =  Brushes.LimeGreen; TODO
        
                //if (!isFound)
                //{
                //    MessageBox.Show("Network Address of Modules -> " + Properties.Settings.Default.setIP_localAOG+"[2 - 254] May not exist. \r\n"
                //    + "Are you sure ethernet is connected?\r\n" + "Go to UDP Settings to fix.\r\n\r\n", "Network Connection Error",
                //    MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    //btnUDP.BackColor = Color.Red;
                //    lblIP.Text = "Not Connected";
                //}
            }
            catch (Exception e)
            {
                //WriteErrorLog("UDP Server" + e);
                /**TODO
                 var msbox = MessageBoxManager.GetMessageBoxStandard(e.Message, "Serious Network Connection Error");
                 await msbox.ShowWindowDialogAsync(this);
                 ***********/
                //MessageBox.Show(e.Message, "Serious Network Connection Error",
                //   MessageBoxButtons.OK, MessageBoxIcon.Error);
                // btnUDP.Background = Brushes.Red; TODO
                //  lblIP.Content = "Error"; TODO
            }
            await Task.Delay(1000);
        }

        private async void LoadLoopback()
        {
            try //loopback
            {
                String IpAddressString = IPAddress.Loopback.ToString();
                Console.WriteLine("Loopback IP address : " + IpAddressString);
                loopBackSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                loopBackSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                loopBackSocket.Bind(new IPEndPoint(IPAddress.Loopback, 17777));
                loopBackSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPointLoopBack,
                    new AsyncCallback(ReceiveDataLoopAsync), null);
            }
            catch (Exception ex)
            {
                //lblStatus.Text = "Error";
                // MessageBox.Show("Load Error: " + ex.Message, "Loopback Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                /*****TODO
                 var msbox = MessageBoxManager.GetMessageBoxStandard("Load Error: " + ex.Message, "Loopback Server");
                 await msbox.ShowWindowDialogAsync(this);
                 ***********/
            }
        }

        #region Send LoopBack

        private void SendToLoopBackMessageAOG(byte[] byteData)
        {
           // traffic.cntrPGNToAOG += byteData.Length;
            SendDataToLoopBack(byteData, epAgOpen);
        }

       // private void SendToLoopBackMessageVR(byte[] byteData)
       // {
       //     SendDataToLoopBack(byteData, epAgVR);
       // }

        private async void SendDataToLoopBack(byte[] byteData, IPEndPoint endPoint)
        {
            try
            {
                if (byteData.Length != 0)
                {
                    // Send packet to AgVR
                    loopBackSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, endPoint,
                        new AsyncCallback(SendDataLoopAsync), null);
                }
            }
            catch (Exception ex)
            {
                //  MessageBox.Show("Send Error: " + ex.Message, "UDP Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                /*******
                 var msbox = MessageBoxManager.GetMessageBoxStandard("Send Error: " + ex.Message, "UDP Client");
                   await msbox.ShowWindowDialogAsync(this);
                   ***********/
            }
        }

        public async void SendDataLoopAsync(IAsyncResult asyncResult)
        {
            try
            {
                loopBackSocket.EndSend(asyncResult);
            }
            catch (Exception ex)
            {
                //  MessageBox.Show("SendData Error: " + ex.Message, "UDP Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                /*******TODO
                 var msbox = MessageBoxManager.GetMessageBoxStandard("SendData Error: " + ex.Message, "UDP Server");
                   await msbox.ShowWindowDialogAsync(this);
                   ***********/
            }
        }

        #endregion

        #region Receive LoopBack

        private void ReceiveFromLoopBack(byte[] data)
        {
           // traffic.cntrPGNFromAOG += data.Length;

            //Send out to udp network
            SendUDPMessage(data, epModule);

            //send out to VR Loopback
           // if (isPluginUsed) SendToLoopBackMessageVR(data);

           if (data[0] == 0x80 && data[1] == 0x81)
            {
                switch (data[3])
                {
                    case 0xFE: //254 AutoSteer Data
                        {
                            //serList.AddRange(data);
                            SendSteerModulePort(data, data.Length);
                            SendMachineModulePort(data, data.Length);
                            break;
                        }
                    case 0xEF: //239 machine pgn
                        {
                            SendMachineModulePort(data, data.Length);
                            SendSteerModulePort(data, data.Length);
                            break;
                        }
                    case 0xE5: //229 Symmetric Sections - Zones
                        {
                            SendMachineModulePort(data, data.Length);
                            //SendSteerModulePort(data, data.Length);
                            break;
                        }
                    case 0xFC: //252 steer settings
                        {
                            SendSteerModulePort(data, data.Length);
                            break;
                        }
                    case 0xFB: //251 steer config
                        {
                            SendSteerModulePort(data, data.Length);
                            break;                        }

                    case 0xEE: //238 machine config
                        {
                            SendMachineModulePort(data, data.Length);
                            SendSteerModulePort(data, data.Length);
                            break;                        }

                    case 0xEC: //236 machine config
                        {
                            SendMachineModulePort(data, data.Length);
                            SendSteerModulePort(data, data.Length);
                            break;
                        }
                }
            }                            
        }

        private void ReceiveDataLoopAsync(IAsyncResult asyncResult)
        {
            try
            {
                // Receive all data
                int msgLen = loopBackSocket.EndReceiveFrom(asyncResult, ref endPointLoopBack);

                byte[] localMsg = new byte[msgLen];
                Array.Copy(buffer, localMsg, msgLen);

                // Listen for more connections again...
                loopBackSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPointLoopBack,
                    new AsyncCallback(ReceiveDataLoopAsync), null);

                //BeginInvoke((MethodInvoker)(() => ReceiveFromLoopBack(localMsg)));
                Dispatcher.UIThread.InvokeAsync(new Action(() =>
                {
                    ReceiveFromLoopBack(localMsg);
                    Console.WriteLine("Helaas pindakaas");
                }));

            }


            catch (Exception)
            {
                //MessageBox.Show("ReceiveData Error: " + ex.Message, "UDP Server", MessageBoxButtons.OK,
                //MessageBoxIcon.Error);
            }
        }

public string iets(){ return logUDPSentence.ToString(); }

        #endregion

        #region Send UDP

        public string SendUDPMessage(byte[] byteData, IPEndPoint endPoint)
        {
            if (isUDPNetworkConnected)
            { 
                if (isUDPMonitorOn) 
                {
                    if (epNtrip != null && endPoint.Port == epNtrip.Port)
                    {
                    Console.WriteLine("NTRIP on:   " + isNTRIPLogOn);
                        if (!isNTRIPLogOn)  //fout
                            logUDPSentence.Append(DateTime.Now.ToString("ss.fff\t") + endPoint.ToString() + "\t" + " > NTRIP\r\n");
                            
                    }
                    else
                    {
                        logUDPSentence.Append(DateTime.Now.ToString("ss.fff\t") + endPoint.ToString() + "\t" + " > " + byteData[3].ToString() + "\r\n");
                                              
                        
                    }
                }

                try
                {
                    // Send packet to the zero
                    if (byteData.Length != 0)
                    {
                        UDPSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None,
                           endPoint, new AsyncCallback(SendDataUDPAsync), null);
                    }
                }
                catch (Exception)
                {
                    //WriteErrorLog("Sending UDP Message" + e.ToString());
                    //MessageBox.Show("Send Error: " + e.Message, "UDP Client", MessageBoxButtons.OK,
                    //MessageBoxIcon.Error);
                }
            }
            return logUDPSentence.ToString();
        }

        private void SendDataUDPAsync(IAsyncResult asyncResult)
        {
            try
            {
                UDPSocket.EndSend(asyncResult);
            }
            catch (Exception)
            {
                //WriteErrorLog(" UDP Send Data" + e.ToString());
                //MessageBox.Show("SendData Error: " + e.Message, "UDP Server", MessageBoxButtons.OK,
                //MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Receive UDP

        private void ReceiveDataUDPAsync(IAsyncResult asyncResult)
        {
            try
            {
                // Receive all data
                int msgLen = UDPSocket.EndReceiveFrom(asyncResult, ref endPointUDP);

                byte[] localMsg = new byte[msgLen];
                Array.Copy(buffer, localMsg, msgLen);

                // Listen for more connections again...
                UDPSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPointUDP,
                    new AsyncCallback(ReceiveDataUDPAsync), null);

                //BeginInvoke((MethodInvoker)(() => ReceiveFromUDP(localMsg)));
                Dispatcher.UIThread.InvokeAsync(new Action(() =>
                {
                    ReceiveFromUDP(localMsg);
                }));

            }
            catch (Exception)
            {
                //WriteErrorLog("UDP Recv data " + e.ToString());
                //MessageBox.Show("ReceiveData Error: " + e.Message, "UDP Server", MessageBoxButtons.OK,
                //MessageBoxIcon.Error);
            }
        }

         private void ReceiveFromUDP(byte[] data)
        {
        Console.WriteLine("data0:  " + data[0] + "  data1:  " + data[1] + "  data2:  " + data[2]);
            try
            {
                if (data[0] == 0x80 && data[1] == 0x81)
                {
                    //module return via udp sent to AOG
                    SendToLoopBackMessageAOG(data);

                    //check for Scan and Hello
                    if (data[3] == 126 && data.Length == 11)
                    {

                        traffic.helloFromAutoSteer = 0;
                        if (isViewAdvanced)
                        {
                            ping = (((DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds - pingSecondsStart) * 1000).ToString("N0");
                            double actualSteerAngle = (Int16)((data[6] << 8) + data[5]);
                            steerAngle = (actualSteerAngle * 0.01).ToString("N1");
                            WASCounts = ((Int16)((data[8] << 8) + data[7])).ToString();

                            switchStatus = ((data[9] & 2) == 2).ToString();
                            workSwitchStatus = ((data[9] & 1) == 1).ToString();
                        }
                    }

                    else if (data[3] == 123 && data.Length == 11)
                    {

                        traffic.helloFromMachine = 0;

                        if (isViewAdvanced)
                        {
                            pingMachine = (((DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds - pingSecondsStart) * 1000).ToString("N0");
                            oneToEight = Convert.ToString(data[5], 2).PadLeft(8, '0');
                            nineToSixteen = Convert.ToString(data[6], 2).PadLeft(8, '0');
                        }
                    }

                    else if (data[3] == 121 && data.Length == 11)
                        traffic.helloFromIMU = 0;

                    //scan Reply
                    else if (data[3] == 203 && data.Length == 13) //
                    {
                        if (data[2] == 126)  //steer module
                        {
                            scanReply.steerIP = data[5].ToString() + "." + data[6].ToString() + "." + data[7].ToString() + "." + data[8].ToString();
Console.WriteLine("scanrepply  " + data[5].ToString() + "." + data[6].ToString() + "." + data[7].ToString() + "." + data[8].ToString());
                            scanReply.subnet[0] = data[09];
                            scanReply.subnet[1] = data[10];
                            scanReply.subnet[2] = data[11];

                            scanReply.subnetStr = data[9].ToString() + "." + data[10].ToString() + "." + data[11].ToString();

                            scanReply.isNewData = true;
                            scanReply.isNewSteer = true;
                        }
                        //
                        else if (data[2] == 123)   //machine module
                        {
                            scanReply.machineIP = data[5].ToString() + "." + data[6].ToString() + "." + data[7].ToString() + "." + data[8].ToString();

                            scanReply.subnet[0] = data[09];
                            scanReply.subnet[1] = data[10];
                            scanReply.subnet[2] = data[11];

                            scanReply.subnetStr = data[9].ToString() + "." + data[10].ToString() + "." + data[11].ToString();

                            scanReply.isNewData = true;
                            scanReply.isNewMachine = true;

                        }
                        else if (data[2] == 121)   //IMU Module
                        {
                            scanReply.IMU_IP = data[5].ToString() + "." + data[6].ToString() + "." + data[7].ToString() + "." + data[8].ToString();

                            scanReply.subnet[0] = data[09];
                            scanReply.subnet[1] = data[10];
                            scanReply.subnet[2] = data[11];

                            scanReply.subnetStr = data[9].ToString() + "." + data[10].ToString() + "." + data[11].ToString();

                            scanReply.isNewData = true;
                            scanReply.isNewIMU = true;
                        }

                        else if (data[2] == 120)    //GPS module
                        {
                            scanReply.GPS_IP = data[5].ToString() + "." + data[6].ToString() + "." + data[7].ToString() + "." + data[8].ToString();

                            scanReply.subnet[0] = data[09];
                            scanReply.subnet[1] = data[10];
                            scanReply.subnet[2] = data[11];

                            scanReply.subnetStr = data[9].ToString() + "." + data[10].ToString() + "." + data[11].ToString();

                            scanReply.isNewData = true;
                            scanReply.isNewGPS = true;
                        }
                    }

                    if (isUDPMonitorOn)
                    {
                        logUDPSentence.Append(DateTime.Now.ToString("ss.fff\t") + endPointUDP.ToString() + "\t" + " < " + data[3].ToString() + "\r\n");
                    }

                } // end of pgns

                else if (data[0] == 36 && (data[1] == 71 || data[1] == 80 || data[1] == 75))
                {
                    traffic.cntrGPSOut += data.Length;
                    rawBuffer += Encoding.ASCII.GetString(data);
                    ParseNMEA(ref rawBuffer);
                    Console.WriteLine("************parsing NMEA");

                    if (isUDPMonitorOn && isGPSLogOn)
                    {
                        logUDPSentence.Append(DateTime.Now.ToString("ss.fff\t") + System.Text.Encoding.ASCII.GetString(data));
                    }
                }
            }
            catch
            {

            }
        }

        #endregion
    }
}
