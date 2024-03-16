using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using Microsoft.Graph.Models;
using PrintblocProject.Model;
using Status = PrintblocProject.Model.Status;
using Avalonia.Threading;
using System.Runtime.InteropServices;

namespace PrintblocProject.Helpers
{
    public class SignalRBackgroundService
    {
        private static string apiUrl = ApiBaseUrl.BaseUrl;

        private HubConnection hubConnection;
        private CancellationTokenSource cancellationTokenSource;
        private string deviceId = ConnectedDevice.Instance.DeviceId;
        ProcessingWindow processingWindow = null;
        PrintFailureWindow printFailureWindow = null;
        public async void StartSignalRConnectionInBackground()
        {
            string signalRUrl = apiUrl + "printjob";
            hubConnection = new HubConnectionBuilder()
                .WithUrl(signalRUrl)
                .Build();

            hubConnection.On<PrintJobModel>("ReceivePrintJobs", async (job) =>
            {
                Console.WriteLine("retrieving device " + deviceId);
                if (job.DeviceId == deviceId && job.Status == "pending" || job.Status == "reprint")
                {
                    Status.Instance.Online = true;
                    await SendToPrinter(job);
                }
            });

            cancellationTokenSource = new CancellationTokenSource();
            await ConnectWithRetry(cancellationTokenSource.Token);
        }

        public void StopSignalRConnection()
        {
            cancellationTokenSource?.Cancel();
            hubConnection?.StopAsync().Wait();
            hubConnection?.DisposeAsync();
        }

        private async Task ConnectWithRetry(CancellationToken cancellationToken)
        {
            try
            {
                await hubConnection.StartAsync(cancellationToken);
                Status.Instance.Online = true;
                await AccountManager.updateConnection(hubConnection.ConnectionId, deviceId);
                Console.WriteLine($"SignalR connection started. Connection ID: {hubConnection.ConnectionId}");

                hubConnection.Closed += async (error) =>
                {
                    Status.Instance.Online = false;
                    Console.WriteLine($"SignalR connection closed: {error?.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                    await ConnectWithRetry(cancellationToken);
                };
            }
            catch (Exception ex)
            {
                Status.Instance.Online = false;
                Console.WriteLine($"Error starting SignalR connection: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                await ConnectWithRetry(cancellationToken);
            }
        }

        public async Task<bool> SendToPrinter(PrintJobModel job)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Print printHelper = new Print();
                    int JobId = job.Id;
                    string fileUrl = job.FilePath;
                    //string fileUrl = "";
                    string printerName = job.PrinterName;
                    bool isColor = job.Color;
                    int startPage = job.StartPage;
                    int endPage = job.EndPage;
                    int numberOfCopies = job.Copies;
                    string paperName = job.PaperName;
                    bool landScape = job.LandScape;
                    await printHelper.PrintFileFromUrl(fileUrl, printerName, paperName, isColor, startPage, endPage, landScape, numberOfCopies, JobId);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    PrintLinux printHelperLinux = new PrintLinux();
                    int JobId = job.Id;
                    string fileUrl = job.FilePath;
                    //string fileUrl = "";
                    string printerName = job.PrinterName;
                    bool isColor = job.Color;
                    int startPage = job.StartPage;
                    int endPage = job.EndPage;
                    int numberOfCopies = job.Copies;
                    string paperName = job.PaperName;
                    bool landScape = job.LandScape;
                    await printHelperLinux.PrintFileFromUrl(fileUrl, printerName, paperName, isColor, startPage, endPage, landScape, numberOfCopies, JobId);
                }
                return true;
            }
            catch (Exception ex)
            {
                String Status = "failed";
                String Message = $"Error printing your document: {ex.Message}";
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    printFailureWindow = new PrintFailureWindow(job.Id, Status, Message, 0, "None");
                    printFailureWindow.Show();
                });
                return false;
            }
        }
    }
}
