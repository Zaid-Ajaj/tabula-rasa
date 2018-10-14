module StorageTests

open Expecto
open LiteDB
open LiteDB.FSharp
open Shared
open System.IO
open Serilog

let pass() = Expect.isTrue true "passed"
let fail() = Expect.isTrue false "failed"
let failedWith msg = Expect.isTrue false msg


let withDatabase (f: LiteDatabase -> unit) = 
  let mapper = FSharpBsonMapper()
  use memory = new MemoryStream()
  use database = new LiteDatabase(memory, mapper)
  f database  
  
let storageTests = 
    testList "Storage tests" [
        testCase "saveFile and readFile work from the database" <| fun _ -> 
          withDatabase <| fun database ->
            let content = "hello there"
            Storage.saveFile "hello.txt" content database |> ignore
            match Storage.readFile "hello.txt" database  with
            | Some "hello there" -> pass()
            | otherwise -> fail()
            match Storage.readFile "non-existent.txt" database with 
            | None -> pass()
            | otherwise -> fail()
        ]