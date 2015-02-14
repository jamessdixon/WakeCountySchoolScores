
#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"
#r "../packages/Microsoft.Azure.Documents.Client.0.9.2-preview/lib/net40/Microsoft.Azure.Documents.Client.dll"
#r "../packages/Newtonsoft.Json.4.5.11/lib/net40/Newtonsoft.Json.dll"

open System
open System.IO
open FSharp.Data
open System.Linq
open Microsoft.Azure.Documents
open Microsoft.Azure.Documents.Client
open Microsoft.Azure.Documents.Linq

type HouseValuation = JsonProvider<"../data/HouseValuationSample.json">

type SearchCriteria = {streetTemplateValue:string;streetName:string;streetNumber:string;}

let getPropertyValue(id: int)=
        let endpointUrl = "https://chickensoftware.documents.azure.com:443/"
        let authKey = "rk3sqMc6W/hB6SQEoaL8Yi1dvSn4C5VmvnrMdPSBQna3L8eCLMwnZeIJNpH8graTfV+GRxR2pYUUBFo5rdQuww=="
        let client = new DocumentClient(new Uri(endpointUrl), authKey) 
        let database = client.CreateDatabaseQuery().Where(fun db -> db.Id = "wakecounty" ).ToArray().FirstOrDefault()
        let collection = client.CreateDocumentCollectionQuery(database.CollectionsLink).Where(fun dc -> dc.Id = "taxinformation").ToArray().FirstOrDefault()
        let documentLink = collection.SelfLink
        let queryString = "SELECT * FROM taxinformation WHERE taxinformation.index = " + id.ToString()
        let query = client.CreateDocumentQuery(documentLink,queryString)
        let firstValue = query |> Seq.head
        HouseValuation.Parse(firstValue.ToString())
   
let createSchoolAssignmentSearchCriteria(id: int) =
    let deliminators = [|(char)32;(char)160|]
    let property = getPropertyValue(id)
    let addressOneTokens = property.AddressOne.Split(deliminators)
    let streetNumber = addressOneTokens.[0]
    let streetTemplateValue = addressOneTokens.[2]
    let streetName = addressOneTokens.[1..] |> Array.reduce(fun acc t -> acc + "+" + t)
    let addressTwoTokens = property.AddressTwo.Split(deliminators)
    let city = addressTwoTokens.[0]
    let streetName' = streetName + city
    {streetTemplateValue=streetTemplateValue;
    streetName=streetName';
    streetNumber=streetNumber;}

let result' = createSchoolAssignmentSearchCriteria 1







