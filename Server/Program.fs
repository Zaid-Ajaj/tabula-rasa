open System

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

open System.IO
open System.Reflection
open System.Text

open Shared.ViewModels
open ClientServer
open Security

open Fable.Remoting.Suave

[<EntryPoint>]
let main argv =

    let store = 
        match argv with
        | [| "--store"; "local-database" |] -> Storage.LocalDatabase
        | [| "--store"; "in-memory" |] -> Storage.InMemory
        | otherwise -> Storage.InMemory // by default

    let webApp = WebApp.createUsing store

    let client = Environment.clientPath
    let webServerConfig = 
        { defaultConfig with 
            homeFolder = Some client }

    let webServer = FableSuaveAdapter.webPartWithBuilderFor webApp routeBuilder

    startWebServer webServerConfig webServer
    printfn "Hello from F#!"
    0 // return an integer exit code
