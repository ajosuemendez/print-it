using System.Printing;

public class JobStatus
{
    public int JobId { get; set; }

    public PrintJobStatus Status { get; set; }

    public int NumberOfPagesPrinted { get; set; }

    public int NumberOfPages { get; set; }

    public double Progress { get; set; }
}