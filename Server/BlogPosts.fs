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
                 
let publishNewPost  (logger: ILogger) (database: LiteDatabase)  = 
   Security.authorize [ "admin" ] <| fun newBlogReq user ->
      let posts = database.GetCollection<BlogPost> "posts"
      try 
        match validatePost newBlogReq database with
        | Some validationError -> validationError 
        | None -> 
            let newPost = toBlogPost newBlogReq
            let result = posts.Insert(newPost)
            AddedPostId (Bson.deserializeField<int> result)
      with 
      | ex ->
        logger.Error(ex, "Error while publishing post {Data}", newBlogReq) 
        DatabaseErrorWhileAddingPost  
          
let saveAsDraft (logger: ILogger) (database: LiteDatabase) = 
    Security.authorizeAdmin <| fun newBlogReq user -> 
       let draft = { toBlogPost newBlogReq with IsDraft = true }
       let posts = database.GetCollection<BlogPost> "posts"
       match validatePost newBlogReq database with 
       | Some validationError -> validationError 
       | None -> 
       try 
        let result = posts.Insert(draft)
        let postId = Bson.deserializeField<int> result
        AddedPostId postId
       with 
       | ex ->
          logger.Error(ex, "Error while saving draft {Data}", newBlogReq) 
          DatabaseErrorWhileAddingPost

let deleteDraft (logger: ILogger) (db: LiteDatabase) = 
    Security.authorizeAdmin <| fun (postId: int) admin ->
        let posts = db.GetCollection<BlogPost> "posts"                         
        let postById = Query.EQ("_id", BsonValue(postId))
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
                logger.Error(ex, "Error while deleting draft {PostId}", postId) 
                DeleteDraftResult.DatabaseErrorWhileDeletingDraft

let deletePublishedArticle (db: LiteDatabase) = 
    Security.authorizeAdmin <| fun postId user ->
        let posts = db.GetCollection<BlogPost> "posts"   
        let foundPost = posts.tryFindOne <@ fun post -> post.Id = postId && not post.IsDraft @>        
        match foundPost with 
        | None -> PostDoesNotExist 
        | Some existingPost -> 
            let postById = Query.createQueryFromExpr<BlogPost> <@ fun post -> post.Id = existingPost.Id @>
            if posts.Delete(postById) > 0 then PostDeleted
            else DatabaseErrorWhileDeletingPost   

let publishDraft (db: LiteDatabase) = 
    Security.authorizeAdmin <| fun postId user ->
        let posts = db.GetCollection<BlogPost> "posts"   
        let post = posts.tryFindOne <@ fun post -> post.Id = postId && post.IsDraft @>          
        match post with 
        | None -> DraftDoesNotExist 
        | Some draft ->
            let modifiedDraft = { draft with IsDraft = false } 
            if posts.Update(modifiedDraft) then DraftPublished 
            else DatabaseErrorWhilePublishingDraft

let turnArticleToDraft (db: LiteDatabase) = 
    Security.authorize [ "admin" ] <| fun postId user -> 
        let posts = db.GetCollection<BlogPost> "posts"
        let foundPost = posts.tryFindOne <@ fun post -> post.Id = postId && not post.IsDraft @>                         
        match foundPost with 
        | None -> MakeDraftResult.ArticleDoesNotExist 
        | Some post ->
            let postAsDraft = { post with IsDraft = true } 
            if posts.Update(postAsDraft) then ArticleTurnedToDraft 
            else MakeDraftResult.DatabaseErrorWhileMakingDraft 
            
let savePostChanges (db: LiteDatabase) = 
    Security.authorize [ "admin" ] <| fun (postItem: BlogPostItem) user ->
        let posts = db.GetCollection<BlogPost> "posts"
        match posts.tryFindOne <@ fun post -> post.Id = postItem.Id @> with 
        | None -> Error "Could not find the post"
        | Some blogPost -> 
            let updatedBlogPost = 
                { blogPost with
                    Title = postItem.Title
                    Slug = postItem.Slug 
                    Content = postItem.Content
                    Tags = postItem.Tags  }
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

let togglePostFeatured (db: LiteDatabase) = 
    Security.authorize [ "admin" ] <| fun postId admin -> 
        let posts = db.GetCollection<BlogPost> "posts"
        match posts.tryFindOne <@ fun post -> post.Id = postId @> with 
        | None -> Error "Blog post could not be found"
        | Some post -> 
            let modifiedPost = { post with IsFeatured = not post.IsFeatured }
            if posts.Update modifiedPost 
            then Ok "Post was successfully updated" 
            else Error "Error occured while updating the blog post"

let getAllDrafts (database: LiteDatabase) = 
    Security.authorizeAny <| fun user -> 
       let posts = database.GetCollection<BlogPost> "posts"
       posts.findMany <@ fun post -> post.IsDraft @>
       |> List.ofSeq
       |> List.map toBlogPostItem

let getPostById (database: LiteDatabase) = 
    Security.authorize [ "admin" ] <| fun postId user -> 
       let posts = database.GetCollection<BlogPost> "posts"
       posts.tryFindOne <@ fun post -> post.Id = postId @>
       |> Option.map toBlogPostItem

let getPostBySlug (database: LiteDatabase) (slug: string) =
   let posts = database.GetCollection<BlogPost> "posts"
   posts.tryFindOne <@ fun post -> post.Slug = slug && not post.IsDraft @>
   |> Option.map toBlogPostItem