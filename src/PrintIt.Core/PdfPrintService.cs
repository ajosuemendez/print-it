using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
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

        public PdfPrintService(ILogger<PdfPrintService> logger)
        {
            _logger = logger;
        }

        public void Print(Stream pdfStream, string printerName, string pageRange = null, int numberOfCopies = 1, string paperSource = null, string paperSize = null, bool isColor = false, bool isLandscape = false)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            PdfDocument document = PdfDocument.Open(pdfStream);

            _logger.LogInformation($"Printing PDF containing {document.PageCount} page(s) to printer '{printerName}'");

            using var printDocument = new PrintDocument();
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

            PrintState state = PrintStateFactory.Create(document, pageRange);
            printDocument.PrintPage += (_, e) => PrintDocumentOnPrintPage(e, state);
            printDocument.QueryPageSettings += (_, e) => MyPrintQueryPageSettingsEvent(e, chosenSource);
            printDocument.Print();
            _logger.LogInformation($"Printing Document Page Settings: {printDocument.DefaultPageSettings}");
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
    }

    public interface IPdfPrintService
    {
        void Print(Stream pdfStream, string printerName, string pageRange = null, int numberOfCopies = 1, string paperSource = null, string paperSize = null, bool isColor = false, bool isLandscape = false);

        void PrintSimpleText(string printerName);

        void PrintZPL(string printerName, string file);

        void PrintZPLFile(string printerName, Stream fileStream);
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
