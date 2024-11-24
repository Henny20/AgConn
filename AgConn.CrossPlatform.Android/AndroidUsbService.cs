using AgConn.CrossPlatform.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

using Android.Hardware.Usb;
using Android.Content; 
using Android.App;
using Android.Util;
using Android.OS;

using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Extensions;
using Hoho.Android.UsbSerial.Util;

namespace AgConn.CrossPlatform.Android;

public class AndroidUsbService : Service, IUsbService 
{
   static readonly string TAG = typeof(AndroidUsbService).Name;
   const int READ_WAIT_MILLIS = 200;
   const int WRITE_WAIT_MILLIS = 200;
   UsbSerialPort port;
   UsbManager usbManager;
   SerialInputOutputManager serialIoManager;
   public string message;
   
   public override void OnCreate()
   {
       // This method is optional to implement
       base.OnCreate();
       Log.Debug(TAG, "OnCreate");
   }
   
   public override IBinder OnBind(Intent intent) {
        return null; 
   } 

   public string[] GetPortNames() {
      return MainActivity.adapter.ToArray();
   }
  
   void WriteData(byte[] data)
   {
        if (serialIoManager.IsOpen)
        {
            port.Write(data, WRITE_WAIT_MILLIS);
        }
   }

   void UpdateReceivedData(byte[] data)
   {
        message = "Read " + data.Length + " bytes: \n"
            + HexDump.DumpHexString(data) + "\n\n";
   }
}
