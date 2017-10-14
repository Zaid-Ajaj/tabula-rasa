module StorageTests

open Expecto
open LiteDB
open LiteDB.FSharp
open System.IO

let pass() = Expect.isTrue true "passed"
let fail() = Expect.isTrue false "failed"

let mapper = FSharpBsonMapper()
let memory = new MemoryStream()
let database = new LiteDatabase(memory, mapper)

let storageTests = 
    testList "Storage tests" [
        testCase "saveFile and readFile work" <| fun _ -> 
           let content = "hello there"
           Storage.saveFile "hello.txt" content database |> ignore
           match Storage.readFile "hello.txt" database  with
           | Some "hello there" -> pass()
           | otherwise -> fail()
           match Storage.readFile "non-existent.txt" database with 
           | None -> pass()
           | otherwise -> fail()
    ]