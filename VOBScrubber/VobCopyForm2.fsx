#r @"bin\Debug\net7.0\FSharp.Core.dll"
#r @"bin\Debug\net7.0\PdfSharp.dll"
#r @"bin\Debug\net7.0\PdfSharp.Charting.dll"
#r @"bin\Debug\net7.0\FSharp.Data.dll"
#r @"bin\Debug\net7.0\itext.forms.dll"
#r @"bin\Debug\net7.0\itext.kernel.dll"
#r @"bin\Debug\net7.0\itext.io.dll"
#r @"bin\Debug\net7.0\itext.commons.dll"
#r @"bin\Debug\net7.0\itext.pdfa.dll"
#r @"bin\Debug\net7.0\itext.pdfxfa.dll"
#r @"bin\Debug\net7.0\itext.pdfocr.api.dll"

open System.IO
open iText
open iText.Forms
open iText.Kernel
open iText.Commons
open iText.Pdfa
open iText.Pdfocr
open PdfSharp.Pdf.IO
open Microsoft.FSharp.Collections
open System.Collections.Generic

// Get the document to copy the form to and get its document id
let docCurrDir = Directory.GetCurrentDirectory()
let docSource = docCurrDir + @"\pdfs\Kyle Mobley The Edge VOB 02.16.2022.pdf" // will change later
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

let currDir = System.IO.Directory.GetCurrentDirectory()
let vobSampleFile = currDir + @"\Template.pdf"
let vobSampleReader = new Pdf.PdfReader(vobSampleFile)
let vobSamplePDF = new Pdf.PdfDocument(vobSampleReader)
let pdfSourceFile = currDir + @"\pdfs\Kyle Mobley The Edge VOB 02.16.2022.pdf"
let pdfSourceReader = new Pdf.PdfReader(pdfSourceFile)
let pdfSource = new Pdf.PdfDocument(pdfSourceReader)
let pdfTargetFile = new Pdf.PdfWriter(currDir + @"\pdfs\Kyle Mobley The Edge VOB 02.16.2022" + "." + docGuid.ToString() + ".pdf")
let pdfTargetWriter = new Pdf.PdfWriter(pdfTargetFile)
let pdfTarget' = new Pdf.PdfDocument(pdfTargetWriter)
(*let list:IList<int> = new List<int>()
let buildList (p:Pdf.PdfDocument) (l:IList<int>) = 
    for i in 1 .. p.GetNumberOfPages() do
        l.Add(i)
    list
let iList = buildList pdfSource list
let copiedPages = pdfSource.CopyPagesTo(iList, pdfTarget)*)
let copyPages (source:Pdf.PdfDocument) (target:Pdf.PdfDocument) = 
    for i in 1 .. source.GetNumberOfPages() do
        target.AddPage(source.GetPage(i).CopyTo(target)) |> ignore
    let ref = target.CreateNextIndirectReference()
    ref
let pdfTarget = copyPages pdfSource pdfTarget'
let pdfFormCopier = new PdfPageFormCopier()
let copyPageForm (c:PdfPageFormCopier) (s:Pdf.PdfDocument) (t:Pdf.PdfDocument) = 
    for i in 1 .. s.GetNumberOfPages() do
        c.Copy(s.GetPage(i),t.GetPage(i))
    let pdfAcroForm' = PdfAcroForm.GetAcroForm(t, true)
    let acfFields = pdfAcroForm'.GetFieldsForFlattening()
    for acfField in acfFields do
        pdfAcroForm'.AddFieldAppearanceToPage(acfField, t.GetPage(t.GetPageNumber(acfField.GetPdfObject())))
    pdfAcroForm'.FlattenFields()
    let pdfXFA = pdfAcroForm'.GetXfaForm()
    let pdfXFAForm = pdfXFA.Write(pdfAcroForm')
    t.Close()
    pdfXFAForm

copyPageForm pdfFormCopier vobSamplePDF (pdfTarget.GetDocument())

vobSampleReader.Close()
vobSamplePDF.Close()
pdfSourceReader.Close()
pdfSource.Close()
pdfTargetWriter.Close()
pdfTarget.GetDocument().Close()