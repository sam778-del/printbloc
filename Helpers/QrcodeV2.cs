using SkiaSharp;
using SkiaSharp.QrCode.Image;
using System;
using System.IO;
using Avalonia.Media.Imaging;
using System.Drawing.Imaging;
using ZXing.QrCode;
using ZXing;

namespace PrintblocProject.Helpers
{
    public class QrcodeV2
    {
        public static SKBitmap GenerateQRCode(string content, int width = 200, int height = 200)
        {
            var writer = new ZXing.SkiaSharp.BarcodeWriter();
            var options = new ZXing.Common.EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = 0
            };
            writer.Options = options;
            writer.Format = ZXing.BarcodeFormat.QR_CODE;

            var qrCode = writer.Write($"https://printbloc.com/crc/{content}");

            // Convert the QR code to a SKBitmap
            SKBitmap skBitmap = new SKBitmap(qrCode.Width, qrCode.Height);
            using (var image = SKImage.FromBitmap(qrCode))
            {
                using (var pixmap = image.PeekPixels())
                {
                    skBitmap.InstallPixels(pixmap);
                }
            }

            return skBitmap;
        }
    }
}
