#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"
#r "../packages/Microsoft.Azure.Documents.Client.0.9.2-preview/lib/net40/Microsoft.Azure.Documents.Client.dll"
#r "../packages/Newtonsoft.Json.4.5.11/lib/net40/Newtonsoft.Json.dll"

open System
open System.IO
open FSharp.Data
open System.Linq
open Newtonsoft.Json
open Microsoft.Azure.Documents
open System.Text.RegularExpressions
open Microsoft.Azure.Documents.Linq
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
    




//let housePyramid = {index=result.HouseIndex; elementarySchool=result.Schools.[0];middleSchool=result.Schools.[1];highSchool=result.Schools.[2]}
//let housePyramid' = JsonConvert.SerializeObject(housePyramid)
//File.AppendAllText(@"F:\Git\WakeCountySchoolScores\Data\houseAssignment.json",housePyramid'.ToString() + "," )


