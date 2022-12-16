#r @"bin\Debug\net7.0\FSharp.Core.dll"
#r @"bin\Debug\net7.0\PdfSharp.dll"
#r @"bin\Debug\net7.0\PdfSharp.Charting.dll"
#r @"bin\Debug\net7.0\itext.forms.dll"
#r @"bin\Debug\net7.0\itext.kernel.dll"
#r @"bin\Debug\net7.0\itext.io.dll"
#r @"bin\Debug\net7.0\itext.commons.dll"

open System.IO
open PdfSharp.Pdf.Content
open PdfSharp.Pdf.Content.Objects
open PdfSharp.Pdf.IO
open iText.Forms
open iText.Kernel


// the Form Template to be copied
let templateCurrDir = Directory.GetCurrentDirectory()
let templatePath = templateCurrDir + @"\Template.pdf"
let (templateReader:Pdf.PdfReader) = new Pdf.PdfReader(templatePath)
let (templateDoc:Pdf.PdfDocument) = new Pdf.PdfDocument(templateReader)

// Get the document to copy the form to and get its document id
let docCurrDir = Directory.GetCurrentDirectory()
let docSource = docCurrDir + @"\pdfs\Kyle Mobley The Edge VOB 02.16.2022.ocr.pdf" // will change later
let docPath = PdfReader.Open(docSource, PdfDocumentOpenMode.InformationOnly).FullPath
let doc' = PdfReader.Open(docPath, PdfDocumentOpenMode.InformationOnly)

// grab the document's id number or create one
let guid (d:PdfSharp.Pdf.PdfDocument) = 
   let guid'= d.Guid
   if guid'.ToString() = "" then
      new System.Guid()
   else  
      guid'

let docGuid = guid doc'
doc'.Close()

let (docSourceReader:Pdf.PdfReader) = new Pdf.PdfReader(docSource)
let (sourceDoc:Pdf.PdfDocument) = new Pdf.PdfDocument(docSourceReader)

// Create a file that we're going to write the original contents of the document along with the template added to it
let docDest = docCurrDir + @"\pdfs\Kyle Mobley The Edge VOB 02.16.2022" + "." + docGuid.ToString() + ".pdf"
let (docCopyWriter:Pdf.PdfWriter) = new Pdf.PdfWriter(docDest)
let (copyDoc: Pdf.PdfDocument) = new Pdf.PdfDocument(docCopyWriter)

sourceDoc.CopyPagesTo(1,2, copyDoc)
sourceDoc.Close()

let pdfFormCopier = new PdfPageFormCopier()

templateDoc.GetFirstPage().CopyAsFormXObject(copyDoc)
templateDoc.GetLastPage().CopyAsFormXObject(copyDoc)

pdfFormCopier.Copy(templateDoc.GetFirstPage(), copyDoc.GetFirstPage())
pdfFormCopier.Copy(templateDoc.GetLastPage(), copyDoc.GetLastPage())

templateDoc.Close()
copyDoc.Close()

