open System

open Suave
open Suave.Files
open Suave.Operators
open System.IO
open System.Reflection

let (</>) x y = Path.Combine(x, y) 

let rec findRoot dir =
    if File.Exists(IO.Path.Combine(dir, "TabulaRasa.sln"))
    then dir
    else
        let parent = Directory.GetParent(dir)
        if isNull parent then
            failwith "Couldn't find package.json directory"
        findRoot parent.FullName

[<EntryPoint>]
let main argv =
    let cwd = Assembly.GetEntryAssembly().Location
    let root = findRoot cwd
    let client = root </> "Client" </> "src" </> "public"
    printfn "Hello %s from F#!" client
    0 // return an integer exit code
