#r @"bin\Debug\net7.0\FSharp.Core.dll"
#r @"bin\Debug\net7.0\PdfSharp.dll"
#r @"bin\Debug\net7.0\PdfSharp.Charting.dll"
#r @"bin\Debug\net7.0\FSharp.Data.dll"
#r @"bin\Debug\net7.0\itext.forms.dll"
#r @"bin\Debug\net7.0\itext.kernel.dll"
#r @"bin\Debug\net7.0\itext.io.dll"
#r @"bin\Debug\net7.0\itext.layout.dll"
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
open iText.Layout
open Microsoft.FSharp.Collections
open System.Collections.Generic

let currDir: string = System.IO.Directory.GetCurrentDirectory()
let pdfSourceFile = currDir + @"\pdfs\John Ericta TaxReturn 2021.pdf"
let pdfSourceReader = new Pdf.PdfReader(pdfSourceFile)
let pdfSource = new Pdf.PdfDocument(pdfSourceReader)
let pdfTargetFile = currDir + @"\pdfs\Copy.pdf"
let pdfTargetWriter = new Pdf.PdfWriter(pdfTargetFile)
let pdfTarget' = new Pdf.PdfDocument(pdfTargetWriter)
let vobSampleFile = currDir + @"\pdfs\Test.pdf"
let vobSampleReader = new Pdf.PdfReader(vobSampleFile)
let vobSamplePDF = new Pdf.PdfDocument(vobSampleReader)
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
    let rel = target.CreateNextIndirectReference()
    rel
    //target

let pdfTarget = copyPages pdfSource pdfTarget'
let pdfFormCopier = new PdfPageFormCopier()

let copyPageForm (c:PdfPageFormCopier) (s:Pdf.PdfDocument) (t:Pdf.PdfDocument) =
    let pdfAFSource: PdfAcroForm = PdfAcroForm.GetAcroForm(s, false)
    let pdfAFTarget: PdfAcroForm = PdfAcroForm.GetAcroForm(t, true)
    // Copy the template's form
    for i: int32 in 1 .. s.GetNumberOfPages() do
        c.Copy(s.GetPage(i),t.GetPage(i))
    //Get the template's form fields
    let pdfAFSourceFields = pdfAFSource.GetFormFields()
    // Copy form fields from the template and add to target
    for pdfAFSourceField in pdfAFSourceFields do
        pdfAFTarget.AddField(pdfAFSourceField.Value)
    let acfTargetFields = pdfAFTarget.GetFieldsForFlattening()
    // Make them appear on the page
    for acfTargetField in acfTargetFields do
        pdfAFTarget.AddFieldAppearanceToPage(acfTargetField, t.GetPage(t.GetPageNumber(acfTargetField.GetPdfObject())))
    //pdfAFSource.FlattenFields()
    //let pdfXFA = pdfAFTarget.GetXfaForm()
    //pdfXFA.Write(pdfAFTarget)
    // t.Close()
    t
    
let pdfTargetTemplated = copyPageForm pdfFormCopier vobSamplePDF (pdfTarget.GetDocument())

let newDoc = new Document(pdfTargetTemplated)
//vobSampleReader.Close()
//pdfSourceReader.Close()
//pdfTargetWriter.Close()
vobSamplePDF.Close()
pdfSource.Close()
newDoc.Close()
