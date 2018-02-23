module BlogPosts

open System
open LiteDB
open LiteDB.FSharp
open LiteDB.FSharp.Extensions
open Shared.DomainModels
open Shared.ViewModels

/// How a blogpost is represented in the database
[<CLIMutable>]
type BlogPost = {
    Id: int
    Title: string
    Content: string
    IsDraft: bool
    IsFeatured: bool
    Slug: string
    Tags : string list
    DateAdded: DateTime
    Comments: Comment list
    Reactions: (SocialReaction * int) list
}

let create (req: NewBlogPostReq) (db : LiteDatabase) = 
    let newPost : BlogPost = {
         Id = 0 // by default, it will be auto incremented
         Title = req.Title
         Content = req.Content
         Slug = req.Slug
         Tags = req.Tags
         DateAdded = DateTime.Now
         Reactions = []
         Comments = []
         IsDraft = false
         IsFeatured = false
      } 
    
    let posts = db.GetCollection<BlogPost> "posts"
    try 
      let result : BsonValue = posts.Insert(newPost)
      Some (Bson.deserializeField<int> result)
    with 
    | _ -> None
        
let publishNewPost (req: SecureRequest<NewBlogPostReq>) (database: LiteDatabase)   = 
   match Security.validateJwt req.Token with
   | None -> Error "Authorization token was invalid"
   | Some user ->
      match create req.Body database with
      | None -> Error "Could not add the new blog post to the database"
      | Some id -> Ok id                 
    
let toBlogPostItem (post: BlogPost) = 
    { Id = post.Id; 
      Title = post.Title; 
      Slug = post.Slug;
      Content = post.Content;
      Featured = post.IsFeatured;
      DateAdded = post.DateAdded }
        
let getAll (database: LiteDatabase) : list<BlogPostItem> = 
    let posts = database.GetCollection<BlogPost> "posts"
    posts.FindAll()
    |> Seq.map toBlogPostItem
    |> List.ofSeq
     
let getPostBySlug (database: LiteDatabase) (slug: string) =
   let posts = database.GetCollection<BlogPost> "posts"
   let query = Query.EQ("Slug", BsonValue(slug))
   posts.Find(query)
   |> Seq.tryHead
   |> Option.map toBlogPostItem