module Storage

open Shared.DomainModels
open LiteDB
open LiteDB.FSharp

open System
open System.IO
open System.Text

type AppUser = {
    Id: Guid
    Username: string
    PasswordHash: string
    PasswordSalt: string
    DateAdded: DateTime
    IsAdmin: bool
}

type Store = 
    | InMemory // using LiteDb's in-memory structure
    | LocalDatabase

let saveFile (filename: string) (content: string) (database: LiteDatabase) = 
    let content = if isNull content then "" else content
    let contentAsBytes = Encoding.UTF8.GetBytes(content)
    use memoryStream = new MemoryStream(contentAsBytes)
    database.FileStorage.Upload(filename, filename, memoryStream) |> ignore

let readFile (filename: string) (database: LiteDatabase) = 
    use memoryStream = new MemoryStream()
    try 
        database.FileStorage.Download(filename, memoryStream) |> ignore
        Encoding.UTF8.GetString(memoryStream.ToArray()) |> Some
    with | ex -> None

let createDatabaseUsing store = 
    let mapper = FSharpBsonMapper()
    match store with
    | InMemory ->
        let memoryStream = new System.IO.MemoryStream()
        new LiteDatabase(memoryStream, mapper)
    | LocalDatabase ->
        let dbFile = Environment.databaseFilePath
        new LiteDatabase(dbFile, mapper)