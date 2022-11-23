module PdfScrubber

open System.IO
open System.Text
open System.Text.Json
open PdfSharp.Pdf.Content
open PdfSharp.Pdf.Content.Objects
open PdfSharp.Pdf.IO
open FSharp.Data

(*let rec extractText(content:CObject, sb:StringBuilder) =
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
   
let text: string = readAllText @"../Roberto Retana The Edge VOB 04.12.2022.pdf"*)

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
let path = @"../../VOBScrubber/Test.pdf"
let docPath = PdfSharp.Pdf.IO.PdfReader.Open(path, PdfDocumentOpenMode.InformationOnly).FullPath
let doc = PdfSharp.Pdf.IO.PdfReader.Open(docPath, PdfDocumentOpenMode.InformationOnly)
let form = doc.AcroForm

if form = null then
   printfn "Pdf has no forms" |> exit -1 

let fields = form.Fields

let rec loop4 (fields:PdfSharp.Pdf.AcroForms.PdfAcroField.PdfAcroFieldCollection, sb:StringBuilder)  = 
   let fieldNames = fields.Names
   fieldNames |> Seq.iter (fun fn -> 
      if fields.Item(fn).HasKids = true then
         if fields.Item(fn).GetType() = typeof<PdfSharp.Pdf.AcroForms.PdfCheckBoxField> then
            let checkbox:PdfSharp.Pdf.AcroForms.PdfCheckBoxField = fields.Item(fn) |> unbox
            let checkedBox = checkbox.Name
            let checkedBoxName = checkbox.CheckedName
            let checkedBoxVal = checkbox.Value
            if checkedBoxVal <> null then
               sb.Append(sprintf """{ "%s": "%s" "%s"},""" checkedBox checkedBoxName (checkedBoxVal.ToString())) |> ignore
            else
               sb.Append(sprintf """{"%s": "%s"},""" checkedBox "") |> ignore
         else if fields.Item(fn).GetType() = typeof<PdfSharp.Pdf.AcroForms.PdfTextField> then
            let textField:PdfSharp.Pdf.AcroForms.PdfTextField = fields.Item(fn) |> unbox
            let textFieldName = textField.Name
            let textFieldVal = textField.Text
            if textFieldVal <> null then
               sb.Append(sprintf """{"%s": "%s"},""" textFieldName textFieldVal) |> ignore
            else
               sb.Append(sprintf """{"%s": "%s"},""" textFieldName "") |> ignore
         else
            sb.Append(sprintf """"%s":[""" (fields.Item(fn).Name)) |> ignore
            let fieldKids = fields.Item(fn).Fields
            loop4 (fieldKids,sb)
      else
         let field = fields.Item(fn).Name
         if fields.Item(fn).Value <> null then
            let fieldValue = fields.Item(fn).Value.ToString()
            sb.Append(sprintf """{"%s": "%s"},""" field fieldValue) |> ignore
         else
            sb.Append(sprintf """{"%s": "%s"},""" field "") |> ignore
   )
   sb.Append("]") |> ignore
   sb.AppendJoin("\n", ",") |> ignore
   sb.Replace(",]", "]") |> ignore

let sb = StringBuilder()
let guid = doc.Guid 
sb.Append("[")
loop4 (fields,sb)
let str = sb.ToString()
let jsonStr = str |> JsonValue.String
let jsonToStr = jsonStr.ToString()
jsonToStr

