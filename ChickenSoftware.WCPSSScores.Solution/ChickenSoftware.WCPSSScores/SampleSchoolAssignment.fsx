#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"
#r "../packages/Microsoft.Azure.Documents.Client.0.9.2-preview/lib/net40/Microsoft.Azure.Documents.Client.dll"
#r "../packages/Newtonsoft.Json.4.5.11/lib/net40/Newtonsoft.Json.dll"
#r "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.5.1/System.Net.Http.dll"

open System
open System.IO
open FSharp.Data
open System.Linq
open Newtonsoft.Json
open System.Net.Http
open Microsoft.Azure.Documents
open System.Text.RegularExpressions
open Microsoft.Azure.Documents.Linq
open Microsoft.Azure.Documents.Client

type HouseAssignment = JsonProvider<"../data/HouseAssignmentSample.json">

let getSchools (index:int) =
    try
        let endpointUrl = "https://chickensoftware.documents.azure.com:443/"
        let authKey = "rk3sqMc6W/hB6SQEoaL8Yi1dvSn4C5VmvnrMdPSBQna3L8eCLMwnZeIJNpH8graTfV+GRxR2pYUUBFo5rdQuww=="
        let client = new DocumentClient(new Uri(endpointUrl), authKey) 
        let database = client.CreateDatabaseQuery().Where(fun db -> db.Id = "wakecounty" ).ToArray().FirstOrDefault()
        let collection = client.CreateDocumentCollectionQuery(database.CollectionsLink).Where(fun dc -> dc.Id = "houseassignment").ToArray().FirstOrDefault()
        let documentLink = collection.SelfLink
        let queryString = "SELECT * FROM houseassignment WHERE houseassignment.houseIndex = " + index.ToString()
        let query = client.CreateDocumentQuery(documentLink,queryString)
        match Seq.length query with
        | 0 -> None                    
        | _ -> 
                let firstValue = query |> Seq.head
                let assignment = HouseAssignment.Parse(firstValue.ToString())
                Some assignment.Schools
    with
        | :? HttpRequestException as ex ->
            None

//http://stackoverflow.com/questions/6062191/f-getting-a-list-of-random-numbers
type System.Random with
    member this.GetValues(minValue, maxValue) =
        Seq.initInfinite (fun _ -> this.Next(minValue, maxValue))

let random = new System.Random(42)
let indexes = random.GetValues(1,350000) |> Seq.take(2000) |> Seq.toArray
let allSchools = indexes |> Seq.map(fun i -> getSchools(i)) |> Seq.toArray

let getNumberOfSchools (trial:int) =
    let trialSchools = allSchools.[1..trial]
    let allSchools' = trialSchools |> Seq.filter(fun s -> s.IsSome)
    let allSchools'' = allSchools' |> Seq.collect(fun s -> s.Value)
    let uniqueSchools = allSchools'' |> Seq.distinct
    uniqueSchools |> Seq.length

let trialCount = [|1..1999|]

trialCount |> Seq.map(fun t -> t, getNumberOfSchools(t))
           |> Seq.iter(fun (t, c) -> printfn "%A %A" t c)

let allSchools' = allSchools |> Seq.filter(fun s -> s.IsSome)
let allSchools'' = allSchools' |> Seq.collect(fun s -> s.Value)
let uniqueSchools = allSchools'' |> Seq.distinct
uniqueSchools |> Seq.iter(fun s ->  printfn "%A" s)




