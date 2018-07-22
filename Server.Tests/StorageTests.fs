module StorageTests

open Expecto
open LiteDB
open LiteDB.FSharp
open Shared
open System.IO
open Serilog

let pass() = Expect.isTrue true "passed"
let fail() = Expect.isTrue false "failed"
let failedWith msg = Expect.isTrue false msg


let withDatabase (f: LiteDatabase -> unit) = 
  let mapper = FSharpBsonMapper()
  use memory = new MemoryStream()
  use database = new LiteDatabase(memory, mapper)
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
              { Title = "title"; 
                Slug = "slug"; 
                Tags = []; 
                Content = "irrelevant" }
            
            match BlogPosts.publishPost Log.Logger database blogPostReq with 
            | AddedPostId _ -> 
              // post added, now add again     
              let sameBlogDifferentSlug = { blogPostReq with Slug = "something" } 
              match BlogPosts.publishPost Log.Logger database sameBlogDifferentSlug  with
              | AddedPostId _ -> failedWith "Shouldn't add a new post with the same title" 
              | AddPostResult.PostWithSameTitleAlreadyExists -> 
                  // then this is the correct error
                  // now check the slug 
                  let sameSlugWithDifferentTitle = { blogPostReq with Title = "something" }
                  match BlogPosts.publishPost Log.Logger database sameSlugWithDifferentTitle   with
                  | AddedPostId _ -> failedWith "Shouldn't add a new post with the same slug"
                  | AddPostResult.PostWithSameSlugAlreadyExists -> pass() 
                  | otherwise -> failwithf "Unexpected result: %A" otherwise 
              | otherwise -> failwithf "Unexpected result: %A" otherwise  
            | otherwise -> failwithf "Unexpected result: %A" otherwise 
        ]