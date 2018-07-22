module BlogPosts

open System
open LiteDB
open LiteDB.FSharp
open LiteDB.FSharp.Extensions
open Shared
open Serilog
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
    let foundPostByTitle = posts.tryFindOne <@ fun post -> post.Title = req.Title && not post.IsDraft @> 
    match foundPostByTitle with 
    | Some _ -> Some PostWithSameTitleAlreadyExists 
    | None -> 
        let foundPostBySlug = posts.tryFindOne <@ fun post -> post.Slug = req.Slug && not post.IsDraft @>
        match foundPostBySlug with  
        | Some _ -> Some PostWithSameSlugAlreadyExists
        | None -> None 
                 
let publishPost (logger: ILogger) (db : LiteDatabase) (req: NewBlogPostReq)  = 
  let posts = db.GetCollection<BlogPost> "posts"
  try 
    match validatePost req db with
    | Some validationError -> validationError 
    | None -> 
        let newPost = toBlogPost req
        let result = posts.Insert(newPost)
        AddedPostId (Bson.deserializeField<int> result)
  with 
  | ex ->
    logger.Error(ex, "Error while publishing post {Data}", req) 
    DatabaseErrorWhileAddingPost
      
let publishNewPost  (logger: ILogger) (database: LiteDatabase) (req: SecureRequest<NewBlogPostReq>)  = 
   match Security.validateJwt req.Token with
   | None -> AddPostResult.AuthError UserUnauthorized
   | Some user -> publishPost logger database req.Body    
          
let saveAsDraft (logger: ILogger) (database: LiteDatabase) (req: SecureRequest<NewBlogPostReq>) = 
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
       | ex ->
          logger.Error(ex, "Error while saving draft {Data}", req) 
          DatabaseErrorWhileAddingPost

let deleteDraft (logger: ILogger) (db: LiteDatabase) (draftReq: SecureRequest<int>) = 
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
            try
              if posts.Delete(postById) > 0 then DraftDeleted
              else DeleteDraftResult.DatabaseErrorWhileDeletingDraft
            with 
            | ex -> 
                logger.Error(ex, "Error while deleting draft {Data}", draftReq) 
                DeleteDraftResult.DatabaseErrorWhileDeletingDraft

let deletePublishedArticle (db: LiteDatabase) (req: SecureRequest<int>) = 
    match Security.validateJwt req.Token with 
    | None -> DeletePostResult.AuthError UserUnauthorized
    | Some user -> 
        let posts = db.GetCollection<BlogPost> "posts"   
        let foundPost = posts.tryFindOne <@ fun post -> post.Id = req.Body && not post.IsDraft @>        
        match foundPost with 
        | None -> DeletePostResult.PostDoesNotExist 
        | Some existingPost -> 
            let postById = Query.createQueryFromExpr<BlogPost> <@ fun post -> post.Id = existingPost.Id @>
            if posts.Delete(postById) > 0 then DeletePostResult.PostDeleted
            else DeletePostResult.DatabaseErrorWhileDeletingPost   

let publishDraft (db: LiteDatabase) (draftReq: SecureRequest<int>) = 
    match Security.validateJwt draftReq.Token with 
    | None -> PublishDraftResult.AuthError UserUnauthorized
    | Some user -> 
        let posts = db.GetCollection<BlogPost> "posts"   
        let post = posts.tryFindOne <@ fun post -> post.Id = draftReq.Body && post.IsDraft @>          
        match post with 
        | None -> PublishDraftResult.DraftDoesNotExist 
        | Some draft ->
            let modifiedDraft = { draft with IsDraft = false } 
            if posts.Update(modifiedDraft) then DraftPublished 
            else PublishDraftResult.DatabaseErrorWhilePublishingDraft

let turnArticleToDraft (db: LiteDatabase) (req: SecureRequest<int>) = 
    match Security.validateJwt req.Token with 
    | None -> MakeDraftResult.AuthError UserUnauthorized
    | Some user -> 
        let posts = db.GetCollection<BlogPost> "posts"
        let foundPost = posts.tryFindOne <@ fun post -> post.Id = req.Body && not post.IsDraft @>                         
        match foundPost with 
        | None -> MakeDraftResult.ArticleDoesNotExist 
        | Some post ->
            let postAsDraft = { post with IsDraft = true } 
            if posts.Update(postAsDraft) then ArticleTurnedToDraft 
            else MakeDraftResult.DatabaseErrorWhileMakingDraft 

let togglePostFeatured (db: LiteDatabase) (req: SecureRequest<int>) = 
    match Security.validateJwt req.Token with
    | None ->  
        Error "User unauthorized"
    | Some user when not (Array.contains "admin" user.Claims) -> 
        Error "User must be an admin"
    | Some admin -> 
        let posts = db.GetCollection<BlogPost> "posts"
        match posts.tryFindOne <@ fun post -> post.Id = req.Body @> with 
        | None -> Error "Blog post could not be found"
        | Some post -> 
            let modifiedPost = { post with IsFeatured = not post.IsFeatured }
            if posts.Update modifiedPost 
            then Ok "Post was successfully updated" 
            else Error "Error occured while updating the blog post"

let savePostChanges (db: LiteDatabase) (req: SecureRequest<BlogPostItem>) = 
    match Security.validateJwt req.Token with 
    | None -> Error "User unauthorized"
    | Some user -> 
        let posts = db.GetCollection<BlogPost> "posts"
        match posts.tryFindOne <@ fun post -> post.Id = req.Body.Id @> with 
        | None -> Error "Could not find the post"
        | Some blogPost -> 
            let updatedBlogPost = 
                { blogPost with
                    Title = req.Body.Title
                    Slug = req.Body.Slug 
                    Content = req.Body.Content
                    Tags = req.Body.Tags  }
            if posts.Update updatedBlogPost then Ok true 
            else Error "Error occured while updating the blog post"
            
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
    posts.findMany <@ fun post -> not post.IsDraft @>
    |> Seq.map toBlogPostItem
    |> List.ofSeq

let getAllDrafts (database: LiteDatabase) (SecurityToken(token)) = 
    match Security.validateJwt token with 
    | None -> Error "User unauthorized"
    | Some user when not (Array.contains "admin" user.Claims) -> 
        Error "User must be an admin"
    | Some user -> 
        let posts = database.GetCollection<BlogPost> "posts"
        posts.findMany <@ fun post -> post.IsDraft @>
        |> Seq.map toBlogPostItem
        |> List.ofSeq
        |> Ok

let getPostById (database: LiteDatabase)  (req: SecureRequest<int>) = 
    match Security.validateJwt req.Token with 
    | None -> Error "User unauthorized"
    | Some user -> 
        let posts = database.GetCollection<BlogPost> "posts"
        posts.tryFindOne <@ fun post -> post.Id = req.Body @>
        |> Option.map toBlogPostItem
        |> function 
            | None -> Error "Could not find the requested article"
            | Some article -> Ok article

let getPostBySlug (database: LiteDatabase) (slug: string) =
   let posts = database.GetCollection<BlogPost> "posts"
   posts.tryFindOne <@ fun post -> post.Slug = slug && not post.IsDraft @>
   |> Option.map toBlogPostItem