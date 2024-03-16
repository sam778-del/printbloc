using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using PrintblocProject.Helpers;
using PrintblocProject.Model;
using PrintblocProject.Utils;
using PropertyChanged;
using SkiaSharp;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PrintblocProject.Views
{
    [DoNotNotify]
    public partial class MainWindow : Window
    {
        private string deviceId = DeviceIdManager.GetDeviceId();
        private Timer connectivityCheckTimer;
        private bool deviceIDPresent = false;
        public MainWindow()
        {
            this.InitializeComponent();
            _ = UpdateQRCode();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task UpdateQRCode()
        {
            int width = 200;
            int height = 200;
            while (!deviceIDPresent)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));

                if (deviceId != null)
                {
                    deviceIDPresent = true; // Set flag to true to stop further refreshing
                    var qrCodeBitmap = QrcodeV2.GenerateQRCode(deviceId, width, height);
                    var byteArray = ConvertToPng(qrCodeBitmap);
                    using (var stream = new MemoryStream(byteArray))
                    {
                        var image = this.FindControl<Image>("QRCodeImage");
                        image.Source = new Bitmap(stream);
                    }
                }
            }
        }

        public static byte[] ConvertToPng(SKBitmap skBitmap)
        {
            using (var image = SKImage.FromBitmap(skBitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = new MemoryStream())
            {
                data.SaveTo(stream);
                return stream.ToArray();
            }
        }
    }
}