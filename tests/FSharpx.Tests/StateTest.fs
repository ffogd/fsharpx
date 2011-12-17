﻿module FSharpx.Tests.StateTest

open FSharpx.State
open NUnit.Framework
open FsUnit

// Simple example
let tick = state_ {
  let! n = getState
  do! putState (n + 1)
  return n }

[<Test>]
let ``When starting a ticker at 0, it should have a state of 0``() =
  let actual = tick 0
  fst actual |> should equal 0

[<Test>]
let ``When starting a ticker at 0 and ticking twice, it should have a state of 2``() =
  let test = state_ {
    let! _ = tick
    let! _ = tick
    return () }
  let actual = exec test 0
  actual |> should equal 2

// Stack example
let enqueue a = fun s -> ((), s @ a::[])
let dequeue = fun (hd::tl) -> (hd, tl)

let workflow = state_ {
  let! queue = getState
  do! enqueue 4
  let! hd = dequeue
  do! enqueue (hd * 3)
  return hd }

[<Test>]
let ``When running the workflow, it should return 4``() =
  eval workflow [] |> should equal 4
