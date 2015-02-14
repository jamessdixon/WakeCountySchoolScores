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


let createHouseValueJson(houseValuation:option<HouseValuation>) =
    match houseValuation.IsSome with
    | true ->
        let index = houseValuation.Value.index.ToString()
        let result = JsonValue.Record [| 
                        "index", JsonValue.String index
                        "addressOne", JsonValue.String houseValuation.Value.addressOne
                        "addressTwo", JsonValue.String houseValuation.Value.addressTwo
                        "addressThree", JsonValue.String houseValuation.Value.addressThree
                        "assessedValue", JsonValue.String houseValuation.Value.assessedValue |] 
        Some result
    | false -> None

let writeValuationToDisk(valuation: option<JsonValue>) =
    match valuation.IsSome with
    | true -> File.AppendAllText(@"C:\Data\propertyData.json",valuation.Value.ToString() + "," )
    | false -> ()

let writeValuationToDocumentDb(houseValuation:option<HouseValuation>) =
    match houseValuation.IsSome with
    | true -> 
        let endpointUrl = "https://chickensoftware.documents.azure.com:443/"
        let authKey = "rk3sqMc6W/hB6SQEoaL8Yi1dvSn4C5VmvnrMdPSBQna3L8eCLMwnZeIJNpH8graTfV+GRxR2pYUUBFo5rdQuww=="
        let client = new DocumentClient(new Uri(endpointUrl), authKey) 
        let database = client.CreateDatabaseQuery().Where(fun db -> db.Id = "wakecounty" ).ToArray().FirstOrDefault()
        let collection = client.CreateDocumentCollectionQuery(database.CollectionsLink).Where(fun dc -> dc.Id = "taxinformation").ToArray().FirstOrDefault()
        let documentLink = collection.SelfLink
        client.CreateDocumentAsync(documentLink, houseValuation.Value) |> ignore
    | false -> ()
    
let doValuationToDisk(id: int) =
    createUri id
    |> getValuation 
    |> createHouseValueJson
    |> writeValuationToDisk
    |> ignore

let doValuationToDocumentDb(id: int) =
    createUri id
    |> getValuation
    |> writeValuationToDocumentDb
    |> ignore

#time
[1..100] |> Seq.iter(fun id -> doValuationToDocumentDb id)


