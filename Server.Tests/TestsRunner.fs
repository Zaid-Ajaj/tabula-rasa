module Runner

open Expecto
open Expecto.Logging
open StorageTests
open SecurityTests
let testConfig =  
    { Expecto.Tests.defaultConfig with 
         parallelWorkers = 1
         verbosity = LogLevel.Debug }

let liteDbTests = 
    testList "All tests" [  
        storageTests
        securityTests
    ]


[<EntryPoint>]
let main argv = runTests testConfig liteDbTests