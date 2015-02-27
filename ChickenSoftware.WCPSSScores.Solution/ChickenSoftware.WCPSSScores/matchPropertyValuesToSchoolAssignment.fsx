
#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"
#r "../packages/Microsoft.Azure.Documents.Client.0.9.2-preview/lib/net40/Microsoft.Azure.Documents.Client.dll"
#r "../packages/Newtonsoft.Json.4.5.11/lib/net40/Newtonsoft.Json.dll"
#r "../packages/FSharp.Collections.ParallelSeq.1.0.2/lib/net40/FSharp.Collections.ParallelSeq.dll"

#load "SchoolAssignments.fsx"

open System
open System.IO
open FSharp.Data
open System.Linq
open SchoolAssignments
open Microsoft.Azure.Documents
open Microsoft.Azure.Documents.Client
open Microsoft.Azure.Documents.Linq
open FSharp.Collections.ParallelSeq

type HouseValuation = JsonProvider<"../data/HouseValuationSample.json">
type HouseAssignment = {houseIndex:int; schools: seq<string>}

let getPropertyValue(id: int)=
        let endpointUrl = "https://chickensoftware.documents.azure.com:443/"
        let authKey = "rk3sqMc6W/hB6SQEoaL8Yi1dvSn4C5VmvnrMdPSBQna3L8eCLMwnZeIJNpH8graTfV+GRxR2pYUUBFo5rdQuww=="
        let client = new DocumentClient(new Uri(endpointUrl), authKey) 
        let database = client.CreateDatabaseQuery().Where(fun db -> db.Id = "wakecounty" ).ToArray().FirstOrDefault()
        let collection = client.CreateDocumentCollectionQuery(database.CollectionsLink).Where(fun dc -> dc.Id = "taxinformation").ToArray().FirstOrDefault()
        let documentLink = collection.SelfLink
        let queryString = "SELECT * FROM taxinformation WHERE taxinformation.index = " + id.ToString()
        let query = client.CreateDocumentQuery(documentLink,queryString)
        match query |> Seq.length with
        | 0 -> None
        | _ -> let firstValue = query |> Seq.head
               let firstValue' = HouseValuation.Parse(firstValue.ToString())
               Some firstValue'

let createSchoolAssignmentSearchCriteria(houseValuation: option<HouseValuation.Root>) =
    match houseValuation.IsSome with
    | true -> let deliminators = [|(char)32;(char)160|]
              let addressOneTokens = houseValuation.Value.AddressOne.Split(deliminators)
              let streetNumber = addressOneTokens.[0]
              let streetTemplateValue = addressOneTokens.[1]
              let streetName = addressOneTokens.[1..] |> Array.reduce(fun acc t -> acc + "+" + t)
              let addressTwoTokens = houseValuation.Value.AddressTwo.Split(deliminators)
              let city = addressTwoTokens.[0]
              let streetName' = streetName + city
              Some {SearchCriteria.streetTemplateValue=streetTemplateValue;
               streetName=streetName';
               streetNumber=streetNumber;}
    | false -> None
    
let writeSchoolAssignmentToDocumentDb(houseAssignment:option<HouseAssignment>) =
    match houseAssignment.IsSome with
    | true -> 
        let endpointUrl = "https://chickensoftware.documents.azure.com:443/"
        let authKey = "rk3sqMc6W/hB6SQEoaL8Yi1dvSn4C5VmvnrMdPSBQna3L8eCLMwnZeIJNpH8graTfV+GRxR2pYUUBFo5rdQuww=="
        let client = new DocumentClient(new Uri(endpointUrl), authKey) 
        let database = client.CreateDatabaseQuery().Where(fun db -> db.Id = "wakecounty" ).ToArray().FirstOrDefault()
        let collection = client.CreateDocumentCollectionQuery(database.CollectionsLink).Where(fun dc -> dc.Id = "houseassignment").ToArray().FirstOrDefault()
        let documentLink = collection.SelfLink
        client.CreateDocumentAsync(documentLink, houseAssignment.Value) |> ignore
    | false -> ()

let createHouseAssignment(id:int)=
    let houseValuation = getPropertyValue(id)
    let schools = houseValuation
                     |> createSchoolAssignmentSearchCriteria
                     |> createSearchCriteria'
                     |> createPage2QueryString
                     |> getSchoolData
    match schools.IsSome with
    | true -> Some {houseIndex=houseValuation.Value.Index; schools=schools.Value}
    | false -> None

//let result = createHouseAssignment 2
//result

let generateHouseAssignment(id:int)=
    createHouseAssignment id
    |> writeSchoolAssignmentToDocumentDb
    ()

//let result = generateHouseAssignment 1

//#time
//[2..100] |> PSeq.iter(fun id -> generateHouseAssignment id)




