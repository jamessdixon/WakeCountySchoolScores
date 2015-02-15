//Get all addresses in wake county
//Get property value of house

//http://services.wakegov.com/realestate/Account.asp?id=0000001

#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"
#r "../packages/Microsoft.Azure.Documents.Client.0.9.2-preview/lib/net40/Microsoft.Azure.Documents.Client.dll"
#r "../packages/Newtonsoft.Json.4.5.11/lib/net40/Newtonsoft.Json.dll"

open System
open System.IO
open FSharp.Data
open System.Linq
open Microsoft.Azure.Documents
open System.Text.RegularExpressions
open Microsoft.Azure.Documents.Client
open Microsoft.Azure.Documents.Linq

type context = HtmlProvider<"../data/RealEstateSample.html">
type HouseValuation = {index: int; addressOne:string; addressTwo:string; addressThree:string; assessedValue:string}

let createUri(id: int) = 
    id, "http://services.wakegov.com/realestate/Account.asp?id=" + id.ToString("D7")

let getValuation(index: int, uri: string)=
    try
        let body = context.Load(uri).Html.Body()
        let tables = body.Descendants("TABLE") |> Seq.toList
        let addressTable = tables.[7]
        let fonts = addressTable.Descendants("font") |> Seq.toList
        let addressOne = fonts.[1].InnerText()
        let addressTwo = fonts.[2].InnerText()
        let addressThree = fonts.[3].InnerText()

        let taxTable = tables.[11]
        let fonts' = taxTable.Descendants("font") |> Seq.toList
        let assessedValue = fonts'.[3].InnerText()

        let result = {index=index; addressOne=addressOne;addressTwo=addressTwo;addressThree=addressThree;assessedValue=assessedValue}
        Some result
    with
          | :? System.ArgumentException ->  None


//let getmaxNumberOfIndex(id) =
//    let uri = createUri(id)
//    let valuation = getValuation(uri)
//    match valuation.IsSome with
//    | true -> CalcMaxIndex.getmaxNumberOfIndex id
//    | false -> CalcMaxIndex.getmaxNumberOfIndex id




//0379931 via manual search




