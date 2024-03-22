namespace Printit.Core.Tests;
using Xunit;
using Moq;
using PrintIt.Core;
using PrintIt.ServiceHost.Controllers;
using Microsoft.AspNetCore.Mvc;


public class PrintControllerTests
{
    private Mock<IPdfPrintService> _pdfPrintServiceMock;
    private PrintController _printController;

    public PrintControllerTests()
    {
        _pdfPrintServiceMock = new Mock<IPdfPrintService>();
        _printController = new PrintController(_pdfPrintServiceMock.Object);
    }

    [Fact]
    public void TestPrintSimpleTextPipe()
    {
        // Arrange
        var request = new PrintSimpleTextTemplateRequest
        {
            PrinterPath = "Brother Color Leg Type1 Class Driver"
        };

        // Act
        var result = _printController.PrintSimpleTextPipe(request);
        
        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        _pdfPrintServiceMock.Verify(x => x.PrintSimpleText(request.PrinterPath), Times.Once);
    }
}