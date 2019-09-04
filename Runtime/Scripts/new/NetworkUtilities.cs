using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace PerformanceRecorder
{
    public class NetworkUtilities
    {
        /// <summary>
        /// Get an array of online IPv4 addresses from all possible network interfaces in the system.
        /// </summary>
        /// <returns>Array of available IP addresses.</returns>
        public static IPAddress[] GetIPAddresses()
        {
            var addresses = new List<IPAddress>();

            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                switch (networkInterface.OperationalStatus)
                {
                    case OperationalStatus.Up:
                        foreach (var ip in networkInterface.GetIPProperties().UnicastAddresses)
                        {
                            var address = ip.Address;

                            if (address.AddressFamily != AddressFamily.InterNetwork)
                                continue;
                            
                            addresses.Add(address);
                        }

                        break;
                }
            }

            return addresses.ToArray();
        }
    }
}
