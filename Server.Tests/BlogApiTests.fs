module BlogApiTests

open Expecto
open LiteDB
open LiteDB.FSharp
open Shared
open System.IO
open Serilog

let pass() = Expect.isTrue true "passed"
let fail() = Expect.isTrue false "failed"
let failedWith msg = Expect.isTrue false msg

// creates a disposable in memory database
let useDatabase (f: LiteDatabase -> unit) : unit = 
    let mapper = FSharpBsonMapper()
    use memoryStream = new MemoryStream()
    use db = new LiteDatabase(memoryStream, mapper)
    f db
    
let blogApiTests = 
    testList "BlogApi tests" [
        testCase "Login with default credentials works" <| fun _ -> 
            useDatabase <| fun db -> 
                let logger = Serilog.Log.Logger 
                let testBlogApi = WebApp.createBlogApi logger db 
                let loginInfo = { Username = "guest"; Password = "guest" }
                let result = Async.RunSynchronously (testBlogApi.login loginInfo) 
                match result with 
                | LoginResult.Success token -> pass() 
                | _ -> fail()
    ]