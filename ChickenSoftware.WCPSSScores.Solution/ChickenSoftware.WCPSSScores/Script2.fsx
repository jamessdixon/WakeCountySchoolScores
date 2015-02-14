#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"

open System.Text
open System.Net
open FSharp.Data

type context' = HtmlProvider<"../data/HouseSearchSample02.html">

let uri = "http://wwwgis2.wcpss.net/addressLookup/index.php"

let webClient = new WebClient()
webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded")

let stringBuilder = new StringBuilder()
stringBuilder.Append("SelectAssignment%7C2014%7CCURRENT=2014-15")
stringBuilder.Append("&DefaultAction=SelectAssignment%7C2014%7CCURRENT&DefaultAction=SelectAssignment%7C2015%7CCURRENT")
stringBuilder.Append("&CatchmentCode=CA+0198.2")
stringBuilder.Append("&StreetName=Strathorn+Dr+Cary")
stringBuilder.Append("&StreetTemplateValue=STRATH")
stringBuilder.Append("&StreetNumber=904")
stringBuilder.Append("&StreetZipCode=27519")
let queryString = stringBuilder.ToString()

let result = webClient.UploadString(uri,"POST",queryString)
let body = context'.Parse(result).Html.Body()

let tables = body.Descendants("TABLE") |> Seq.toList
let schoolTable = tables.[0]
let schoolRows = schoolTable.Descendants("TR") |> Seq.toList
let schoolData = schoolRows |> Seq.collect(fun r -> r.Descendants("TD")) |>Seq.toList
let schoolData' = schoolData |> Seq.map(fun d -> d.InnerText().Trim()) 
let schoolData'' = schoolData' |> Seq.filter(fun s -> s <> System.String.Empty) 

let removeNonEssentialData (s:string) =
    let markerPosition = s.IndexOf('(')
    match markerPosition with
    | -1 -> s
    | _ -> s.Substring(0,markerPosition).Trim()

let schoolData''' = schoolData'' |> Seq.map(fun s -> removeNonEssentialData(s))
let unimportantPhrases = [|"Neighborhood Busing";
                        "This school has an enrollment cap";
                        "2015 BASE ATTENDANCE AREA";
                        "2014 BASE ATTENDANCE AREA"|]
let containsUnimportantPhrase (s:string) =
    unimportantPhrases |> Seq.exists(fun p -> s.Contains(p))
let schools = schoolData''' |> Seq.filter(fun s -> containsUnimportantPhrase(s) = false )

schools

