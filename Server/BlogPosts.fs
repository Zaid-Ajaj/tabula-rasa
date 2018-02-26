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

let toBlogPost (req : NewBlogPostReq) = 
    {  Id = 0 // by default, it will be auto incremented
       Title = req.Title
       Content = req.Content
       Slug = req.Slug
       Tags = req.Tags
       DateAdded = DateTime.Now
       Reactions = []
       Comments = []
       IsDraft = false
       IsFeatured = false }

let postAlreadyPublished (req: NewBlogPostReq) (db : LiteDatabase) = 
    let posts = db.GetCollection<BlogPost> "posts"
    let noDraft = Query.EQ("IsDraft", BsonValue(false))
    let byTitle = Query.EQ("Title", BsonValue(req.Title)) 
    posts.Find(Query.And(noDraft, byTitle)) 
    |> Seq.tryHead
    |> function 
        | Some _ -> Some (sprintf "A post with title '%s' already exists" req.Title) 
        | None -> 
            let bySlug = Query.EQ("Slug", BsonValue(req.Slug)) 
            posts.Find(Query.And(noDraft, bySlug)) 
            |> Seq.tryHead 
            |> function 
                | Some _ -> Some (sprintf "A post with slug '%s' already exists" req.Slug)
                | None -> None 
                 
let publishPost (req: NewBlogPostReq) (db : LiteDatabase) = 
  let posts = db.GetCollection<BlogPost> "posts"
  try 
    match postAlreadyPublished req db with
    | Some errorMessage -> Error errorMessage 
    | None -> 
        let newPost = toBlogPost req
        let result : BsonValue = posts.Insert(newPost)
        Ok (Bson.deserializeField<int> result)
  with 
  | ex -> Error "Could not add the new blog post to the database"
      
let publishNewPost (req: SecureRequest<NewBlogPostReq>) (database: LiteDatabase) = 
   match Security.validateJwt req.Token with
   | None -> Error "Authorization token was invalid"
   | Some user -> publishPost req.Body database   
          
let saveAsDraft (req: SecureRequest<NewBlogPostReq>) (database: LiteDatabase) = 
   match Security.validateJwt req.Token with
   | None -> Error "Authorization token was invalid"
   | Some user ->
       let posts = database.GetCollection<BlogPost> "posts"
       let draft = { toBlogPost req.Body with IsDraft = true }
       try 
        let result : BsonValue = posts.Insert(draft)
        Ok (Bson.deserializeField<int> result) 
       with 
       | ex -> 
         // TODO: log ex 
         Error "Could not add the draft to the database"                   
    
let toBlogPostItem (post: BlogPost) = 
    { Id = post.Id; 
      Title = post.Title; 
      Slug = post.Slug;
      Content = post.Content;
      Featured = post.IsFeatured;
      DateAdded = post.DateAdded;
      Tags = post.Tags }
        
let getAll (database: LiteDatabase) : list<BlogPostItem> = 
    let posts = database.GetCollection<BlogPost> "posts"
    let notDraft = Query.EQ("IsDraft", BsonValue(false))
    posts.Find(notDraft)
    |> Seq.map toBlogPostItem
    |> List.ofSeq
     
let getPostBySlug (database: LiteDatabase) (slug: string) =
   let posts = database.GetCollection<BlogPost> "posts"
   let bySlug = Query.EQ("Slug", BsonValue(slug))
   let notDraft = Query.EQ("IsDraft", BsonValue(false))
   posts.Find(Query.And(notDraft, bySlug))
   |> Seq.tryHead
   |> Option.map toBlogPostItem