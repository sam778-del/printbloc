using System;
using System.Drawing.Printing;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Threading;
using PdfiumViewer;
using Point = System.Drawing.Point;

namespace PrintblocProject.Helpers
{
    public class Print
    {
        public async Task<bool> PrintFileFromUrl(
            string fileUrl,
            string printerName,
            string paperName,
            bool isColor,
            int startPage,
            int endPage,
            bool landScape,
            int numberOfCopies,
            int JobId)
        {
            bool printStatus = false;
            string tempFilePath = await DownloadFileAsync(fileUrl);
            if (!string.IsNullOrEmpty(tempFilePath))
            {
                try
                {
                    string fileExtension = Path.GetExtension(tempFilePath).ToLower();

                    switch (fileExtension)
                    {
                        case ".jpg":
                        case ".jpeg":
                        case ".png":
                        case ".gif":
                            printStatus = await PrintImage(tempFilePath, printerName, paperName, isColor, startPage, endPage, landScape, numberOfCopies, JobId, fileExtension);
                            break;
                        case ".pdf":
                            Console.WriteLine("started printing");
                            printStatus = await PrintPdf(tempFilePath, printerName, paperName, isColor, startPage, endPage, landScape, numberOfCopies, JobId, fileExtension);
                            break;
                        default:
                            Console.WriteLine("Unsupported file type for printing.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    await AccountManager.updatePrintJob(JobId, "failed", "File does not exist at the specified path.", 0, "None");
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        PrintFailureWindow printFailureWindow = new PrintFailureWindow(JobId, "failed", $"Error while identifying or printing the file: {ex.Message}", 0, "None");
                        printFailureWindow.Show();
                    });
                    Console.WriteLine($"Error while identifying or printing the file: {ex.Message}");
                    printStatus = false;
                }
                finally
                {
                    File.Delete(tempFilePath);
                }
                return printStatus;
            }
            else
            {
                await AccountManager.updatePrintJob(JobId, "failed", "File does not exist at the specified path.", 0, "None");
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    PrintFailureWindow printFailureWindow = new PrintFailureWindow(JobId, "failed", "File does not exist at the specified path.", 0, "None");
                    printFailureWindow.Show();
                });
                return false;
            }
        }


        public async Task<bool> PrintImage(
            string imagePath,
            string printerName,
            string paperName,
            bool isColor,
            int startPage,
            int endPage,
            bool landScape,
            int numberOfCopies,
            int JobId,
            string fileExtension)
        {
            int counter = 0;
            ProcessingWindow processingWindow = null;
            try
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    processingWindow = new ProcessingWindow();
                    processingWindow.Show();
                });

                var printerSettings = new PrinterSettings
                {
                    PrinterName = printerName,
                    Copies = (short)numberOfCopies,
                };
                var pageSettings = new PageSettings(printerSettings)
                {
                    Color = isColor,
                    Margins = new Margins(0, 0, 0, 0),
                    Landscape = landScape,
                };

                foreach (PaperSize papersize in printerSettings.PaperSizes)
                {
                    if (papersize.PaperName == paperName)
                    {
                        pageSettings.PaperSize = papersize;
                        break;
                    }
                }

                using (System.Drawing.Image imageToPrint = System.Drawing.Image.FromFile(imagePath))
                {
                    using (var printDocument = new PrintDocument())
                    {
                        printDocument.PrinterSettings = printerSettings;
                        printDocument.DefaultPageSettings = pageSettings;
                        printDocument.OriginAtMargins = true;
                        printDocument.PrinterSettings.FromPage = startPage;
                        printDocument.PrinterSettings.ToPage = endPage;
                        printDocument.PrintPage += (s, e) =>
                        {
                            Point loc = new Point(100, 100);
                            e.Graphics.DrawImage(imageToPrint, loc);
                            counter++;
                        };
                        printDocument.Print();
                    }

                }

                Console.WriteLine("Document printed" + counter);
                String Status = "printed";
                String Message = "Document printed succesfully";
                await AccountManager.updatePrintJob(JobId, Status, Message, counter, fileExtension);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (processingWindow != null)
                        processingWindow.Close();
                });
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    PrintSuccessWindow printSuccessWindow = new PrintSuccessWindow(JobId, Status, Message, counter, fileExtension);
                    printSuccessWindow.Show();
                });
                return true;
            }
            catch (Exception ex)
            {

                String Status = "failed";
                String Message = $"Error printing your document: {ex.Message}";
                await AccountManager.updatePrintJob(JobId, Status, Message, counter, fileExtension);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (processingWindow != null)
                        processingWindow.Close();
                });
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    PrintFailureWindow printFailureWindow = new PrintFailureWindow(JobId, Status, Message, counter, fileExtension);
                    printFailureWindow.Show();
                });
                return false;
            }
        }

        public async Task<bool> PrintPdf(
            string pdfPath,
            string printerName,
            string paperName,
            bool isColor,
            int startPage,
            int endPage,
            bool landScape,
            int numberOfCopies,
            int JobId,
            string fileExtension)
        {
            int counter = 0;
            ProcessingWindow processingWindow = null;
            bool isPrinted = false;
            try
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    processingWindow = new ProcessingWindow();
                    processingWindow.Show();
                });

                var printerSettings = new PrinterSettings
                {
                    PrinterName = printerName,
                    Copies = (short)numberOfCopies,
                };
                var pageSettings = new PageSettings(printerSettings)
                {
                    Color = isColor,
                    Margins = new Margins(0, 0, 0, 0),
                    Landscape = landScape,
                };

                foreach (PaperSize papersize in printerSettings.PaperSizes)
                {
                    if (papersize.PaperName == paperName)
                    {
                        pageSettings.PaperSize = papersize;
                        break;
                    }
                }

                using (var document = PdfDocument.Load(pdfPath))
                {
                    using (var printDocument = document.CreatePrintDocument())
                    {
                        printDocument.PrinterSettings = printerSettings;
                        printDocument.DefaultPageSettings = pageSettings;
                        printDocument.OriginAtMargins = true;
                        printDocument.PrinterSettings.FromPage = startPage;
                        printDocument.PrinterSettings.ToPage = endPage;
                        printDocument.PrintPage += (sender, e) => counter++;
                        printDocument.Print();
                    }
                }

                String Status = counter == 0 ? "failed" : "printed";
                String Message = counter == 0 ? "Opps! something went wrong could not process your print" : "Document printed succesfully";
                await AccountManager.updatePrintJob(JobId, Status, Message, counter, fileExtension);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (processingWindow != null)
                        processingWindow.Close();
                });

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (counter == 0)
                    {
                        PrintFailureWindow printFailureWindow = new PrintFailureWindow(JobId, Status, Message, counter, fileExtension);
                        printFailureWindow.Show();
                    }
                    else
                    {
                        PrintSuccessWindow printSuccessWindow = new PrintSuccessWindow(JobId, Status, Message, counter, fileExtension);
                        printSuccessWindow.Show();
                    }
                });
                return true;
            }
            catch (Exception ex)
            {

                String Status = "failed";
                String Message = $"Error printing your document: {ex.Message}";
                await AccountManager.updatePrintJob(JobId, Status, Message, counter, fileExtension);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (processingWindow != null)
                        processingWindow.Close();
                });
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    PrintFailureWindow printFailureWindow = new PrintFailureWindow(JobId, Status, Message, counter, fileExtension);
                    printFailureWindow.Show();
                });
                return false;
            }
        }
        //public PaperSizeType GetPaperType(string paperName)
        //{
        //    PaperSizeType result;
        //    switch (paperName.ToLower())
        //    {
        //        case "a3":
        //            result = PaperSizeType.PaperA3;
        //            break;
        //        case "a4":
        //            result = PaperSizeType.PaperA4;
        //            break;
        //        case "letter":
        //            result = PaperSizeType.PaperLetter;
        //            break;
        //        case "legal":
        //            result = PaperSizeType.PaperLegal;
        //            break;
        //        default:
        //            result = PaperSizeType.PaperLetter;
        //            break;
        //    }
        //    return result;
        //}

        //public async Task<bool> PrintWordDocument(
        //   string documentPath,
        //   string printerName,
        //   string paperName,
        //   bool isColor,
        //   int startPage,
        //   int endPage,
        //   bool landscape,
        //   int numberOfCopies,
        //   int jobId,
        //   string fileExtension)
        //{
        //    bool printStatus = false;
        //    int counter = 0;
        //    Application wordApp = null;
        //    Document document = null;

        //    try
        //    {
        //        wordApp = new Application();
        //        object missing = System.Reflection.Missing.Value;
        //        document = wordApp.Documents.Open(documentPath, ReadOnly: true, Visible: false);
        //        document.Activate();
        //        wordApp.ActivePrinter = printerName;
        //        wordApp.Options.PrintBackground = isColor;
        //        wordApp.Options.PrintBackgrounds = isColor;

        //        counter = document.ComputeStatistics(WdStatistic.wdStatisticPages);

        //        foreach (Section section in document.Sections)
        //        {
        //            PageSetup pageSetup = section.PageSetup;
        //            pageSetup.Orientation = landscape ? WdOrientation.wdOrientLandscape : WdOrientation.wdOrientPortrait;

        //            if (pageSetup.PaperSize != GetWordPaperSize(paperName))
        //            {
        //                try
        //                {
        //                    pageSetup.PaperSize = GetWordPaperSize(paperName);
        //                }
        //                catch (ArgumentException)
        //                {
        //                    Console.WriteLine($"Paper size '{paperName}' not found. Using default paper size.");
        //                }
        //            }
        //        }

        //        object range = WdPrintOutRange.wdPrintRangeOfPages;
        //        object fromPage = startPage;
        //        object toPage = endPage;


        //        object ranges = WdPrintOutRange.wdPrintFromTo;
        //        document.PrintOut(
        //            Copies: numberOfCopies,
        //            Background: true,
        //            Range: ranges,
        //            From: fromPage,
        //            To: toPage);

        //        document.Close(SaveChanges: false);
        //        wordApp.Quit();
        //        Marshal.ReleaseComObject(document);
        //        Marshal.ReleaseComObject(wordApp);
        //        printStatus = true;

        //        string status = "printed";
        //        string message = "Document printed successfully";
        //        await AccountManager.updatePrintJob(jobId, status, message, counter, fileExtension);
        //    }
        //    catch (Exception ex)
        //    {
        //        string status = "failed";
        //        string message = $"Error printing your document: {ex.Message}";
        //        await AccountManager.updatePrintJob(jobId, status, message, counter, fileExtension);
        //        printStatus = false;

        //        if (document != null)
        //        {
        //            document.Close(SaveChanges: false);
        //            Marshal.ReleaseComObject(document);
        //        }

        //        if (wordApp != null)
        //        {
        //            wordApp.Quit();
        //            Marshal.ReleaseComObject(wordApp);
        //        }
        //    }

        //    return printStatus;
        //}

        private static string RemoveSpecialCharacters(string str)
        {
            string pattern = @"[^\w.%]+";
            string cleanedString = Regex.Replace(str, pattern, "_");
            return cleanedString;
        }

        private static string GenerateUniqueFileName(string fileUrl)
        {
            string fileName = Path.GetFileName(fileUrl);
            string cleanedFileName = RemoveSpecialCharacters(fileName);
            string uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{cleanedFileName}";
            return uniqueFileName;
        }

        private static async Task<string> DownloadFileAsync(string fileUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(fileUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string fileName = GenerateUniqueFileName(fileUrl);
                        string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

                        using (FileStream fileStream = File.Create(tempFilePath))
                        {
                            await response.Content.CopyToAsync(fileStream);
                            return tempFilePath;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error: Failed to download the file. Status code: {response.StatusCode}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file: {ex.Message}");
                return null;
            }
        }
    }
}
