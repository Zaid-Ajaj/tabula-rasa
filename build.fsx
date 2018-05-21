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
let npm = "npm"
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

Target "NpmInstall" <| fun _ ->
    run npm "install" "Client"

Target "Watch" <| fun () ->
  [ async { run dotnet "watch run" "Server" }; 
    async { run dotnet "fable npm-run start" ("Client" </> "src") } ]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore

Target "WatchLocalDb" <| fun () ->
  [ async { run dotnet "watch run --store localdb" "Server" }; 
    async { run dotnet "fable npm-run start" ("Client" </> "src") } ]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore

Target "Release" <| fun _ ->
  CleanDir "dist"
  [ async { run dotnet "build --configuration Release --output ../dist" "Server" }
    async { 
        run dotnet "fable npm-run build" ("Client" </> "src") 
        CopyRecursive ("Client" </> "public") ("dist" </> "client") true |> ignore
    } ]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore

"Clean" 
  ==> "DotnetRestore"
  ==> "ServerTests"

"Clean" 
  ==> "NpmInstall"
  ==> "DotnetRestore"
  ==> "Watch"

"Clean" 
  ==> "NpmInstall"
  ==> "DotnetRestore"
  ==> "WatchLocalDb"

"Clean" 
  ==> "NpmInstall"
  ==> "DotnetRestore"
  ==> "Release"

RunTargetOrDefault "Clean"