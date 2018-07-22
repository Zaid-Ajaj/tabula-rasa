module Program

open Suave
open Suave.Operators
open Suave.Filters

open Environment
open Fable.Remoting.Suave
open StorageTypes
open Suave.RequestErrors
open Serilog
open Suave.SerilogExtensions

[<EntryPoint>]
let main argv =

    let storageType = 
        match argv with
        | [| "--store"; "localdb" |] -> Store.LocalDatabase
        | [| "--store"; "in-memory" |] -> Store.InMemory
        | otherwise -> Store.LocalDatabase // by default

    let webApp = WebApp.createUsing storageType

    let clientPath = solutionRoot </> "dist" </> "client"
    printfn "Client directory: %s" clientPath

    let webAppConfig = 
        { defaultConfig with 
            homeFolder = Some clientPath }

    let webApp = 
        choose [
            GET >=> path "/" >=> Files.browseFileHome "index.html"
            Files.browseHome
            webApp
            NOT_FOUND "The resource you requested was not found"
        ]

    Log.Logger <- 
      LoggerConfiguration() 
        // Suave.SerilogExtensions has native destructuring mechanism
        // this helps Serilog deserialize the fsharp types like unions/records
        .Destructure.FSharpTypes()
        // use package Serilog.Sinks.Console  
        // https://github.com/serilog/serilog-sinks-console
        .WriteTo.Console() 
        // add more sinks etc.
        .CreateLogger()

    startWebServer webAppConfig webApp
    0
