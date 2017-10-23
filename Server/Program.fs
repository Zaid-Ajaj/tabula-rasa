module Program

open Suave
open Environment

[<EntryPoint>]
let main argv =

    let store = 
        match argv with
        | [| "--store"; "local-database" |] -> Storage.LocalDatabase
        | [| "--store"; "in-memory" |] -> Storage.InMemory
        | otherwise -> Storage.InMemory // by default

    let webApp = WebApp.createUsing store

    let client = solutionRoot </> "dist" </> "client"
    printfn "Client directory: %s" client

    let webAppConfig = 
        { defaultConfig with 
            homeFolder = Some client }

    startWebServer webAppConfig webApp
    printfn "Hello from F#!"
    0 // return an integer exit code
