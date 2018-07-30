module Runner

open Expecto
open Expecto.Logging
open StorageTests
open SecurityTests
open BlogApiTests 

let testConfig =  
    { Expecto.Tests.defaultConfig with 
         parallelWorkers = 1
         verbosity = LogLevel.Debug }

let liteDbTests = 
    testList "All tests" [  
        storageTests
        securityTests
        blogApiTests
    ]


[<EntryPoint>]
let main argv = runTests testConfig liteDbTests