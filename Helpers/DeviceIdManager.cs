using PrintblocProject.Model;
using System;
using System.Data.SQLite;
using System.IO;


namespace PrintblocProject.Helpers
{
    class DeviceIdManager
    {
        private const string FileName = "DDDDD-FFFF-JJJJ.txt";
        private const string tokenFile = "DDDDD-FFFF-TOKEN.txt";
        private const string teamName = "DDDDD-FFFF-TEAM.txt";
        public static void SaveDeviceTeamName(string teamname)
        {
            try
            {
                string teamnameFilePath = Path.Combine(Path.GetTempPath(), teamName);
                using (StreamWriter teamnameWriter = new StreamWriter(teamnameFilePath))
                {
                    teamnameWriter.WriteLine(teamname);
                }
                //Console.WriteLine("Teamname saved successfully!");
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }
        public static void SaveDeviceToken(string token)
        {
            try
            {
                string tokenFilePath = Path.Combine(Path.GetTempPath(), tokenFile);
                using (StreamWriter tokenWriter = new StreamWriter(tokenFilePath))
                {
                    tokenWriter.WriteLine(token);
                }
                // Console.WriteLine("Token saved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }
        public static void SaveDeviceId(string deviceId)
        {
            try
            {
                string deviceIdFilePath = Path.Combine(Path.GetTempPath(), FileName);
                using (StreamWriter deviceIdWriter = new StreamWriter(deviceIdFilePath))
                {
                    deviceIdWriter.WriteLine(deviceId);
                }
                //Console.WriteLine("Device ID saved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }

        public static string GetDeviceId()
        {
            try
            {
                string filePath = Path.Combine(Path.GetTempPath(), FileName);
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string deviceId = reader.ReadLine();
                        // Console.WriteLine("Device ID retrieved successfully!");
                        ConnectedDevice.Instance.DeviceId = deviceId;
                        return deviceId;
                    }
                }
                else
                {
                    Console.WriteLine("Device ID file does not exist.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving device ID: {ex.Message}");
                return null;
            }
        }

        public static string GetDeviceToken()
        {
            try
            {
                string filePath = Path.Combine(Path.GetTempPath(), tokenFile);
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string token = reader.ReadLine();
                        Console.WriteLine("Token retrieved successfully!");
                        AuthResult.Instance.Token = token;
                        return token;
                    }
                }
                else
                {
                    Console.WriteLine("Token file does not exist.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving token: {ex.Message}");
                return null;
            }
        }

        public static string GetDeviceTeamname()
        {
            try
            {
                string filePath = Path.Combine(Path.GetTempPath(), teamName);
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string teamname = reader.ReadLine();
                        Console.WriteLine("Teamname retrieved successfully!");
                        return teamname;
                    }
                }
                else
                {
                    Console.WriteLine("Teamname file does not exist.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving teamname: {ex.Message}");
                return null;
            }
        }

        public static void DeleteTeamNameFile()
        {
            try
            {
                DeleteFileFunc(teamName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }
        }

        public static void DeleteTokenFile()
        {
            try
            {
                DeleteFileFunc(tokenFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }
        }

        public static void DeleteFile()
        {
            try
            {
                DeleteFileFunc(FileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }
        }

        public static void DeleteFileFunc(string filePathName)
        {
            try
            {
                string filePath = Path.Combine(Path.GetTempPath(), filePathName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine("File deleted successfully!");
                }
                else
                {
                    Console.WriteLine("File does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }
        }                                                                                                                                       
    }
}
