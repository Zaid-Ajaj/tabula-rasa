module StorageTypes

open System

type Store =
    | InMemory // using LiteDb's in-memory structure
    | LocalDatabase

// although we only need one admin for the whole app, 
// putting the admin info inside a seperate collection in the database
// makes for consistent data access mechanism 
[<CLIMutable>]
type AdminInfo =
    { Id : int
      BlogTitle : string
      Name : string
      Username : string
      PasswordHash : string
      PasswordSalt : string
      Email : string
      About : string
      Bio : string
      ProfileImageUrl : string }

/// How a blogpost is represented in the database
[<CLIMutable>]
type BlogPost =
    { Id : int
      Title : string
      Content : string
      IsDraft : bool
      IsFeatured : bool
      Slug : string
      Tags : string list
      DateAdded : DateTime }
