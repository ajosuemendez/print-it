using System;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

public class CustomPrintController : StandardPrintController
{
    public int JobId { get; set; }

    [DllImport("gdi32.dll", EntryPoint = "StartDocA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern int StartDoc(IntPtr hdc, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA lpdi);

    private void CheckSecurity(PrintDocument document)
    {
        PrintingPermission printPermission = document.PrinterSettings.IsDefaultPrinter
            ? new PrintingPermission(PrintingPermissionLevel.DefaultPrinting)
            : new PrintingPermission(PrintingPermissionLevel.AllPrinting);

        printPermission.Demand();
    }

    public override void OnStartPrint(PrintDocument document, PrintEventArgs e)
    {
        base.OnStartPrint(document, e);
        CheckSecurity(document);
        System.Drawing.Graphics graphics = document.PrinterSettings.CreateMeasurementGraphics();

        IntPtr dcHandle = graphics.GetHdc();

        if (!document.PrinterSettings.IsValid)
        {
            throw new InvalidPrinterException(document.PrinterSettings);
        }

        DOCINFOA docInfo = new DOCINFOA()
        {
            lpszDocName = document.DocumentName,
            cbSize = Marshal.SizeOf(typeof(DOCINFOA)),
            lpszOutput = null,
            lpszDatatype = null,
            fwType = 0,
        };

        JobId = StartDoc(dcHandle, docInfo);
        if (JobId < 0)
        {
            int error = Marshal.GetLastWin32Error();
            if (error != 0x4c7)
            {
                throw new Win32Exception(error);
            }

            e.Cancel = true;
        }
    }
}