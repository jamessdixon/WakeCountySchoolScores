
#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"

open System.Net
open FSharp.Data

type context = HtmlProvider<"../data/HouseSearchSample01.html">
type context' = HtmlProvider<"../data/HouseSearchSample02.html">
type SearchCriteria = {streetTemplateValue:string;street:string;suffix:string;city:string;streetNumber:string;}

let uri = "http://wwwgis2.wcpss.net/addressLookup/index.php"

let webClient = new WebClient()
webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded")

let createPage1QueryString(searchCriteria:SearchCriteria)=
    let streetName = searchCriteria.street+"+"+searchCriteria.suffix+"+"+searchCriteria.city
    "StreetTemplateValue="+searchCriteria.streetTemplateValue+"&StreetName="+streetName+"&StreetNumber="+searchCriteria.streetNumber+"&SubmitAddressSelectPage=CONTINUE&DefaultAction=SubmitAddressSelectPage"

let createPage2QueryString(queryString:string) =
    let result = webClient.UploadString(uri,"POST",queryString)
    let body = context.Parse(result).Html.Body()
    let inputs = body.Descendants("INPUT") |> Seq.toList
    let inputs' = inputs |> Seq.map(fun i -> i.Attribute("name").Value(),i.Attribute("value").Value())
    let getValueFromInput(nameToFind:string) =
        inputs' |> Seq.filter(fun (n,v) -> n = nameToFind) 
                                    |> Seq.map(fun (n,v) -> v)
                                    |> Seq.head
    let catchmentCode = getValueFromInput("CatchmentCode") 
    let streetZipCode = getValueFromInput("StreetZipCode") 
    "SelectAssignment%7C2014%7CCURRENT=2014-15&DefaultAction=SelectAssignment%7C2014%7CCURRENT&DefaultAction=SelectAssignment%7C2015%7CCURRENT&CatchmentCode="+catchmentCode+"&StreetName="+streetName+"&StreetTemplateValue="+streetTemplateValue+"&StreetNumber="+streetNumber+"&StreetZipCode="+streetZipCode

let getSchoolData(queryString:string) =
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
    let unimportantPhrases = [|"Neighborhood Busing";"This school has an enrollment cap"|]
    let containsUnimportantPhrase (s:string) =
        unimportantPhrases |> Seq.exists(fun p -> s.Contains(p))
    schoolData''' |> Seq.filter(fun s -> containsUnimportantPhrase(s) = false )


let searchCriteria = {streetTemplateValue="STRAT";street="Strathorn";suffix="Dr";city="Cary";streetNumber="904"}
let result = createPage1QueryString(searchCriteria)
                |> createPage2QueryString
                |> getSchoolData



