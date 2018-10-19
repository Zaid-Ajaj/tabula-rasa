module Storage

open StorageTypes
open LiteDB
open LiteDB.FSharp
open System.IO
open System.Text

let saveFile (filename : string) (content : string) (database : LiteDatabase) =
    let content =
        if isNull content then ""
        else content
    
    let contentAsBytes = Encoding.UTF8.GetBytes(content)
    use memoryStream = new MemoryStream(contentAsBytes)
    database.FileStorage.Upload(filename, filename, memoryStream) |> ignore

let readFile (filename : string) (database : LiteDatabase) =
    use memoryStream = new MemoryStream()
    try 
        database.FileStorage.Download(filename, memoryStream) |> ignore
        Encoding.UTF8.GetString(memoryStream.ToArray()) |> Some
    with ex -> None

let createDatabaseUsing store =
    let mapper = FSharpBsonMapper()
    match store with
    | Store.InMemory -> 
        let memoryStream = new System.IO.MemoryStream()
        new LiteDatabase(memoryStream, mapper)
    | Store.LocalDatabase -> 
        let dbFile = Environment.databaseFilePath
        new LiteDatabase(dbFile, mapper)
