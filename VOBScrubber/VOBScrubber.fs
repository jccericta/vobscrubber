﻿module PdfScrubber

open System.IO
open System.Runtime.Serialization.Json
open System.Text
open System.Text.Json
open PdfSharp.Pdf.Content
open PdfSharp.Pdf.Content.Objects
open PdfSharp.Pdf.IO
open FSharp.Data

// Take file at path, grab the full path and open the pdf reader stream, then scan for Acro forms
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
let currDir = System.IO.Directory.GetCurrentDirectory() + @"\VOBScrubber\"
let path = currDir + @"\pdfs\Test.pdf"
let docPath = PdfSharp.Pdf.IO.PdfReader.Open(path, PdfDocumentOpenMode.InformationOnly).FullPath
let doc = PdfSharp.Pdf.IO.PdfReader.Open(docPath, PdfDocumentOpenMode.InformationOnly)
let form = doc.AcroForm

// if no form exist, exit gracefully
if form = null then
   printfn "Pdf has no forms" |> exit -1 

// Grab the fields of the form
let fields = form.Fields

// recursively loop the form fields for any child fields, if kids then loop, else print out name, value.
// handles special cases such as checkboxes and textfields
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
               sb.Append(sprintf """{"%s": "(%s,%s)"},""" checkedBox checkedBoxName (checkedBoxVal.ToString())) |> ignore
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
            sb.Append(sprintf """{"%s":[""" (fields.Item(fn).Name)) |> ignore
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
   sb.Append("]},") |> ignore

// grab the document's id number or create one
let guid = 
   let guid'= doc.Guid
   if guid'.ToString() = "" then
      new System.Guid()
   else  
      guid'

// the string to concatenate
let sb = StringBuilder()
sb.Append("[") |> ignore

// call the recursive function
loop4 (fields,sb)

// fix string
sb.Replace(",]", "]") |> ignore
sb.Append(",") |> ignore
sb.Replace("},,", "") |> ignore
let str = sb.ToString()

// parse the string into json format
let json = JsonValue.Parse(str)

// turn it into a json string
let jsonStr = json.ToString()

// save json file with json string serialized
let jsonFile = currDir + @"\json\Hansei.VOB." + guid.ToString() + ".json"
let saveJsonToFile (json:string, path:string) = 
   let fs = new FileStream(path, FileMode.OpenOrCreate)
   (new DataContractJsonSerializer(typeof<string>)).WriteObject(fs,json)
   fs.Close()

// save it
saveJsonToFile(jsonStr, jsonFile)

// close pdf
doc.Close()

(*
let jsonToStr = jsonStr.ToString()
let addText (fs:FileStream, str:string) = 
   let (text:byte[]) = (new UTF8Encoding(true)).GetBytes(str)
   fs.Write(text, 0, text.Length)
let fs = System.IO.File.Create(jsonFile)   
addText(fs,jsonToStr)
*)

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



