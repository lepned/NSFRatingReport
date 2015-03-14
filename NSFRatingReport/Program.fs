open System.IO
open System
open FSharp.Data

[<Literal>]
let path = """c:\users\lepne_000\documents\visual studio 2013\Projects\NSFRatingReport\NSFRatingReport\NSF.txt"""

type Spiller = {Navn:string; Rating: Nullable<int>; Poeng: decimal ; StartNr : int; Gruppe: string; Resultater: string option list; Plassering:int}
type NSFReport = {Text:string; Aktivitet: string ; CombinedText:string}
type ExcelReport = CsvProvider<path>
let ssk = ExcelReport.Load(path)

let readInSpillere =    
    ssk.Rows
    |>Seq.mapi(fun idx row -> 
        let res = 
            [row.``1``;row.``2``;row.``3``;row.``4``;row.``5``;row.``6``;row.``7``;row.``8``;row.``9``;row.``10``;row.``11``;row.``12``]            
            |>List.map(fun res ->
                if res = "=" || res = "-" || res = "+wo" then None 
                else
                    let first,resten = res.[0], res.[1..]
                    Some (first.ToString() + "0" + resten))
        {Navn=row.Navn; Rating=row.ELO;Poeng=row.Poeng; StartNr=row.StartNr; Gruppe=row.Gr; Resultater=res;Plassering=idx+1})
    |>List.ofSeq

let opponents spiller =
    spiller.Resultater |> List.map(fun res ->         
        match res with
        |None -> None
        |Some txt ->
            let start, slutt = txt.[0], txt.[1..]
            let ok, nr = Int32.TryParse(slutt)
            if ok then Some (readInSpillere|>List.find(fun sp -> sp.StartNr=nr)) else None) 

let makeString (txt:String option) (motst:Spiller option) =
    match motst with
    |None -> "    "
    |Some sp ->
        let res, place = txt.Value.[0], sp.Plassering        
        if place < 10 then res.ToString() + "00" + place.ToString()
        else res.ToString() + "0" + place.ToString()

let mapResultsToString =
    readInSpillere
    |>List.map(fun s -> s, opponents s)
    |>List.map (fun (spiller,opponents) -> spiller, List.zip spiller.Resultater opponents)
    |>List.map (fun (sp, zippedlist) -> sp, zippedlist|>List.map (fun (txt, sp) -> makeString txt sp))
    |>List.map (fun (s, lst) -> s,lst|> List.fold (fun acc el -> acc + el) "")        
    |>List.fold (fun acc (sp,text) -> sprintf "%s %s %s %s%s " acc (sp.Plassering.ToString()) sp.Navn text Environment.NewLine) " "

let countAct =
    let start = (Environment.NewLine + "-----------------------------------------------" + Environment.NewLine)
    readInSpillere
    |>List.map (fun (s) -> s.Navn, s.Resultater |> List.sumBy (fun sp -> if sp.IsSome then 1 else 0))
    |>List.fold (fun acc (navn, ant) -> sprintf "%s %s -> %s %s" acc navn (ant.ToString()) Environment.NewLine) start

let report = { Text= mapResultsToString; Aktivitet=countAct; CombinedText = mapResultsToString + countAct}

File.WriteAllText("""c:\users\lepne_000\documents\EloRapport.txt""", report.CombinedText)

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    0 // return an integer exit code
