using System;
using System.Management;
using System.Text;
using System.Security.Cryptography;
using System.Net.Http;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace PrintblocProject.Helpers
{
    public class DeviceInformation
    {
        public async Task<string> GetLocalIPAddress()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = "https://ipinfo.io/ip";
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string publicIpAddress = await response.Content.ReadAsStringAsync();
                        return publicIpAddress;
                    }
                    else
                    {
                        Console.WriteLine("Failed to retrieve public IP address. Status code: " + response.StatusCode);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving public IP address: " + ex.Message);
                return null;
            }
        }

        //public static string getlocalipaddress()
        //{
        //    try
        //    {
        //        string localip = string.empty;
        //        iphostentry host = dns.gethostentry(dns.gethostname());

        //        foreach (ipaddress ip in host.addresslist)
        //        {
        //            if (ip.addressfamily == addressfamily.internetwork)
        //            {
        //                localip = ip.tostring();
        //                break;
        //            }
        //        }

        //        return localip;
        //    }
        //    catch (exception ex)
        //    {
        //        console.writeline("error retrieving local ip address: " + ex.message);
        //        return null;
        //    }
        //}

        public string GetProcessorName()
        {
            string processorName = string.Empty;

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows implementation
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        processorName = obj["Name"].ToString();
                        break; // Assuming there's only one processor
                    }
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    // Linux implementation
                    string cpuInfoPath = "/proc/cpuinfo";
                    if (File.Exists(cpuInfoPath))
                    {
                        string[] lines = File.ReadAllLines(cpuInfoPath);
                        foreach (string line in lines)
                        {
                            if (line.StartsWith("model name", StringComparison.OrdinalIgnoreCase))
                            {
                                int index = line.IndexOf(':');
                                if (index != -1)
                                {
                                    processorName = line.Substring(index + 1).Trim();
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Unsupported operating system.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving processor information: " + ex.Message);
            }

            return processorName;
        }

        public string GetMachineName()
        {
            string machineName = Environment.MachineName;
            return machineName;
        }

        public string GetOperatingSystemInfo()
        {
            string osInfo = string.Empty;

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows implementation
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        osInfo = obj["Caption"] + " - Version: " + obj["Version"];
                        break; // Assuming there's only one operating system
                    }
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    // Linux implementation
                    string osReleasePath = "/etc/os-release";
                    if (File.Exists(osReleasePath))
                    {
                        string[] lines = File.ReadAllLines(osReleasePath);
                        foreach (string line in lines)
                        {
                            if (line.StartsWith("PRETTY_NAME", StringComparison.OrdinalIgnoreCase))
                            {
                                int index = line.IndexOf('=');
                                if (index != -1)
                                {
                                    osInfo = line.Substring(index + 1).Trim('"');
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Unsupported operating system.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving operating system information: " + ex.Message);
            }

            return osInfo;
        }


        public string GetUniqueDeviceIdentifier()
        {
            string macAddress = GetMacAddress();

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(macAddress));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < 4; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString().Substring(0, 8);
            }
        }

        private string GetMacAddress()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                string macAddress = "";

                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"])
                    {
                        macAddress = mo["MacAddress"].ToString();
                        break;
                    }
                }

                return macAddress;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo("ip", "link show");
                    psi.RedirectStandardOutput = true;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;

                    using (Process process = new Process())
                    {
                        process.StartInfo = psi;
                        process.Start();

                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        string[] lines = output.Split('\n');
                        foreach (string line in lines)
                        {
                            if (line.Contains("link/ether"))
                            {
                                string[] parts = line.Split(' ');
                                return parts[parts.Length - 1];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error retrieving MAC address on Linux: " + ex.Message);
                }
            }

            return string.Empty;
        }

    }
}
