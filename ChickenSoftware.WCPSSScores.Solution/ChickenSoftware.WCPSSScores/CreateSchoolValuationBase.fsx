#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"
#r "../packages/Microsoft.Azure.Documents.Client.0.9.2-preview/lib/net40/Microsoft.Azure.Documents.Client.dll"
#r "../packages/Newtonsoft.Json.4.5.11/lib/net40/Newtonsoft.Json.dll"
#r "../packages/FSharp.Collections.ParallelSeq.1.0.2/lib/net40/FSharp.Collections.ParallelSeq.dll"

open System
open System.IO
open FSharp.Data
open System.Linq
open Newtonsoft.Json
open Microsoft.Azure.Documents
open System.Text.RegularExpressions
open Microsoft.Azure.Documents.Linq
open FSharp.Collections.ParallelSeq
open Microsoft.Azure.Documents.Client

type HouseAssignment = JsonProvider<"../data/HouseAssignmentSample.json">
type HouseValuation = JsonProvider<"../data/HouseValuationSample.json">

let endpointUrl = "https://chickensoftware.documents.azure.com:443/"
let authKey = "rk3sqMc6W/hB6SQEoaL8Yi1dvSn4C5VmvnrMdPSBQna3L8eCLMwnZeIJNpH8graTfV+GRxR2pYUUBFo5rdQuww=="
let client = new DocumentClient(new Uri(endpointUrl), authKey) 
let database = client.CreateDatabaseQuery().Where(fun db -> db.Id = "wakecounty" ).ToArray().FirstOrDefault()

let getAssignment (id:int) =
    let collection = client.CreateDocumentCollectionQuery(database.CollectionsLink).Where(fun dc -> dc.Id = "houseassignment").ToArray().FirstOrDefault()
    let documentLink = collection.SelfLink
    let queryString = "SELECT * FROM houseassignment WHERE houseassignment.houseIndex = " + id.ToString()
    let query = client.CreateDocumentQuery(documentLink,queryString)
    match query |> Seq.length with
    | 0 -> None
    | _ -> 
        let assignmentValue = query |> Seq.head
        let assignment = HouseAssignment.Parse(assignmentValue.ToString())
        Some assignment

let getValuation (id:int) =
    let collection = client.CreateDocumentCollectionQuery(database.CollectionsLink).Where(fun dc -> dc.Id = "taxinformation").ToArray().FirstOrDefault()
    let documentLink = collection.SelfLink
    let queryString = "SELECT * FROM taxinformation WHERE taxinformation.index = 1"
    let query = client.CreateDocumentQuery(documentLink,queryString)
    match query |> Seq.length with
    | 0 -> None
    | _ -> 
        let valuationValue = query |> Seq.head
        let valuation = HouseValuation.Parse(valuationValue.ToString())
        Some valuation

let assignSchoolTaxBase (id:int) =
    let assignment = getAssignment(id)
    let valuation = getValuation(id)
    match assignment.IsSome,valuation.IsSome with
    | true, true -> assignment.Value.Schools 
                    |> Seq.map(fun s -> s, valuation.Value.AssessedValue)
                    |> Some
    | _ -> None

let indexes = [|1..350000|]

#time
indexes |> PSeq.map(fun i -> assignSchoolTaxBase(i))
        |> Seq.filter(fun s -> s.IsSome)
        |> Seq.collect(fun s -> s.Value)   
        |> Seq.groupBy(fun (s,av) -> s)
        |> Seq.map(fun (s,ss) -> s,ss |> Seq.sumBy(fun (s,av)-> av))
        |> Seq.toArray

indexes

