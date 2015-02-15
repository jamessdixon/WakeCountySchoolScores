
#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"

open System.IO
open System.Net
open System.Text
open FSharp.Data

type context = HtmlProvider<"../data/HouseSearchSample01.html">
type context' = HtmlProvider<"../data/HouseSearchSample02.html">

type SearchCriteria = {streetTemplateValue:string;
                        streetName:string;
                        streetNumber:string;}

type SearchCriteria' = {catchmentCode:string;
                        streetName:string;
                        streetTemplateValue:string;
                        streetNumber:string;
                        streetZipCode:string}

let uri = "http://wwwgis2.wcpss.net/addressLookup/index.php"

let composeStreetNameFromIndividualComponents(street:string, suffix:string, city:string) =
    street + "+" + suffix + "+" + city

let createTitleCase(s:string)=
    let chars = s.ToCharArray()
    let firstLetter = chars.[0..0] |> Array.map(fun c -> System.Char.ToUpper(c))
    let remainingLetters = chars.[1..] |> Array.map(fun c -> System.Char.ToLower(c))
    let chars' = Array.append firstLetter remainingLetters
    System.String.Concat(chars')


let createPage1QueryString(searchCriteria:SearchCriteria)=
    let streetTemplateValue' = createTitleCase(searchCriteria.streetTemplateValue)
    let words = searchCriteria.streetName.Split('+')
    let words' = words |> Array.map(fun w -> createTitleCase(w))
    let streetName' = words' |> String.concat("+")
    let stringBuilder = new StringBuilder()
    stringBuilder.Append("StreetTemplateValue=") |> ignore
    stringBuilder.Append(streetTemplateValue') |> ignore
    stringBuilder.Append("&StreetName=") |> ignore
    stringBuilder.Append(streetName') |> ignore
    stringBuilder.Append("&StreetNumber=") |> ignore
    stringBuilder.Append(searchCriteria.streetNumber) |> ignore
    stringBuilder.Append("&SubmitAddressSelectPage=CONTINUE") |> ignore
    stringBuilder.Append("&DefaultAction=SubmitAddressSelectPage") |> ignore
    stringBuilder.ToString()

//let searchCriteria = {SearchCriteria.streetTemplateValue="WAKE";streetName="WAKE+FOREST+RD+RALEIGH";streetNumber="1506"}
//let queryString = createPage1QueryString(searchCriteria)
//queryString

let getServerGeneratedParameters(queryString:string)=
    try
        use webClient = new WebClient()
        webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded")
        let result = webClient.UploadString(uri,"POST",queryString)
        let body = context.Parse(result).Html.Body()
        let inputs = body.Descendants("INPUT") |> Seq.toList
        let inputs' = inputs |> Seq.map(fun i -> i.Attribute("name").Value(),i.Attribute("value").Value())
        let getValueFromInput(nameToFind:string) =
            inputs' |> Seq.filter(fun (n,v) -> n = nameToFind) 
                                        |> Seq.map(fun (n,v) -> v)
                                        |> Seq.head
        let catchmentCode = getValueFromInput("CatchmentCode") 
        let catchmentCode' = catchmentCode.Replace(" ","+")
        let streetZipCode = getValueFromInput("StreetZipCode")
        let result' = catchmentCode', streetZipCode
        Some result'
    with
          | :? System.ArgumentException ->  None
          | _ -> None

//let searchCriteria = {SearchCriteria.streetTemplateValue="WAKE";streetName="WAKE+FOREST+RD+RALEIGH";streetNumber="1506"}
//let queryString = createPage1QueryString(searchCriteria)
//let result= getServerGeneratedParameters(queryString)
//result

let createSearchCriteria' (searchCriteria:option<SearchCriteria>) =
    match searchCriteria.IsSome with
    | true -> 
        let page1QueryString = createPage1QueryString(searchCriteria.Value)
        let serverParameters = getServerGeneratedParameters(page1QueryString)
        match serverParameters.IsSome with
        | true -> 
            Some {catchmentCode=fst serverParameters.Value;
            streetName=searchCriteria.Value.streetName;
            streetTemplateValue=searchCriteria.Value.streetTemplateValue;
            streetNumber=searchCriteria.Value.streetNumber;
            streetZipCode=snd serverParameters.Value}
        | false -> None
    | false -> None

let createPage2QueryString(searchCriteria:option<SearchCriteria'>)=
    match searchCriteria.IsSome with
    | true ->
        let stringBuilder = new StringBuilder()
        stringBuilder.Append("SelectAssignment%7C2014%7CCURRENT=2014-15") |> ignore
        stringBuilder.Append("&DefaultAction=SelectAssignment%7C2014%7CCURRENT") |> ignore
        stringBuilder.Append("&DefaultAction=SelectAssignment%7C2015%7CCURRENT") |> ignore
        stringBuilder.Append("&CatchmentCode=") |> ignore
        stringBuilder.Append(searchCriteria.Value.catchmentCode) |> ignore
        stringBuilder.Append("&StreetName=") |> ignore
        stringBuilder.Append(searchCriteria.Value.streetName) |> ignore
        stringBuilder.Append("&StreetTemplateValue=") |> ignore
        stringBuilder.Append(searchCriteria.Value.streetTemplateValue) |> ignore
        stringBuilder.Append("&StreetNumber=") |> ignore
        stringBuilder.Append(searchCriteria.Value.streetNumber) |> ignore
        stringBuilder.Append("&StreetZipCode=") |> ignore
        stringBuilder.Append(searchCriteria.Value.streetZipCode) |> ignore
        let result = stringBuilder.ToString()
        Some result
    | false -> None    

let getSchoolData(queryString:option<string>) =
    match queryString.IsSome with
    | true ->
        try
            use webClient = new WebClient()
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded")
            let result = webClient.UploadString(uri,"POST",queryString.Value)
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
            let schoolData'''' = schoolData''' |> Seq.filter(fun s -> containsUnimportantPhrase(s) = false )
            Some schoolData'''' 
        with
              | :? System.Net.WebException ->  None
              | _ -> None
        
    | false -> None

let writeSchoolDataToDisk(schoolData: Option<string>) =
    match schoolData.IsSome with
    | true -> 
        File.AppendAllText(@"C:\Data\assignmentData.json",schoolData.Value.ToString() + "," )
    | false -> ()

//let streetName = composeStreetNameFromIndividualComponents("Strathorn","Dr","Cary")
//let searchCriteria = {SearchCriteria.streetTemplateValue="STRATH";streetName=streetName;streetNumber="904"}
//let result = createSearchCriteria'(searchCriteria)
//                |> createPage2QueryString
//                |> getSchoolData
//result.Value
