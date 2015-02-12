
#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"

open System.Net
open FSharp.Data

type context = HtmlProvider<"../data/HouseSearchSample01.html">
type context' = HtmlProvider<"../data/HouseSearchSample02.html">

let uri = "http://wwwgis2.wcpss.net/addressLookup/index.php"
let streetLookup = "StreetTemplateValue=STRATH&StreetName=Strathorn+Dr+Cary&StreetNumber=904&SubmitAddressSelectPage=CONTINUE&DefaultAction=SubmitAddressSelectPage"
let streetLookup' = "SelectAssignment%7C2014%7CCURRENT=2014-15&DefaultAction=SelectAssignment%7C2014%7CCURRENT&DefaultAction=SelectAssignment%7C2015%7CCURRENT&CatchmentCode=CA+0198.2&StreetName=Strathorn+Dr+Cary&StreetTemplateValue=STRATH&StreetNumber=904&StreetZipCode=27519"

//CatchmentCode=CA+0198.2

let webClient = new WebClient()
webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded")
let result = webClient.UploadString(uri,"POST",streetLookup')
let body = context'.Parse(result).Html.Body()

let tables = body.Descendants("TABLE") |> Seq.toList
let schoolTable = tables.[0]
let schoolRows = schoolTable.Descendants("TR") |> Seq.toList
let schoolData = schoolRows |> Seq.collect(fun r -> r.Descendants("TD")) |>Seq.toList
let schoolData' = schoolData |> Seq.map(fun d -> d.InnerText().Trim()) 
let schoolData'' = schoolData' |> Seq.filter(fun s -> s <> System.String.Empty) 

//Strip out noise
let removeNonEssentialData (s:string) =
    let markerPosition = s.IndexOf('(')
    match markerPosition with
    | -1 -> s
    | _ -> s.Substring(0,markerPosition).Trim()

let schoolData''' = schoolData'' |> Seq.map(fun s -> removeNonEssentialData(s))

let unimportantPhrases = [|"Neighborhood Busing";"This school has an enrollment cap"|]
let containsUnimportantPhrase (s:string) =
    unimportantPhrases |> Seq.exists(fun p -> s.Contains(p))

let schoolData'''' = schoolData''' |> Seq.filter(fun s -> containsUnimportantPhrase(s) = false )

schoolData''''


//For Blog #1
//schoolRows |> Seq.mapi(fun r i -> r.[i].Descendants("TD"))

//let tables = body.Descendants("TABLE") |> Seq.toList
//let schoolTable = tables.[0]
//let schoolRows = schoolTable.Descendants("TR") |> Seq.toList
//let elementaryDatas = schoolRows.[0].Descendants("TD") |> Seq.toList
//let elementarySchool = elementaryDatas.[1].InnerText()
//let middleSchoolDatas = schoolRows.[1].Descendants("TD") |> Seq.toList
//let middleSchool = middleSchoolDatas.[1].InnerText()
////Need to skip for the enrollement cap message
//let highSchoolDatas = schoolRows.[3].Descendants("TD") |> Seq.toList
//let highSchool = highSchoolDatas.[1].InnerText()





