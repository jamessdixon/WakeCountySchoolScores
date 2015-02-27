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
type HousePyramid = {index:int; elementarySchool:string; middleSchool:string; highSchool:string}

let endpointUrl = "https://chickensoftware.documents.azure.com:443/"
let authKey = "rk3sqMc6W/hB6SQEoaL8Yi1dvSn4C5VmvnrMdPSBQna3L8eCLMwnZeIJNpH8graTfV+GRxR2pYUUBFo5rdQuww=="
let client = new DocumentClient(new Uri(endpointUrl), authKey) 
let database = client.CreateDatabaseQuery().Where(fun db -> db.Id = "wakecounty" ).ToArray().FirstOrDefault()
let collection = client.CreateDocumentCollectionQuery(database.CollectionsLink).Where(fun dc -> dc.Id = "houseassignment").ToArray().FirstOrDefault()
let documentLink = collection.SelfLink
let queryString = "SELECT * FROM houseassignment WHERE houseassignment.houseIndex = 1"
let query = client.CreateDocumentQuery(documentLink,queryString)
let firstValue = query |> Seq.head
let result = HouseAssignment.Parse(firstValue.ToString())
let housePyramid = {index=result.HouseIndex; elementarySchool=result.Schools.[0];middleSchool=result.Schools.[1];highSchool=result.Schools.[2]}
let housePyramid' = JsonConvert.SerializeObject(housePyramid)
File.AppendAllText(@"F:\Git\WakeCountySchoolScores\Data\houseAssignment.json",housePyramid'.ToString() + "," )


