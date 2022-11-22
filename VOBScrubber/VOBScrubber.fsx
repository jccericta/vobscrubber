#r @"bin\Debug\net7.0\FSharp.Core.dll"
#r @"bin\Debug\net7.0\PdfSharp.dll"
#r @"bin\Debug\net7.0\PdfSharp.Charting.dll"

open System.Text
open PdfSharp.Pdf.IO
open PdfSharp.Pdf.Content
open PdfSharp.Pdf.Content.Objects
open FSharp.Data

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
let path = @"../../VOBScrubber/Test.pdf"
let docPath = PdfSharp.Pdf.IO.PdfReader.Open(path, PdfDocumentOpenMode.InformationOnly).FullPath
let doc = PdfSharp.Pdf.IO.PdfReader.Open(docPath, PdfDocumentOpenMode.InformationOnly)
let form = doc.AcroForm

if form = null then
   printfn "Pdf has no forms" |> exit -1 

let fields = form.Fields

// fields.Names
// fields.DescendantNames |> printfn "%A"

(*let loop (fields:PdfSharp.Pdf.AcroForms.PdfAcroField.PdfAcroFieldCollection) = 
   (
      for i = 0 to fields.Count - 1 do
         let pField = fields.[i]
         let pFieldName = pField.Name
         if pField.GetType() <> typeof<PdfSharp.Pdf.AcroForms.PdfCheckBoxField> then
            if pField.HasKids = true then
               let cFields = pField.Fields
               for j = 0 to cFields.Count - 1  do
                  let cField = cFields.[j]
                  let cFieldName = cField.Name
                  let cItem = cFields.Item(cFieldName)
                  let cItemName = cItem.Name
                  let cFieldValue = 
                     if cItem.Value = null then
                        let result = ""
                        result
                     else
                        let result = cItem.Value.ToString()
                        result
                  printfn "'%s': { %s }" cItemName cFieldValue
            else
               if pField.Value <> null then
                  printfn "'%s': { %s }" pFieldName (pField.Value.ToString())
               else 
                  printfn "'%s': { %s }" pFieldName null
         else
            if pField.Value <> null then
                  printfn "'%s': { %s }" pFieldName (pField.Value.ToString())
               else 
                  printfn "'%s': { %s }" pFieldName null
   ) |> printfn "} %A"
   

let rec loop2 (fields:PdfSharp.Pdf.AcroForms.PdfAcroField.PdfAcroFieldCollection) = 
   let fieldNames = fields.Names
   for fn in fieldNames do
      if fields.Item(fn).HasKids then
         printfn "'%s' : {" (fields.Item(fn).Name)
         let fieldKids = fields.Item(fn).Fields
         loop fieldKids
      else
         let field = fields.Item(fn).Name
         if fields.Item(fn).Value <> null then
            let fieldValue = fields.Item(fn).Value.ToString()
            printfn "%s: { %s }" field fieldValue
         else
            printfn "%s: { %s }" field null

do loop2 fields*)

// let rec loop3 (fields:PdfSharp.Pdf.AcroForms.PdfAcroField.PdfAcroFieldCollection, sb:StringBuilder)  = 
//    let fieldNames = fields.Names
//    for fn in fieldNames do
//       if fields.Item(fn).HasKids then
//          if fields.Item(fn).GetType() = typeof<PdfSharp.Pdf.AcroForms.PdfCheckBoxField> then
//             if fields.Item(fn).Value <> null then
//                printfn "{'%s': %s}," (fields.Item(fn).Name) (fields.Item(fn).Value.ToString())
//                //sb.Append("{ '"+(fields.Item(fn).Name) + "': " + (fields.Item(fn).Value.ToString()) + " }")
//             else
//                printfn "{'%s': %s}," (fields.Item(fn).Name) null
//                //sb.Append("{ '"+(fields.Item(fn).Name) + "': " + null + " }")
//          else if fields.Item(fn).GetType() = typeof<PdfSharp.Pdf.AcroForms.PdfTextField> then
//             if fields.Item(fn).Value <> null then
//                printfn "{'%s': %s}," (fields.Item(fn).Name) (fields.Item(fn).Value.ToString())
//                //sb.Append("{ '"+(fields.Item(fn).Name) + "': " + (fields.Item(fn).Value.ToString()) + " }")
//             else
//                printfn "{'%s': %s}," (fields.Item(fn).Name) null
//                //sb.Append("{ '"+(fields.Item(fn).Name) + "': " + null + " }")
//          else
//             printfn "'%s': [ " (fields.Item(fn).Name)
//             //sb.Append("'"+(fields.Item(fn).Name) + "': [ ")
//             let fieldKids = fields.Item(fn).Fields
//             loop3 (fieldKids,sb)
//       else
//          let field = fields.Item(fn).Name
//          if fields.Item(fn).Value <> null then
//             let fieldValue = fields.Item(fn).Value.ToString()
//             printfn "{'%s': %s}," field fieldValue
//             //sb.Append("{ '"+ field + "': " + fieldValue + " }")
//          else
//             printfn "'%s': %s," field null
//             // /sb.Append("{ '"+field + "': " + null + " }")
//             //sb.Append(",")
//    printfn "],"
//    //sb.Append "],"

// let sb = StringBuilder()   
// loop3 (fields,sb)
let sb = StringBuilder()
let guid = doc.Guid 
sb.Append(sprintf "{Guid: %A}," guid) |> ignore 

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
               sb.Append(sprintf " {'%s': %s %s}" checkedBox checkedBoxName (checkedBoxVal.ToString())) |> ignore
            else
               sb.Append(sprintf " {'%s': %s}" checkedBox null) |> ignore
         else if fields.Item(fn).GetType() = typeof<PdfSharp.Pdf.AcroForms.PdfTextField> then
            let textField:PdfSharp.Pdf.AcroForms.PdfTextField = fields.Item(fn) |> unbox
            let textFieldName = textField.Name
            let textFieldVal = textField.Text
            if textFieldVal <> null then
               sb.Append(sprintf " {'%s': %s}" textFieldName textFieldVal) |> ignore
            else
               sb.Append(sprintf " {'%s': %s}" textFieldName null) |> ignore
         else
            sb.Append(sprintf " '%s':[" (fields.Item(fn).Name)) |> ignore
            let fieldKids = fields.Item(fn).Fields
            loop4 (fieldKids,sb)
      else
         let field = fields.Item(fn).Name
         if fields.Item(fn).Value <> null then
            let fieldValue = fields.Item(fn).Value.ToString()
            sb.Append(sprintf "{'%s': %s}," field fieldValue) |> ignore
         else
            sb.Append(sprintf "{'%s': %s}," field null) |> ignore
   )
   sb.Append("] ") |> ignore

loop4 (fields,sb)
sb.ToString()

