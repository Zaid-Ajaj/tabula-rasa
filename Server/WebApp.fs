module WebApp

open Shared
open LiteDB
open Fable.Remoting.Server
open Fable.Remoting.Suave
open Serilog
open Suave.SerilogExtensions

let liftAsync x = async { return x }

/// Composition root of the application
let createBlogApi (logger: ILogger) (database: LiteDatabase) : IBlogApi = 
     // create initial admin guest admin if one does not exists
    Admin.writeAdminIfDoesNotExists database Admin.guestAdmin 
    let getBlogInfo() = async { return Admin.blogInfo database }
    let getPosts() = async { return BlogPosts.getPublishedArticles database } 
    let blogApi : IBlogApi = {   
        getBlogInfo = getBlogInfo
        getPosts = getPosts 
        login = Admin.login logger database >> liftAsync
        publishNewPost = BlogPosts.publishNewPost logger database >> liftAsync
        getPostBySlug =  BlogPosts.getPostBySlug database >> liftAsync 
        savePostAsDraft = BlogPosts.saveAsDraft logger database >> liftAsync
        getDrafts = BlogPosts.getAllDrafts database >> liftAsync
        deleteDraftById = BlogPosts.deleteDraft logger database >> liftAsync 
        publishDraft = BlogPosts.publishDraft database >> liftAsync
        deletePublishedArticleById = BlogPosts.deletePublishedArticle database >> liftAsync
        turnArticleToDraft = BlogPosts.turnArticleToDraft database >> liftAsync
        getPostById = BlogPosts.getPostById database >> liftAsync
        savePostChanges = BlogPosts.savePostChanges database >> liftAsync
        updateBlogInfo = Admin.updateBlogInfo database >> liftAsync
        togglePostFeauted = BlogPosts.togglePostFeatured database >> liftAsync 
    }

    blogApi

open Suave.Http
open System 

// Log unhandled exceptions 
let errorHandler (ex: Exception)  
                 (routeInfo: RouteInfo<HttpContext>) =
    // get a contextual logger with RequestId attached to it
    let contextLogger = routeInfo.httpContext.Logger()
    // log the exception with relevant data
    let errorMsgTemplate = "Error occured while invoking {MethodName} at {RoutePath}"
    contextLogger.Error(ex, errorMsgTemplate, routeInfo.methodName, routeInfo.path)
    // No need to propagate custom errors back to client
    Ignore 


/// Creates a WebPart from the BlogApi protocol
let createUsing storeType = 
    let database = Storage.createDatabaseUsing storeType
    Remoting.createApi()
    |> Remoting.fromContext (fun ctx -> createBlogApi (ctx.Logger()) database) 
    |> Remoting.withRouteBuilder routes 
    |> Remoting.withDiagnosticsLogger (printfn "%s")
    |> Remoting.withErrorHandler errorHandler
    |> Remoting.buildWebPart 
    |> SerilogAdapter.Enable
