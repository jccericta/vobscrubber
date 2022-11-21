module PdfScrubber
open System.Text
open PdfSharp.Pdf.IO
open PdfSharp.Pdf.Content
open PdfSharp.Pdf.Content.Objects
open FSharp.Markdown
open FSharp.Markdown.Pdf
open FSharp.MetadataFormat.ValueReader
open FSharp.Data

let rec extractText(content:CObject, sb:StringBuilder) =
   match content with
   | :? CArray as (xs: CArray) -> for x: CObject in xs do extractText(x, sb)
   | :? CComment -> ()
   | :? CInteger -> ()
   | :? CName -> ()
   | :? CNumber -> ()
   | :? COperator as (op: COperator) // Tj/TJ = Show text
      when op.OpCode.OpCodeName = OpCodeName.Tj ||
            op.OpCode.OpCodeName = OpCodeName.TJ ->
      for element: CObject in op.Operands do extractText(element, sb)
      sb.Append(" ") |> ignore
   | :? COperator -> ()
   | :? CSequence as (xs: CSequence) -> for x: CObject in xs do extractText(x, sb)
   | :? CString as (s: CString) -> sb.Append(s.Value) |> ignore
   | (x: CObject) -> raise <| System.NotImplementedException(x.ToString())

let readAllText (pdfPath:string) =
   use document: PdfSharp.Pdf.PdfDocument = PdfReader.Open(pdfPath, PdfDocumentOpenMode.ReadOnly)
   let result: StringBuilder = StringBuilder()
   for page: PdfSharp.Pdf.PdfPage in document.Pages do
      let content: CSequence = ContentReader.ReadContent(page)
      extractText(content, result)
      result.AppendLine() |> ignore
   result.ToString()
   
let text: string = readAllText @"../Roberto Retana The Edge VOB 04.12.2022.pdf"

