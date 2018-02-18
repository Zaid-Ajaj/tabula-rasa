module Program

open Suave
open Environment
open Fable.Remoting.Suave

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

    startWebServer webAppConfig webApp
    printfn "Hello from F#!"
    0 // return an integer exit code
