using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgConn.CrossPlatform.Interfaces;

public interface IUsbService
{

    public string[] GetPortNames();
    // public Task<string[]> GetPortNames();
}


