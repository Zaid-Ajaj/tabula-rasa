module StorageTests

open Expecto
open LiteDB
open LiteDB.FSharp
open Shared.ViewModels
open System.IO

let pass() = Expect.isTrue true "passed"
let fail() = Expect.isTrue false "failed"
let failedWith msg = Expect.isTrue false msg


let withDatabase (f: LiteDatabase -> unit) = 
  let mapper = FSharpBsonMapper()
  let memory = new MemoryStream()
  let database = new LiteDatabase(memory, mapper)
  f database  
  
let storageTests = 
    testList "Storage tests" [
        testCase "saveFile and readFile work from the database" <| fun _ -> 
          withDatabase <| fun database ->
            let content = "hello there"
            Storage.saveFile "hello.txt" content database |> ignore
            match Storage.readFile "hello.txt" database  with
            | Some "hello there" -> pass()
            | otherwise -> fail()
            match Storage.readFile "non-existent.txt" database with 
            | None -> pass()
            | otherwise -> fail()
            
        testCase "Error message is returned with trying to publish existing post" <| fun _ ->
          withDatabase <| fun database -> 
            let blogPostReq : NewBlogPostReq = 
              { Title = "title"; Slug = "slug"; Tags = []; Content = "irrelevant" }
            
            match BlogPosts.publishPost blogPostReq database with 
            | Error _ -> failedWith "Shouldn't get an error message just yet"
            | Ok id -> 
              // post added, now add again     
              let sameBlogDifferentSlug = { blogPostReq with Slug = "something" } 
              match BlogPosts.publishPost sameBlogDifferentSlug database with
              | Ok _ -> failedWith "Shouldn't add a new post with the same title" 
              | Error "A post with title 'title' already exists" -> 
                  // then this is the correct error
                  // now check the slug 
                  let sameSlugWithDifferentTitle = { blogPostReq with Title = "something" }
                  match BlogPosts.publishPost sameSlugWithDifferentTitle database with
                  | Ok _ -> failedWith "Shouldn't add a new post with the same slug"
                  | Error "A post with slug 'slug' already exists" -> pass() 
                  | Error otherError -> failwith "Shouldn't fail with now" 
              | Error otherError -> failwith "Shouldn't fail with now" 
        ]
       