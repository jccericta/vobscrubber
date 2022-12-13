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
open Microsoft.FSharp.Collections
open System.Collections.Generic

let currDir = System.IO.Directory.GetCurrentDirectory()
let vobSampleFile = currDir + @"\pdfs\Test.pdf"
let vobSampleReader = new Pdf.PdfReader(vobSampleFile)
let vobSamplePDF = new Pdf.PdfDocument(vobSampleReader)
let pdfSourceFile = currDir + @"\pdfs\John Ericta TaxReturn 2021.pdf"
let pdfSourceReader = new Pdf.PdfReader(pdfSourceFile)
let pdfSource = new Pdf.PdfDocument(pdfSourceReader)
let pdfTargetFile = new Pdf.PdfWriter(currDir + @"\pdfs\Copy.pdf")
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
    let pdfAcroForm' = PdfAcroForm.GetAcroForm(t, true)
    for i in 1 .. s.GetNumberOfPages() do
        c.Copy(s.GetPage(i),t.GetPage(i))
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