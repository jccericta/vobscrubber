#r @"bin\Debug\net7.0\FSharp.Core.dll"
#r @"bin\Debug\net7.0\PdfSharp.dll"
#r @"bin\Debug\net7.0\PdfSharp.Charting.dll"
#r @"bin\Debug\net7.0\FSharp.Data.dll"
#r @"bin\Debug\net7.0\itext.forms.dll"
#r @"bin\Debug\net7.0\itext.kernel.dll"
#r @"bin\Debug\net7.0\itext.io.dll"
#r @"bin\Debug\net7.0\itext.commons.dll"

open System.IO
open iText.Forms
open iText.Kernel

let fTemplateCurrDir = System.IO.Directory.GetCurrentDirectory()
let fTemplatePath = fTemplateCurrDir + @"\pdfs\Test.pdf"
let iTempReader = iText.Kernel.Pdf.PdfReader(fTemplatePath)
let iTempDoc = Pdf.PdfDocument(iTempReader)

let fCopyCurrDir = System.IO.Directory.GetCurrentDirectory()
let fCopyPath = fCopyCurrDir + @"\pdfs\Test copy.pdf"
let fTempPath = fCopyCurrDir + @"\pdfs\Test temp.pdf"
let iTextCopyReader = iText.Kernel.Pdf.PdfReader(fCopyPath)
let iTextTempWriter = iText.Kernel.Pdf.PdfWriter(fTempPath)
let iTextCRDoc = Pdf.PdfDocument(iTextCopyReader)
let iTextTWDoc = Pdf.PdfDocument(iTextTempWriter)

iTextTWDoc.AddPage(iTextCRDoc.GetFirstPage().CopyTo(iTextTWDoc))
iTextTWDoc.AddPage(iTextCRDoc.GetLastPage().CopyTo(iTextTWDoc))

iTextCopyReader.Close()
iTextCRDoc.Close()

let pdfFormCopier = PdfPageFormCopier()
pdfFormCopier.Copy(iTempDoc.GetFirstPage(), iTextTWDoc.GetFirstPage())

iTempReader.Close()
iTempDoc.Close()
iTextTWDoc.Close()
iTextCRDoc.Close()