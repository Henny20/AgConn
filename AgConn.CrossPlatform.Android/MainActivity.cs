/* Copyright 2017 Tyler Technologies Inc.
 *
 * Project home page: https://github.com/anotherlab/UsbSerialForAndroid.
 * Portions of this library are based on usb-serial-for-android (https://github.com/mik3y/usb-serial-for-android).
 * Portions of this library are based on Xamarin USB Serial for Android (https://bitbucket.org/lusovu/xamarinusbserial).
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;
using Android.App;
using Android.Content.PM;
using Android.Content;
using Android.Net;
using Android.Hardware.Usb;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Extensions;
using Hoho.Android.UsbSerial.Util;

using Android.Net.Wifi;

[assembly: UsesFeature("android.hardware.usb.host")]

namespace AgConn.CrossPlatform.Android;

[Activity(
    Label = "AgConn.CrossPlatform.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
[IntentFilter(new[] {
    UsbManager.ActionUsbDeviceAttached
})]
[MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        AgConn.CrossPlatform.RegisteredServices.UsbService = new AndroidUsbService();
        AgConn.CrossPlatform.RegisteredServices.NetService = new AndroidNetService();
        return base.CustomizeAppBuilder(builder)
            .UseReactiveUI();
    }

    public static MainActivity Instance
    {
        get;
        private set;
    }

    public static List<string> adapter = null;
    public static List<System.Net.IPAddress> ipAddresses = null;

    public static string hostname = "";

    static readonly string TAG = typeof(MainActivity).Name;
    const string ACTION_USB_PERMISSION = "com.CompanyName.AAgIO.USB_PERMISSION";

    UsbManager usbManager;
    // ListView listView;
    //  TextView progressBarTitle;
    //  ProgressBar progressBar;
    ConnectivityManager connMgr;

    // UsbSerialPortAdapter adapter;
    BroadcastReceiver detachedReceiver;
    UsbSerialPort selectedPort;

    BroadcastReceiver netReceiver;


    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);

        // Set our view from the "main" layout resource
        //SetContentView(Resource.Layout.Main);

        usbManager = GetSystemService(Context.UsbService) as UsbManager;
        // listView = FindViewById<ListView>(Resource.Id.deviceList);
        // progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
        //  progressBarTitle = FindViewById<TextView>(Resource.Id.progressBarTitle);

        connMgr = (ConnectivityManager)GetSystemService(Context.ConnectivityService);
        //   NetworkInfo networkInfo = connMgr.ActiveNetworkInfo;
        //  hostname = networkInfo.TypeName;

        //    WifiManager wifiManager = (WifiManager)GetSystemService(Service.WifiService);  
        //  hostname = wifiManager.ConnectionInfo.SSID.ToString();
        // hostname = wifiManager.ConnectionInfo.IpAddress.ToString();

    }

    protected override async void OnResume()
    {
        base.OnResume();

        // adapter = new UsbSerialPortAdapter(this);
        // listView.Adapter = adapter;

        // listView.ItemClick += async (sender, e) => {
        //    await OnItemClick(sender, e);
        // };

        await PopulateListAsync();

        //register the broadcast receivers
        detachedReceiver = new UsbDeviceDetachedReceiver(this);
        RegisterReceiver(detachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));

        netReceiver = new NetBroadcastReceiver(this);
        RegisterReceiver(netReceiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
    }
    protected override void OnPause()
    {
        base.OnPause();

        // unregister the broadcast receivers
        var temp = detachedReceiver; // copy reference for thread safety
        if (temp != null)
            UnregisterReceiver(temp);

        var tmp = netReceiver;
        if (tmp != null)
            UnregisterReceiver(tmp);
    }
    /***
    protected override void OnStop(){
        base.OnStop();

        var temp = detachedReceiver; 
        if (temp != null)
            UnregisterReceiver(temp);
    }
    *****/
    internal static Task<IList<IUsbSerialDriver>> FindAllDriversAsync(UsbManager usbManager)
    {
        // using the default probe table
        //return UsbSerialProber.GetDefaultProber().FindAllDriversAsync (usbManager);

        var table = UsbSerialProber.DefaultProbeTable;
        table.AddProduct(0x1546, 0x01a7, typeof(CdcAcmSerialDriver)); // U-Blox AG [u-blox 7]
                                                                      //table.AddProduct(0x09D8, 0x0420, typeof(CdcAcmSerialDriver)); // Elatec TWN4

        var prober = new UsbSerialProber(table);
        return prober.FindAllDriversAsync(usbManager);

    }

    async Task PopulateListAsync()
    {
        //ShowProgressBar();

        Log.Info(TAG, "Refreshing device list ...");

        var drivers = await FindAllDriversAsync(usbManager);
        adapter = new List<string>();
        //adapter.Clear();
        foreach (var driver in drivers)
        {
            var ports = driver.Ports;
            Log.Info(TAG, string.Format("+ {0}: {1} port{2}", driver, ports.Count, ports.Count == 1 ? string.Empty : "s"));
            foreach (var port in ports)
                adapter.Add(port.GetDriver().GetDevice().ProductId.ToString());
            Log.Info(TAG, "Succes ermee ...");
        }

        //adapter.NotifyDataSetChanged();
        //progressBarTitle.Text = string.Format("{0} device{1} found", adapter.Count, adapter.Count == 1 ? string.Empty : "s");
        Toast.MakeText(this, string.Format("{0} device{1} found", adapter.Count, adapter.Count == 1 ? string.Empty : "s"), ToastLength.Short).Show();
        //HideProgressBar();
        Log.Info(TAG, "Done refreshing, " + adapter.Count + " entries found.");
    }
    
     async Task NetsAsync()
     {
          var inetEnum = Java.Net.NetworkInterface.NetworkInterfaces;
            if (inetEnum is null)
            {
                ipAddresses = new List<System.Net.IPAddress>();
            }

            ipAddresses = new List<System.Net.IPAddress>();
            foreach (var interfaces in Java.Util.Collections.List(inetEnum))
            {
                var addresses = (interfaces as Java.Net.NetworkInterface)?.InetAddresses;
                if (addresses == null)
                {
                    continue;
                }

                foreach (Java.Net.InetAddress address in Java.Util.Collections.List(addresses))
                {
                    if (address.HostAddress == null || address.IsLoopbackAddress || address is not Java.Net.Inet4Address)
                    {
                        continue;
                    }
                    ipAddresses.Add(System.Net.IPAddress.Parse(address.HostAddress));
                }
            }

     }
     
    void ShowProgressBar()
    {
        // progressBar.Visibility = ViewStates.Visible;
        //   progressBarTitle.Text = GetString(Resource.String.refreshing);
    }

    void HideProgressBar()
    {
        // progressBar.Visibility = ViewStates.Invisible;
    }

    class NetBroadcastReceiver : BroadcastReceiver
    {
        readonly MainActivity activity;

        public NetBroadcastReceiver(MainActivity activity)
        {
            this.activity = activity;
        }
        public async override void OnReceive(Context context, Intent intent)
        {
            NetworkInfo networkInfo = activity.connMgr.ActiveNetworkInfo;
            hostname = networkInfo.TypeName;
            /***
            var ifaces = Java.Net.NetworkInterface.NetworkInterfaces;
            while (ifaces.HasMoreElements)
            {
                var iface = ifaces.NextElement().JavaCast<Java.Net.NetworkInterface>();
                // ...
            }
            ****/
             await activity.NetsAsync();
           
        }
    }


    #region UsbDeviceDetachedReceiver implementation

    class UsbDeviceDetachedReceiver
        : BroadcastReceiver
    {
        readonly string TAG = typeof(UsbDeviceDetachedReceiver).Name;
        readonly MainActivity activity;

        public UsbDeviceDetachedReceiver(MainActivity activity)
        {
            this.activity = activity;
        }

        public async override void OnReceive(Context context, Intent intent)
        {
            var device = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;

            Log.Info(TAG, "USB device detached: " + device.DeviceName);

            await activity.PopulateListAsync();
        }
    }

    #endregion


}



