## Introduction

This document provides detailed information about the Printing Windows Service. Currently this service allows users to print documents via HTTP Request on a specified printer.

## Requirements
You need to have installed .NetSDK 8. Alternatively, you can use the VS Code plugin C# Dev Kit

## Usage instructions using VS Code

1. Clone this repository
2. We are currently using the pdfium-v8-win-x64 binaries. If want you can download and add the latest version of the PDFium binaries: https://github.com/bblanchon/pdfium-binaries
3. If you decide to update the binaries make sure that the csproject is properly pointing to those. You would have to modify path where the binary is found in PrintIt.ServiceHost.csproj.
```xml
<Content Include="..\..\pdfium-v8-win-x64\bin\pdfium.dll">
```
4. Create a .env file in the PrintIt.Servicehost folder with the following values:

  ```xml
PORT=7000
HOST=http://localhost:
LOGGING__LOGLEVEL__DEFAULT=Debug
LOGGING__LOGLEVEL__SYSTEM=Information
LOGGING__LOGLEVEL__MICROSOFT=Information
```

5. If you have downloaded the VS code extension then you can start the service by clicking the play button in VS Code. Make sure to first open the Programm.cs in PrintIt.Servicehost.csproj and then click play.

   
#### [GET] /printers/list

List all available printers on the system.

#### [GET] /printers/paperSources?printerPath=\\\\REMOTE_PC_NAME\\PRINTER-NAME

List all paper sources(trays) on the printer with the UNC-path `\\REMOTE_PC_NAME\PRINTER-NAME`.

#### [GET] /printers/paperSizes?printerPath=\\\\REMOTE_PC_NAME\\PRINTER-NAME

List all paper sizes on the printer with the UNC-path `\\REMOTE_PC_NAME\PRINTER-NAME`.

#### [POST] /printers/install?printerPath=\\\\REMOTE_PC_NAME\\PRINTER-NAME

Install the network printer with the UNC-path `\\REMOTE_PC_NAME\PRINTER-NAME`. 

#### [POST] /print/from-pdf

To print a PDF on a given printer, post a multipart form to this end-point with the following fields:

Field Name   | Required           | Content
------------ | ------------------ | ---------
PdfFile      | :heavy_check_mark: | The PDF file to print (Content-type: application/pdf)
PrinterPath  | :heavy_check_mark: | The UNC-path of the printer to send the PDF to
PageRange    |                    | An optional page range string (f.e. "1-5", "1, 3", "1, 4-8", "2-", "-5")
Copies       |                    | An optional number of copies (defaults to 1)
PaperSource  |                    | An optional name of the page source. See GET for valid page sources
PaperSize    |                    | An optional name of the page size. See GET for valid page sizes

## PDFium

This project uses the [PDFium library](https://pdfium.googlesource.com/) for rendering the PDF file which is licensed under Apache 2.0, see [LICENSE](pdfium-binary/LICENSE).

The version included in this repository under the folder `pdfium-binary` was taken from https://github.com/bblanchon/pdfium-binaries/releases/tag/chromium/4194.

