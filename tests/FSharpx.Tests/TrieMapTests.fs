﻿module FSharpx.DataStructures.Tests.TrieMapTests

open System
open FSharpx.DataStructures
open FSharpx.DataStructures.TrieMap
open FSharpx.DataStructures.Experimental
open FSharpx.DataStructures.Experimental.TrieMap_Packed
open NUnit.Framework
open FsUnit

// Tests predicated on the assumption that standard F# Map container is good, and is used as a reference.

type TwinMaps<'Key, 'T when 'Key : equality and 'Key : comparison> = {
    TrieMap : TrieMap<'Key, 'T>
    PTrieMap : TrieMap_Packed<'Key, 'T>
    Map : Map<'Key, 'T> }

let addToMaps key value twinMaps =
    {
      TrieMap = twinMaps.TrieMap |> TrieMap.add key value
      PTrieMap = twinMaps.PTrieMap |> TrieMap_Packed.add key value
      Map = twinMaps.Map |> Map.add key value }

let removeFromMaps key twinMaps =
    {
      TrieMap = twinMaps.TrieMap |> TrieMap.remove key 
      PTrieMap = twinMaps.PTrieMap |> TrieMap_Packed.remove key
      Map = twinMaps.Map |> Map.remove key }

let emptyMaps = { TrieMap = TrieMap.empty; PTrieMap = TrieMap_Packed.empty; Map = Map.empty }

type AssignedHashTestKey (keyValue : int, keyHash : int) =
    member this.GetKey() = keyValue
    override x.GetHashCode() = keyHash
    override x.Equals(yobj) = 
        match yobj with
        | :? AssignedHashTestKey as kv -> kv.GetKey() = keyValue
        | _ -> false

let mapsInSynch twinMaps =
    let matchCount =
        Seq.map2 (&&)
            (Seq.map2 (=) (twinMaps.TrieMap |> Seq.sortBy fst) (twinMaps.Map |> Map.toSeq |> Seq.sortBy fst))
            (Seq.map2 (=) (twinMaps.PTrieMap |> Seq.sortBy fst) (twinMaps.Map |> Map.toSeq |> Seq.sortBy fst))
         |> Seq.filter (fun x -> x) |> Seq.length
    let keys = twinMaps.Map |> Map.toSeq |> Seq.map fst
    let getMatchCount =
        keys
        |> Seq.map
            (fun key ->
                let fromRef = twinMaps.Map.[key]
                let fromTM = twinMaps.TrieMap.[key]
                let fromPTM = twinMaps.PTrieMap.[key]
                (fromRef = fromTM) && (fromRef = fromPTM))
        |> Seq.filter (fun x -> x) |> Seq.length
    (matchCount = twinMaps.Map.Count) && (matchCount = twinMaps.TrieMap.Count) && (matchCount = twinMaps.PTrieMap.Count) && (getMatchCount = twinMaps.Map.Count)


[<Test>]
let ``a big bunch of distinct Adds should result in contents stored properly``() =
    let entries = List.init 10000 (fun _ -> (Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
    let maps = entries |> List.fold (fun maps (k, v) -> addToMaps k v maps) emptyMaps
    maps |> mapsInSynch |> should equal true

