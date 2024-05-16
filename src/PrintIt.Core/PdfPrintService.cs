using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Printing;

// using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using PrintIt.Core.Internal;
using PrintIt.Core.Pdfium;
using RawPrint.NetStd;

namespace PrintIt.Core
{
    [ExcludeFromCodeCoverage]
    internal sealed class PdfPrintService : IPdfPrintService
    {
        private readonly ILogger<PdfPrintService> _logger;

        private readonly Dictionary<int, JobStatus> _jobStatuses = new Dictionary<int, JobStatus>();

        public PdfPrintService(ILogger<PdfPrintService> logger)
        {
            _logger = logger;
        }

        public int Print(Stream pdfStream, string printerName, string documentName = "file.pdf", string pageRange = null, int numberOfCopies = 1, string paperSource = null, string paperSize = null, bool isColor = false, bool isLandscape = false, bool printToFile = false)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            PdfDocument document = PdfDocument.Open(pdfStream);

            _logger.LogInformation($"Printing PDF containing {document.PageCount} page(s) to printer '{printerName}'");

            CustomPrintController customPrintController = new CustomPrintController();
            using var printDocument = new CustomPrintDocument();
            printDocument.PrintController = customPrintController;

            // Append a unique identifier to the documentName
            string uniqueNameId = DateTime.Now.ToString("yyyyMMddHHmmss");
            printDocument.DocumentName = $"{documentName}_{uniqueNameId}";

            // printDocument.DocumentName = documentName;
            printDocument.PrinterSettings.PrinterName = printerName;
            printDocument.PrinterSettings.Copies = (short)Math.Clamp(numberOfCopies, 1, short.MaxValue);
            printDocument.DefaultPageSettings.Color = false; // Set the page default's to not print in color
            printDocument.DefaultPageSettings.Landscape = isLandscape;
            PaperSource chosenSource = null;
            if (paperSource != null)
            {
                foreach (PaperSource source in printDocument.PrinterSettings.PaperSources)
                {
                    if (source != null && source.SourceName == paperSource)
                    {
                        printDocument.PrinterSettings.DefaultPageSettings.PaperSource = source;
                        chosenSource = source;
                        break;
                    }
                }

                if (chosenSource == null)
                {
                    throw new SelectPaperSourceException(paperSource, printerName);
                }
            }

            if (paperSize != null)
            {
                bool paperSizeSet = false;
                foreach (PaperSize size in printDocument.PrinterSettings.PaperSizes)
                {
                    if (size != null && size.PaperName == paperSize)
                    {
                        printDocument.DefaultPageSettings.PaperSize = size;
                        paperSizeSet = true;
                        break;
                    }
                }

                if (!paperSizeSet)
                {
                    throw new SelectPaperSizeException(paperSize, printerName);
                }
            }

            if (isColor)
            {
                // check if printer supports color
                if (printDocument.PrinterSettings.SupportsColor)
                {
                    printDocument.DefaultPageSettings.Color = true;
                }
            }

            _logger.LogInformation($"Printing Document in Color ? : {printDocument.DefaultPageSettings.Color}");

            if (printToFile)
            {
                string uniqueId = DateTime.Now.ToString("yyyyMMddHHmmss");
                string newDocumentName = $"{documentName}_{uniqueId}.pdf";
                printDocument.PrinterSettings.PrintFileName = $"C:/Users/AlejandroLopez/Documents/VirtualPrintFiles/{newDocumentName}";
                printDocument.PrinterSettings.PrintToFile = true; // Skip the save file dialog
            }

            PrintState state = PrintStateFactory.Create(document, pageRange);
            printDocument.PrintPage += (_, e) => PrintDocumentOnPrintPage(e, state);
            printDocument.QueryPageSettings += (_, e) => MyPrintQueryPageSettingsEvent(e, chosenSource);

            printDocument.Print();
            int jobId = -1;

            jobId = ((CustomPrintController)printDocument.PrintController).JobId;
            _logger.LogInformation($"Printing Document Page Settings: {printDocument.DefaultPageSettings}");

            // _logger.LogInformation("The job ID of the current print job is: " + printDocument.JobId);

            // jobId = printDocument.JobId;

            // Get the print queue
            // LocalPrintServer printServer = new LocalPrintServer();
            // PrintQueue printQueue = printServer.GetPrintQueue(printerName);

            // // Get the job ID from the Queue given the unique document name
            // PrintJobInfoCollection jobs = printQueue.GetPrintJobInfoCollection();
            // foreach (PrintSystemJobInfo job in jobs)
            // {s
            //     if (job.Name == $"{documentName}_{uniqueNameId}")
            //     {
            //         jobId = job.JobIdentifier;
            //         _logger.LogInformation($"Job ID: {jobId}");
            //         break;
            //     }
            // }
            return jobId;
        }

        public void PrintZPLFile(string printerName, Stream fileStream)
        {
            IPrinter printer = new Printer();

            // Print the file
            try
            {
                printer.PrintRawStream(printerName, fileStream, "zpl-test-1");
                _logger.LogInformation($"All Good Printing");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error printing ZPL: {ex.Message}");
            }
        }

        public void PrintZPL(string printerName, string file)
        {
            // string zplCommandsFilePath = @"C:\zpl-test.txt";
            // System.IO.File.Copy(zplCommandsFilePath, printerName);
            IPrinter printer = new Printer();

            // Specify the file path and filename (e.g., ZPL file)
            string filePath = @"C:\zpl-test.txt";

            // Print the file
            try
            {
                printer.PrintRawFile(printerName, filePath, "zpl-test.txt");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error printing ZPL: {ex.Message}");
            }

            // string zplCode = "^XA^FO50,50^A0N,50,50^FDHello, Zebra!^FS^XZ";

            // byte[] zplBytes = System.Text.Encoding.UTF8.GetBytes(zplCode);

            // try
            // {
            //     using (var printer = new RawPrinterHelper("YourPrinterName"))
            //     {
            //         printer.SendBytesToPrinter(zplBytes);
            //     }
            // }
            // catch (Exception ex)
            // {
            //     // Handle any exceptions (printer not found, etc.)
            //     _logger.LogInformation($"Error printing ZPL: {ex.Message}");
            // }
        }

        public void PrintSimpleText(string printerName)
        {
            PrintDocument printDoc = new PrintDocument();

            // Set the printer name
            printDoc.PrinterSettings.PrinterName = printerName;

            // Set the PrintPage event handler. This is where you specify what to print.
            printDoc.PrintPage += new PrintPageEventHandler(PrintSimpleTextPage);

            // Print the document.
            printDoc.Print();

            _logger.LogInformation($"Printing Simple Text Page Settings: {printDoc.DefaultPageSettings}");
        }

        private void PrintSimpleTextPage(object sender, PrintPageEventArgs e)
        {
             // Draw a simple text string on the page
            e.Graphics.DrawString("Hello, world!", new Font("Arial", 12), Brushes.Black, 0, 0);
        }

        private void PrintDocumentOnPrintPage(PrintPageEventArgs e, PrintState state)
        {
            var destinationRect = new RectangleF(
                x: e.Graphics.VisibleClipBounds.X * e.Graphics.DpiX / 100.0f,
                y: e.Graphics.VisibleClipBounds.Y * e.Graphics.DpiY / 100.0f,
                width: e.Graphics.VisibleClipBounds.Width * e.Graphics.DpiX / 100.0f,
                height: e.Graphics.VisibleClipBounds.Height * e.Graphics.DpiY / 100.0f);
            using PdfPage page = state.Document.OpenPage(state.CurrentPageIndex);
            page.RenderTo(e.Graphics, destinationRect);
            e.HasMorePages = state.AdvanceToNextPage();
        }

        private void MyPrintQueryPageSettingsEvent(QueryPageSettingsEventArgs e, PaperSource paperSource)
        {
            if (paperSource != null)
            {
                e.PageSettings.PaperSource = paperSource;
            }
        }

        public Dictionary<int, JobStatus> GetQueueInfo(string printerName)
        {
            LocalPrintServer printServer = new LocalPrintServer();

            try
            {
                PrintQueue printQueue = new PrintQueue(printServer, printerName);

                var printJobs = printQueue.GetPrintJobInfoCollection();
                foreach (PrintSystemJobInfo printJob in printJobs)
                {
                    _jobStatuses[printJob.JobIdentifier] = new JobStatus { JobId = printJob.JobIdentifier, Status = printJob.JobStatus, NumberOfPagesPrinted = printJob.NumberOfPagesPrinted, NumberOfPages = printJob.NumberOfPages, Progress = printJob.NumberOfPagesPrinted / printJob.NumberOfPages };

                    _logger.LogInformation("Job ID: " + printJob.JobIdentifier);
                    _logger.LogInformation("Job Status: " + printJob.JobStatus);
                    _logger.LogInformation("Number of Pages Printed: " + printJob.NumberOfPagesPrinted);
                    _logger.LogInformation("Total Pages: " + printJob.NumberOfPages);
                    _logger.LogInformation("Progress: " + (printJob.NumberOfPagesPrinted / printJob.NumberOfPages));
                }
            }
            catch (PrintQueueException ex)
            {
                _logger.LogInformation($"Failed to get print queue for printer '{printerName}': {ex.Message}");
            }

            return _jobStatuses;
        }

        public JobStatus GetJobInfo(string printerName, int jobId)
        {
            LocalPrintServer printServer = new LocalPrintServer();
            JobStatus jobInfoObj = new JobStatus();
            try
            {
                PrintQueue printQueue = new PrintQueue(printServer, printerName);
                var printJobs = printQueue.GetPrintJobInfoCollection();

                foreach (PrintSystemJobInfo jobInfo in printJobs)
                {
                    if (jobInfo.JobIdentifier == jobId)
                    {
                        // jobInfo now contains the information of the print job with the ID you're looking for
                        // You can access its properties here, for example:
                        _logger.LogInformation($"Job name: {jobInfo.Name}");
                        _logger.LogInformation($"Job status: {jobInfo.JobStatus}");

                        // Get the number of pages printed and remaining
                        int pagesPrinted = jobInfo.NumberOfPagesPrinted;
                        int totalPages = jobInfo.NumberOfPages;
                        int pagesLeft = totalPages - pagesPrinted;

                        _logger.LogInformation($"Pages printed: {pagesPrinted}");
                        _logger.LogInformation($"Total pages: {totalPages}");
                        _logger.LogInformation($"Pages left: {pagesLeft}");

                        jobInfoObj.JobId = jobInfo.JobIdentifier;
                        jobInfoObj.Status = jobInfo.JobStatus;
                        jobInfoObj.NumberOfPagesPrinted = jobInfo.NumberOfPagesPrinted;
                        jobInfoObj.NumberOfPages = jobInfo.NumberOfPages;
                        jobInfoObj.Progress = jobInfo.NumberOfPages != 0 ? (double)jobInfo.NumberOfPagesPrinted / jobInfo.NumberOfPages : 0;
                        break;
                    }
                }
            }
            catch (PrintQueueException ex)
            {
                _logger.LogInformation($"Failed to get print queue for printer '{printerName}': {ex.Message}");
                throw;
            }

            return jobInfoObj;
        }
    }

    public interface IPdfPrintService
    {
        int Print(Stream pdfStream, string printerName, string documentName, string pageRange = null, int numberOfCopies = 1, string paperSource = null, string paperSize = null, bool isColor = false, bool isLandscape = false, bool printToFile = false);

        void PrintSimpleText(string printerName);

        void PrintZPL(string printerName, string file);

        void PrintZPLFile(string printerName, Stream fileStream);

        Dictionary<int, JobStatus> GetQueueInfo(string printerName);

        JobStatus GetJobInfo(string printerName, int jobId);
    }

    public sealed class SelectPaperSizeException : Exception
    {
        public SelectPaperSizeException(string paperSize, string printerPath)
            : base($"PaperSize: {paperSize} was not valid for printerName: {printerPath}")
        {
            PrinterPath = printerPath;
            PaperSize = paperSize;
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Public API")]
        public string PrinterPath { get; }

        public string PaperSize { get; }
    }

    public sealed class SelectPaperSourceException : Exception
    {
        public SelectPaperSourceException(string paperSource, string printerPath)
            : base($"PaperSource: {paperSource} was not valid for PrinterName: {printerPath}")
        {
            PrinterPath = printerPath;
            PaperSource = paperSource;
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Public API")]
        public string PrinterPath { get; }

        public string PaperSource { get; }
    }
}
