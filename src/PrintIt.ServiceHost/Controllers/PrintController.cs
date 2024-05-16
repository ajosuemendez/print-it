using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using System.Collections.Generic;

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrintIt.Core;
using System.Linq;

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
        public async Task<Dictionary<string, int>> PrintFromPdf([FromForm] PrintFromTemplateRequest request)
        {
            await using Stream pdfStream = request.PdfFile.OpenReadStream();
            // print returns the job to be printed after being put in the queue
            int jobId = _pdfPrintService.Print(pdfStream,
                printerName: request.PrinterPath,
                documentName: request.DocumentName,
                pageRange: request.PageRange,
                numberOfCopies: request.Copies ?? 1,
                paperSource: request.PaperSource, 
                paperSize: request.PaperSize,
                isColor: request.IsColor,
                isLandscape: request.IsLandscape);

            Dictionary<string, int> job = new Dictionary<string, int>
            {
                { "jobId", jobId }
            };

            return job;
        }

        [HttpPost]
        [Route("pipe")]
        public async Task<IActionResult> PrintZPLPipe([FromForm] PrintZPLTemplateRequest request)
        {
            await using Stream streamFile = request.File.OpenReadStream();
            _pdfPrintService.PrintZPLFile(request.PrinterPath, streamFile); 
            return Ok();
        }
    }

    public sealed class PrintFromTemplateRequest
    {
        [Required]
        public IFormFile PdfFile { get; set; }

        [Required]
        public string PrinterPath { get; set; }

        public string DocumentName { get; set; }

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

    // public sealed class JobStatusRequest
    // {
    //     [Required]
    //     public string PrinterPath { get; set; }
    //     [Required]
    //     public int JobId { get; set; }
    // }
    // public sealed class QueueStatusRequest
    // {
    //     [Required]
    //     public string PrinterPath { get; set; }
    // }
}
