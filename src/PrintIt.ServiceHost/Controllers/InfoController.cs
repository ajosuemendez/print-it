using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PrintIt.Core;
using System.Linq;

namespace PrintIt.ServiceHost.Controllers
{
    [ApiController]
    [Route("info")]
    public class InfoController : ControllerBase
    {
        private readonly IPdfPrintService _pdfPrintService;

        public InfoController(IPdfPrintService pdfPrintService)
        {
            _pdfPrintService = pdfPrintService;
        }

        [HttpPost]
        [Route("statusqueue")]
        public ActionResult<List<JobStatus>> QueueInfo([FromForm] QueueStatusRequest request)
        {
            Dictionary<int, JobStatus> queueInfo = _pdfPrintService.GetQueueInfo(request.PrinterPath);
            // by default dotnet is not allowing object serialization for sending Dictionaries (or JSON)
            // That's why I am transforming it to a List.
            // TODO later: Find a package to allow sending JSON Objects.
            List<JobStatus> list = queueInfo.Values.ToList();
            return list;
        }

        [HttpPost]
        [Route("statusjob")]
        public ActionResult<JobStatus> JobInfo([FromForm] JobStatusRequest request)
        {
            return _pdfPrintService.GetJobInfo(request.PrinterPath, request.JobId);
        }
    }

    public sealed class JobStatusRequest
    {
        [Required]
        public string PrinterPath { get; set; }
        [Required]
        public int JobId { get; set; }
    }
    public sealed class QueueStatusRequest
    {
        [Required]
        public string PrinterPath { get; set; }
    }
}
