using System;
using AgConn.CrossPlatform.Interfaces;

namespace AgConn.CrossPlatform
{
    public static class RegisteredServices
    {

        public static IUsbService? UsbService { get; set; }
       // public static INetService? NetService { get; set; }
    }
}
