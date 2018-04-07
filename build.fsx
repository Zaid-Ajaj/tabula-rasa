#r "packages/build/FAKE/tools/FakeLib.dll"

open System
open Fake

let run fileName args workingDir =
    printfn "CWD: %s" workingDir
    let fileName, args =
        if EnvironmentHelper.isUnix
        then fileName, args else "cmd", ("/C " + fileName + " " + args)
    let ok =
        execProcess (fun info ->
            info.FileName <- fileName
            info.WorkingDirectory <- workingDir
            info.Arguments <- args) TimeSpan.MaxValue
    if not ok then failwith (sprintf "'%s> %s %s' task failed" workingDir fileName args)

let dotnet = "dotnet"
let projects =  [ "Server"; "Server.Tests"; "Client" </> "src" ]

Target "Clean" <| fun _ ->
    projects 
    |> List.collect (fun proj -> [ proj </> "bin"; proj </> "obj" ])
    |> List.iter CleanDir

Target "DotnetRestore" <| fun _ ->
    projects
    |> List.iter (run dotnet "restore --no-cache")

Target "ServerTests" <| fun _ ->
    run dotnet "run" "Server.Tests"


"Clean" 
  ==> "DotnetRestore"
  ==> "ServerTests"

RunTargetOrDefault "Clean"