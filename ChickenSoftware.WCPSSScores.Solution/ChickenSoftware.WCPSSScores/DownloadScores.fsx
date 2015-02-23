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

let endpointUrl = "https://chickensoftware.documents.azure.com:443/"
let authKey = "rk3sqMc6W/hB6SQEoaL8Yi1dvSn4C5VmvnrMdPSBQna3L8eCLMwnZeIJNpH8graTfV+GRxR2pYUUBFo5rdQuww=="
let client = new DocumentClient(new Uri(endpointUrl), authKey) 
let database = client.CreateDatabaseQuery().Where(fun db -> db.Id = "wakecounty" ).ToArray().FirstOrDefault()
let collection = client.CreateDocumentCollectionQuery(database.CollectionsLink).Where(fun dc -> dc.Id = "houseassignment").ToArray().FirstOrDefault()
let documentLink = collection.SelfLink
let get        
        
//client.CreateDocumentAsync(documentLink, houseAssignment.Value) |> ignore



