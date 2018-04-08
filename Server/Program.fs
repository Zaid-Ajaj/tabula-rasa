module Program

open Suave
open Suave.Operators
open Suave.Filters
open Suave.Successful
open Environment
open Fable.Remoting.Suave
open Suave.RequestErrors

FableSuaveAdapter.logger <- Some (printfn "%s")

[<EntryPoint>]
let main argv =

    let store = 
        match argv with
        | [| "--store"; "local-database" |] -> Storage.LocalDatabase
        | [| "--store"; "in-memory" |] -> Storage.InMemory
        | otherwise -> Storage.InMemory // by default

    let webApp = WebApp.createUsing store

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

    startWebServer webAppConfig webApp
    printfn "Hello from F#!"
    0 // return an integer exit code
