using System.Drawing.Printing;
using System.Linq; // Required for LINQ methods
using System.Printing;

public class CustomPrintDocument : PrintDocument
{
    public int JobId { get; private set; }

    protected override void OnEndPrint(PrintEventArgs e)
    {
        base.OnEndPrint(e);

        LocalPrintServer printServer = new LocalPrintServer();

        PrintQueue printQueue = printServer.DefaultPrintQueue;

        PrintJobInfoCollection printJobs = printQueue.GetPrintJobInfoCollection();

        var printJobList = printJobs.Cast<PrintSystemJobInfo>().ToList();

        // Get the id of the last print job (assuming it's the current one)
        if (printJobList.Count > 0)
        {
            JobId = printJobList.Last().JobIdentifier;
        }
    }
}
