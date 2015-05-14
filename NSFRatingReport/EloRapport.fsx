#r @"C:\Users\lepne_000\Documents\visual studio 2013\Projects\NSFRatingReport\packages\FSharp.Data.2.1.1\lib\net40\FSharp.Data.dll"

open System
open FSharp.Data
open System.Globalization
open System.IO

type Player = 
    { Rank : int
      Navn : string
      Motstandere : string }

let source = __SOURCE_DIRECTORY__

[<Literal>]
let file = "C:\Users\lepne_000\Desktop\SjakkResCSV.csv"

type SjakkRes = CsvProvider<file, Encoding="ISO-8859-15">

let res = SjakkRes.Load(file)

let startRank = //lookup av rank
    res.Rows
    |> Seq.mapi (fun idx row -> row.StartNr, idx + 1)
    |> Map.ofSeq

let (|HarSpilt|IkkeSpilt|) (resultat:string) =
    match Int32.TryParse(resultat.Substring(1)) with
    |true, v -> 
        let res, rank = resultat.Substring(0, 1), startRank.[v] 
        let eloFormat = sprintf "%s%s%s" res (if rank < 10 then "00" else "0") (rank.ToString())
        HarSpilt eloFormat
    |_ -> IkkeSpilt "    "

let resultater = 
    res.Rows
    |> Seq.mapi (fun idx row -> 
           let runder = 
               [ row.``1``; row.``2``; row.``3``; row.``4``; row.``5``; row.``6``; row.``7``; row.``8``; 
                 row.``9``; row.``10``; row.``11``; row.``12`` ]
               |> List.map (function |HarSpilt v -> v | IkkeSpilt v -> v)
               |> String.concat ""
           { Rank = idx + 1
             Navn = row.Navn
             Motstandere = runder })

let writeToDisk path (players : Player seq) = 
    let arr = players |> Seq.map (fun player -> sprintf "%i %s %s" player.Rank player.Navn player.Motstandere)
    File.WriteAllLines(path, arr)

let path = Path.Combine(source, "Test.txt")

writeToDisk path resultater
