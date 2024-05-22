## Introduction

This document provides detailed information about the Printing Windows Service. Currently this service allows users to print documents via HTTP Request on a specified printer.

## Requirements
- VS Code
- VS Code plugin C# Dev Kit

## Usage instructions using VS Code

1. Clone this repository
2. We are currently using the pdfium-v8-win-x64 binaries. If you want you can download and add the latest version of the PDFium binaries: https://github.com/bblanchon/pdfium-binaries (Make sure to use PDFium V8)
3. If you decide to update the binaries make sure that the csproject is properly pointing to those. You would have to modify path where the binary is found in /src/PrintIt.ServiceHost/PrintIt.ServiceHost.csproj.
```xml
<Content Include="..\..\pdfium-v8-win-x64\binaries\pdfium.dll">
```
4. Create a .env file in the /src/PrintIt.ServiceHost/ folder with the following values:

  ```xml
PORT=7000
HOST=http://localhost:
LOGGING__LOGLEVEL__DEFAULT=Debug
LOGGING__LOGLEVEL__SYSTEM=Information
LOGGING__LOGLEVEL__MICROSOFT=Information
```

5. Open the Programm.cs in PrintIt.Servicehost.csproj and then click on the play button in the upper right side (You must have installed the VS Code plugin C# Dev Kit).

6. Wait a few seconds until you see that the application is listening on port [7000](http://localhost:7000/).

## Test API with SWAGGER

You can have access to te swagger UI in http://localhost:7000/doc

We currently have 3 controllers: Info, Print and Printers

### INFO Endpoint
#### [POST] /info/statusqueue
Requires: Printer Path

Returns: All the jobs that are currently in the queue for the corresponding printer

#### [POST] /info/statusjob
Requires: Printer Path and a Job Id

Returns: Information about the specific job (how many pages to be printed and how many pages are already printed for example)

### PRINTERS Endpoints
#### [GET] /printers/list

List all available printers on the system.

#### [GET] /printers/paperSources?printerPath=\\\\REMOTE_PC_NAME\\PRINTER-NAME

List all paper sources(trays) on the printer with the UNC-path `\\REMOTE_PC_NAME\PRINTER-NAME`.

#### [GET] /printers/paperSizes?printerPath=\\\\REMOTE_PC_NAME\\PRINTER-NAME

List all paper sizes on the printer with the UNC-path `\\REMOTE_PC_NAME\PRINTER-NAME`.

#### [POST] /printers/install?printerPath=\\\\REMOTE_PC_NAME\\PRINTER-NAME

Install the network printer with the UNC-path `\\REMOTE_PC_NAME\PRINTER-NAME`. 

### PRINT Endpoints
#### [POST] /print/from-pdf

To print a PDF on a given printer, post a multipart form to this end-point with the following fields:

Field Name     | Required           | Content
------------   | ------------------ | ---------
PdfFile        | :heavy_check_mark: | The PDF file to print (Content-type: application/pdf)
PrinterPath    | :heavy_check_mark: | The UNC-path of the printer to send the PDF to
DocumentName   |                    | An optional Name for the job to be printed
PageRange      |                    | An optional page range string (f.e. "1-5", "1, 3", "1, 4-8", "2-", "-5")
Copies         |                    | An optional number of copies (defaults to 1)
PaperSource    |                    | An optional name of the page source. See GET for valid page sources
PaperSize      |                    | An optional name of the page size. See GET for valid page sizes
IsColor        |                    | If true then print in Color (default false)
IsLandscape    |                    | If true then print in Landscape mode (default false)

#### [POST] /print/pipe

You can also Print from a Raw stream file.

Field Name     | Required           | Content
------------   | ------------------ | ---------
File           | :heavy_check_mark: | File to print
PrinterPath    | :heavy_check_mark: | The UNC-path of the printer to send the PDF to

## PDFium

This project uses the [PDFium library](https://pdfium.googlesource.com/) for rendering the PDF file which is licensed under Apache 2.0, see [LICENSE](pdfium-binary/LICENSE).

