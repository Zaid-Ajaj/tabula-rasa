module BlogPosts

open System
open LiteDB
open LiteDB.FSharp
open LiteDB.FSharp.Extensions
open Shared

open StorageTypes

let toBlogPost (req : NewBlogPostReq) = 
    {  Id = 0 // by default, it will be auto incremented
       Title = req.Title
       Content = req.Content
       Slug = req.Slug
       Tags = req.Tags
       DateAdded = DateTime.Now
       IsDraft = false
       IsFeatured = false }

let validatePost (req: NewBlogPostReq) (db : LiteDatabase) = 
    let posts = db.GetCollection<BlogPost> "posts"
    let noDraft = Query.EQ("IsDraft", BsonValue(false))
    let byTitle = Query.EQ("Title", BsonValue(req.Title)) 
    posts.Find(Query.And(noDraft, byTitle)) 
    |> Seq.tryHead
    |> function 
        | Some _ -> Some PostWithSameTitleAlreadyExists 
        | None -> 
            let bySlug = Query.EQ("Slug", BsonValue(req.Slug)) 
            posts.Find(Query.And(noDraft, bySlug)) 
            |> Seq.tryHead 
            |> function 
                | Some _ -> Some PostWithSameSlugAlreadyExists
                | None -> None 
                 
let publishPost (db : LiteDatabase) (req: NewBlogPostReq)  = 
  let posts = db.GetCollection<BlogPost> "posts"
  try 
    match validatePost req db with
    | Some validationError -> validationError 
    | None -> 
        let newPost = toBlogPost req
        let result = posts.Insert(newPost)
        AddedPostId (Bson.deserializeField<int> result)
  with 
  | _ -> DatabaseErrorWhileAddingPost
      
let publishNewPost (database: LiteDatabase) (req: SecureRequest<NewBlogPostReq>)  = 
   match Security.validateJwt req.Token with
   | None -> AddPostResult.AuthError UserUnauthorized
   | Some user -> publishPost database req.Body    
          
let saveAsDraft (database: LiteDatabase) (req: SecureRequest<NewBlogPostReq>) = 
   match Security.validateJwt req.Token with
   | None -> AddPostResult.AuthError UserUnauthorized
   | Some user ->
       let draft = { toBlogPost req.Body with IsDraft = true }
       let posts = database.GetCollection<BlogPost> "posts"
       match validatePost req.Body database with 
       | Some validationError -> validationError 
       | None -> 
       try 
        let result = posts.Insert(draft)
        let postId = Bson.deserializeField<int> result
        AddedPostId postId
       with 
       | ex -> DatabaseErrorWhileAddingPost

let deleteDraft (db: LiteDatabase) (draftReq: SecureRequest<int>) = 
    match Security.validateJwt draftReq.Token with 
    | None -> DeleteDraftResult.AuthError UserUnauthorized
    | Some user -> 
        let posts = db.GetCollection<BlogPost> "posts"                         
        let postById = Query.EQ("_id", BsonValue(draftReq.Body))
        let postIsDraft = Query.EQ("IsDraft", BsonValue(true))
        let draftById = Query.And(postById, postIsDraft)
        match posts.TryFind(draftById) with 
        | None -> DeleteDraftResult.DraftDoesNotExist 
        | Some _ -> 
            if posts.Delete(postById) > 0 then DraftDeleted
            else DeleteDraftResult.DatabaseErrorWhileDeletingDraft

let deletePublishedArticle (db: LiteDatabase) (draftReq: SecureRequest<int>) = 
    match Security.validateJwt draftReq.Token with 
    | None -> DeleteArticleResult.AuthError UserUnauthorized
    | Some user -> 
        let posts = db.GetCollection<BlogPost> "posts"                         
        let postById = Query.EQ("_id", BsonValue(draftReq.Body))
        let postIsNotDraft = Query.EQ("IsDraft", BsonValue(false))
        let draftById = Query.And(postById, postIsNotDraft)
        match posts.TryFind(draftById) with 
        | None -> DeleteArticleResult.ArticleDoesNotExist 
        | Some _ -> 
            if posts.Delete(postById) > 0 then DeleteArticleResult.ArticleDeleted
            else DeleteArticleResult.DatabaseErrorWhileDeletingArticle   

let publishDraft (db: LiteDatabase) (draftReq: SecureRequest<int>) = 
    match Security.validateJwt draftReq.Token with 
    | None -> PublishDraftResult.AuthError UserUnauthorized
    | Some user -> 
        let posts = db.GetCollection<BlogPost> "posts"                         
        let postById = Query.EQ("_id", BsonValue(draftReq.Body))
        let postIsDraft = Query.EQ("IsDraft", BsonValue(true))
        let draftById = Query.And(postById, postIsDraft)
        match posts.TryFind(draftById) with 
        | None -> PublishDraftResult.DraftDoesNotExist 
        | Some draft ->
            let post = { draft with IsDraft = false } 
            if posts.Update(post) then DraftPublished 
            else PublishDraftResult.DatabaseErrorWhilePublishingDraft

let turnArticleToDraft (db: LiteDatabase) (req: SecureRequest<int>) = 
    match Security.validateJwt req.Token with 
    | None -> MakeDraftResult.AuthError UserUnauthorized
    | Some user -> 
        let posts = db.GetCollection<BlogPost> "posts"                         
        let postById = Query.EQ("_id", BsonValue(req.Body))
        let postIsPublished = Query.EQ("IsDraft", BsonValue(false))
        let draftById = Query.And(postById, postIsPublished)
        match posts.TryFind(draftById) with 
        | None -> MakeDraftResult.ArticleDoesNotExist 
        | Some draft ->
            let post = { draft with IsDraft = true } 
            if posts.Update(post) then ArticleTurnedToDraft 
            else MakeDraftResult.DatabaseErrorWhileMakingDraft 


let toBlogPostItem (post: BlogPost) = 
    { Id = post.Id; 
      Title = post.Title; 
      Slug = post.Slug;
      Content = post.Content;
      Featured = post.IsFeatured;
      DateAdded = post.DateAdded;
      Tags = post.Tags }
        
let getPublishedArticles (database: LiteDatabase) : list<BlogPostItem> = 
    let posts = database.GetCollection<BlogPost> "posts"
    let notDraft = Query.EQ("IsDraft", BsonValue(false))
    posts.Find(notDraft)
    |> Seq.map toBlogPostItem
    |> List.ofSeq

let getAllDrafts (database: LiteDatabase) (AuthToken(token)) = 
    match Security.validateJwt token with 
    | None -> Error "User unauthorized"
    | Some user -> 
        let posts = database.GetCollection<BlogPost> "posts"
        let postsThatAreDrafts = Query.EQ("IsDraft", BsonValue(true))
        posts.Find(postsThatAreDrafts)
        |> Seq.map toBlogPostItem
        |> List.ofSeq
        |> Ok
     
let getPostBySlug (database: LiteDatabase) (slug: string) =
   let posts = database.GetCollection<BlogPost> "posts"
   let bySlug = Query.EQ("Slug", BsonValue(slug))
   let notDraft = Query.EQ("IsDraft", BsonValue(false))
   posts.TryFind(Query.And(notDraft, bySlug))
   |> Option.map toBlogPostItem