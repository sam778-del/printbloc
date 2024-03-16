using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Timers;
using PrintblocProject.Model;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Avalonia.Logging;

namespace PrintblocProject.Helpers
{
    public class PrinterMonitor
    {
        private Timer printerCheckTimer;

        public class DetailedPrinterInfo
        {
            public string PrinterName { get; set; }
            public string PrinterType { get; set; }
            public string PrinterColor { get; set; }
            public string PrinterID { get; set; }
        }

        public void StartMonitoringPrinters()
        {
            printerCheckTimer = new Timer();
            printerCheckTimer.Interval = 5000;
            printerCheckTimer.Elapsed += CheckPrinters;
            printerCheckTimer.AutoReset = true;
            printerCheckTimer.Start();
        }

        private async void CheckPrinters(object sender, ElapsedEventArgs e)
        {
            Logger.TryGet(LogEventLevel.Fatal, LogArea.Control)?.Log(this, "Avalonia Infrastructure");
            string deviceId = DeviceIdManager.GetDeviceId();
            if (!string.IsNullOrEmpty(deviceId))
            {
                try
                {
                    List<string> printers = new List<string>();
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        foreach (string printer in PrinterSettings.InstalledPrinters)
                        {
                            printers.Add(printer);
                        }
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        printers = GetLinuxPrinters();
                    }

                        await DisplayPrinterInformation(printers);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error retrieving printer information: " + ex.Message);
                }
            }
        }


        public async Task<List<DetailedPrinterInfo>> DisplayPrinterInformation(List<string> printers)
        {
            List<DetailedPrinterInfo> printerInfoList = new List<DetailedPrinterInfo>();
            string deviceId = DeviceIdManager.GetDeviceId();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                foreach (string printer in printers)
                {
                    DetailedPrinterInfo printerInfo = new DetailedPrinterInfo();
                    printerInfo.PrinterName = printer;
                    PrinterSettings settings = new PrinterSettings { PrinterName = printer };
                    printerInfo.PrinterColor = settings.SupportsColor ? "Color" : "Monochrome";
                    printerInfoList.Add(printerInfo);
                    await AccountManager.StoreDevicePrinter(deviceId, printerInfo.PrinterName, printerInfo.PrinterColor);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                foreach (string printer in printers)
                {
                    DetailedPrinterInfo printerInfo = new DetailedPrinterInfo();
                    printerInfo.PrinterName = printer;
                    printerInfo.PrinterColor = "Color";
                    printerInfoList.Add(printerInfo);
                    await AccountManager.StoreDevicePrinter(deviceId, printerInfo.PrinterName, printerInfo.PrinterColor);
                }
            }

            return printerInfoList;
        }

        public void StopMonitoringPrinters()
        {
            if (printerCheckTimer != null)
            {
                printerCheckTimer.Stop();
                printerCheckTimer.Dispose();
            }
        }

        private List<string> GetLinuxPrinters()
        {
            List<string> printers = new List<string>();

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "lpstat",
                    Arguments = "-p",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = "/etc/cups"
                };

                Process process = new Process { StartInfo = psi };
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Error: {error}");
                }
                else
                {
                    string[] lines = output.Split('\n');
                    foreach (string line in lines)
                    {
                        // Extract printer name from the output
                        if (line.StartsWith("printer"))
                        {
                            string[] parts = line.Split(' ');
                            if (parts.Length > 1)
                            {
                                string printerName = parts[1];
                                printers.Add(printerName);
                            }
                        }
                    }
                }
                return printers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving printer information: {ex.Message}");
            }

            return printers;
        }

    }
}
