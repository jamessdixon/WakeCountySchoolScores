
#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"

open System.IO
open FSharp.Data

type school  = {name:string; calendar:string; address:string; city:string; zipCode:int }

type elementaryContext = HtmlProvider<"../html/elementarySchools.html">
type middleContext = HtmlProvider<"../html/middleSchools.html">
type highContext = HtmlProvider<"../html/highSchools.html">

let elementaryRows = elementaryContext.Load("../html/elementarySchools.html").Tables.``Elementary Schools``.Rows
let middleRows = middleContext.Load("../html/middleSchools.html").Tables.``Middle Schools``.Rows
let highRows = highContext.Load("../html/highSchools.html").Tables.``High Schools``.Rows

let elemetarySchools = elementaryRows 
                        |> Seq.map(fun r -> {name=r.Name;calendar=r.Calendar;address=r.Address;city=r.City; zipCode=r.``ZIP Code``})
                        |> Seq.toArray
let middleSchools = middleRows 
                        |> Seq.map(fun r -> {name=r.Name;calendar=r.Calendar;address=r.Address;city=r.City; zipCode=r.``ZIP Code``})
                        |> Seq.toArray
let highSchools = highRows 
                        |> Seq.map(fun r -> {name=r.Name;calendar=r.Calendar;address=r.Address;city=r.City; zipCode=r.``ZIP Code``})
                        |> Seq.toArray

let schools = Array.append elemetarySchools middleSchools
let schools' = Array.append schools highSchools
schools' |> Seq.iter(fun s -> printfn "%A" s.name)

