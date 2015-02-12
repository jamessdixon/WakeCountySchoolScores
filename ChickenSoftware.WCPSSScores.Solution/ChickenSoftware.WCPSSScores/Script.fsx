//Get all addresses in wake county
//Get property value of house
//Get school assignment

//http://services.wakegov.com/realestate/Account.asp?id=0000001


#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"

open System.IO
open FSharp.Data

type context = HtmlProvider<"../data/RealEstateSample.html">
type houseValuation = {addressOne:string; addressTwo:string; addressThree:string; assessedValue:string}

let createUri(id: int) = 
    "http://services.wakegov.com/realestate/Account.asp?id=" + id.ToString("D7")

let getValuation(uri: string)=
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

        let result = JsonValue.Record [| 
                        "addressOne", JsonValue.String addressOne
                        "addressTwo", JsonValue.String addressTwo
                        "addressThree", JsonValue.String addressThree
                        "assessedValue", JsonValue.String assessedValue |] 
        Some result
    with
          | :? System.ArgumentException ->  None

let writeValuation(valuation: option<JsonValue>) =
    match valuation.IsSome with
    | true -> File.AppendAllText(@"C:\Data\data.json",valuation.Value.ToString() + "," )
    | false -> ()

let doValuation(id: int) =
    createUri id
    |> getValuation
    |> writeValuation
    |> ignore

#time
[1..100] |> Seq.iter(fun id -> doValuation id)


