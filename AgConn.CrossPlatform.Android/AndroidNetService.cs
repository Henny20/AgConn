using AgConn.CrossPlatform.Interfaces;
using System.Collections.Generic;
using Android.Content; 
using Android.App;
using Android.Util;
using Android.OS;

namespace AgConn.CrossPlatform.Android;

public class AndroidNetService : INetService 
{
  public string getHostName() {
       return MainActivity.hostname;
   }
   
     public List<System.Net.IPAddress> GetAllLocalValidIp4Addresses() {
      return MainActivity.ipAddresses;
   }
}   
