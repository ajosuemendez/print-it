using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrintIt.Core;

namespace PrintIt.ServiceHost.Controllers
{
    [ApiController]
    [Route("print")]
    public class PrintController : ControllerBase
    {
        private readonly IPdfPrintService _pdfPrintService;

        public PrintController(IPdfPrintService pdfPrintService)
        {
            _pdfPrintService = pdfPrintService;
        }

        [HttpPost]
        [Route("from-pdf")]
        public async Task<IActionResult> PrintFromPdf([FromForm] PrintFromTemplateRequest request)
        {
            await using Stream pdfStream = request.PdfFile.OpenReadStream();
            _pdfPrintService.Print(pdfStream,
                printerName: request.PrinterPath,
                pageRange: request.PageRange,
                numberOfCopies: request.Copies ?? 1,
                paperSource: request.PaperSource, 
                paperSize: request.PaperSize,
                isColor: request.IsColor,
                isLandscape: request.IsLandscape);
            return Ok();
        }

        [HttpPost]
        [Route("pipe")]
        public async Task<IActionResult> PrintZPLPipe([FromForm] PrintZPLTemplateRequest request)
        {
            await using Stream streamFile = request.File.OpenReadStream();
            _pdfPrintService.PrintZPLFile(request.PrinterPath, streamFile); 
            // _pdfPrintService.PrintZPL(request.PrinterPath, request.File); 
            return Ok();
        }
        // public IActionResult PrintSimpleTextPipe([FromForm] PrintSimpleTextTemplateRequest request)
        // {
        //     _pdfPrintService.PrintSimpleText(request.PrinterPath);   
        //     return Ok();
        // }
    }

    public sealed class PrintFromTemplateRequest
    {
        [Required]
        public IFormFile PdfFile { get; set; }

        [Required]
        public string PrinterPath { get; set; }

        public string PageRange { get; set; }

        public int? Copies { get; set; }

        public string PaperSource { get; set; }

        public string PaperSize { get; set; }

        public bool IsColor { get; set; }

        public bool IsLandscape { get; set; }
    }

    public sealed class PrintSimpleTextTemplateRequest
    {
        [Required]
        public string PrinterPath { get; set; }
    }

    public sealed class PrintZPLTemplateRequest
    {
        [Required]
        public string PrinterPath { get; set; }
        [Required]
        public IFormFile File { get; set; }
    }
}
