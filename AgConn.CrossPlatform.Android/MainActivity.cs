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
        return base.CustomizeAppBuilder(builder)
            .UseReactiveUI();
    }

    public static MainActivity Instance
    {
        get;
        private set;
    }

    public static List<string> adapter = null;
 
    public static string hostname = "";

    static readonly string TAG = typeof(MainActivity).Name;
    const string ACTION_USB_PERMISSION = "com.CompanyName.AAgIO.USB_PERMISSION";

    UsbManager usbManager;
    ConnectivityManager connMgr;
    
    BroadcastReceiver detachedReceiver;
    UsbSerialPort selectedPort;

    BroadcastReceiver netReceiver;


    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);

        usbManager = GetSystemService(Context.UsbService) as UsbManager;
      
        connMgr = (ConnectivityManager)GetSystemService(Context.ConnectivityService);
    
    }

    protected override async void OnResume()
    {
        base.OnResume();

        await PopulateListAsync();

        //register the broadcast receivers
        detachedReceiver = new UsbDeviceDetachedReceiver(this);
        RegisterReceiver(detachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));

    }
    protected override void OnPause()
    {
        base.OnPause();

        // unregister the broadcast receivers
        var temp = detachedReceiver; // copy reference for thread safety
        if (temp != null)
            UnregisterReceiver(temp);
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
        Log.Info(TAG, "Refreshing device list ...");
	
        var drivers = await FindAllDriversAsync(usbManager);
        adapter = new List<string>();

        foreach (var driver in drivers)
        {
            var ports = driver.Ports;
            Log.Info(TAG, string.Format("+ {0}: {1} port{2}", driver, ports.Count, ports.Count == 1 ? string.Empty : "s"));
            foreach (var port in ports)
                adapter.Add(port.GetDriver().GetDevice().ProductId.ToString());
            Log.Info(TAG, "Succes ...");
        }

        Toast.MakeText(this, string.Format("{0} device{1} found", adapter.Count, adapter.Count == 1 ? string.Empty : "s"), ToastLength.Short).Show();

        Log.Info(TAG, "Done refreshing, " + adapter.Count + " entries found.");
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



