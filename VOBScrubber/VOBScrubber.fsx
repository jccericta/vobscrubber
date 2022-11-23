#r @"bin\Debug\net7.0\FSharp.Core.dll"
#r @"bin\Debug\net7.0\PdfSharp.dll"
#r @"bin\Debug\net7.0\PdfSharp.Charting.dll"
#r @"bin\Debug\net7.0\FSharp.Data.dll"

open System.IO
open System.Runtime.Serialization.Json
open System.Text
open System.Text.Json
open PdfSharp.Pdf.Content
open PdfSharp.Pdf.Content.Objects
open PdfSharp.Pdf.IO
open FSharp.Data

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
let guid = 
   let guid'= doc.Guid
   if guid'.ToString() = "" then
      new System.Guid()
   else  
      guid'
sb.Append("[")
//sb.Append(sp1rintf "[{'Guid': '%A'}," guid) |> ignore 
loop4 (fields,sb)
let str = sb.ToString()
let jsonStr = str |> JsonValue.String
let jsonFile = @"../../VOBScrubber/Hansei.VOB." + guid.ToString() + ".json"
let jsonToStr = jsonStr.ToString()

let saveJsonToFile (json:string, path:string) = 
   let fs = new FileStream(path, FileMode.OpenOrCreate)
   (new DataContractJsonSerializer(typeof<string>)).WriteObject(fs,json)

saveJsonToFile(jsonToStr, jsonFile)
(*
let jsonToStr = jsonStr.ToString()
let addText (fs:FileStream, str:string) = 
   let (text:byte[]) = (new UTF8Encoding(true)).GetBytes(str)
   fs.Write(text, 0, text.Length)
let fs = System.IO.File.Create(jsonFile)   
addText(fs,jsonToStr)
*)
