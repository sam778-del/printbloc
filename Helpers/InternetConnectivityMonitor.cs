using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Timers;
using PrintblocProject.Models;

namespace PrintblocProject.Helpers
{
    public class InternetConnectivityMonitor
    {
        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        private static Timer connectivityCheckTimer;

        public static void StartMonitoring(int intervalInSeconds)
        {
            connectivityCheckTimer = new Timer(intervalInSeconds * 1000);
            connectivityCheckTimer.Elapsed += OnTimedEvent;
            connectivityCheckTimer.AutoReset = true;
            connectivityCheckTimer.Enabled = true;
        }

        private static void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            bool isConnected = IsConnectedToInternet();
            //Status.Instance.Online = isConnected;
        }

        public static bool IsConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }

        // You can stop monitoring by calling this method
        public static void StopMonitoring()
        {
            connectivityCheckTimer?.Stop();
            connectivityCheckTimer?.Dispose();
        }
    }
}
