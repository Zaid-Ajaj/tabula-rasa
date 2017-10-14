module Runner

open Expecto
open Expecto.Logging
open StorageTests

let testConfig =  
    { Expecto.Tests.defaultConfig with 
         parallelWorkers = 1
         verbosity = LogLevel.Debug }

let liteDbTests = 
    testList "All tests" [  
        storageTests
    ]


[<EntryPoint>]
let main argv = runTests testConfig liteDbTests