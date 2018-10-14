module WebApp

open Shared
open LiteDB
open Fable.Remoting.Server
open Fable.Remoting.Suave
open Serilog
open Suave.SerilogExtensions
open Elmish.Bridge
open Elmish 

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
        publishNewPost = BlogPosts.publishNewPost logger database
        getPostBySlug =  BlogPosts.getPostBySlug database >> Async.result
        savePostAsDraft = BlogPosts.saveAsDraft logger database 
        getDrafts = BlogPosts.getAllDrafts database
        deleteDraftById = BlogPosts.deleteDraft logger database 
        publishDraft = BlogPosts.publishDraft database
        deletePublishedArticleById = BlogPosts.deletePublishedArticle database 
        turnArticleToDraft = BlogPosts.turnArticleToDraft database
        getPostById = BlogPosts.getPostById database 
        savePostChanges = BlogPosts.savePostChanges database
        updateBlogInfo = Admin.updateBlogInfo database
        togglePostFeatured = BlogPosts.togglePostFeatured database 
        updatePassword = Admin.updatePassword logger database
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


/// Creates a WebPart from the BlogApi protocol, then enable serilog on top of the web part
let createUsing storeType = 
    let database = Storage.createDatabaseUsing storeType
    Remoting.createApi()
    |> Remoting.fromContext (fun ctx -> createBlogApi (ctx.Logger()) database) 
    |> Remoting.withRouteBuilder routes 
    |> Remoting.withDiagnosticsLogger (printfn "%s")
    |> Remoting.withErrorHandler errorHandler
    |> Remoting.buildWebPart 
    |> SerilogAdapter.Enable

// server state is what the server keeps track of
type ServerState = Nothing 

// the server message is what the server reacts to
// in this case, it reacts to messages from client
type ServerMsg = ClientMsg of RemoteClientMsg 

// The postsHub keeps track of connected clients and has broadcasting logic
let postsHub = 
    ServerHub<ServerState, ServerMsg, RemoteServerMsg>() 
        .RegisterServer(ClientMsg) 

// react to messages coming from client
let update currentClientDispatch (ClientMsg clientMsg) currentState = 
    match clientMsg with 
    // when a post is added
    | PostAdded -> 
        // tell all clients to reload posts
        postsHub.BroadcastClient ReloadPosts
        currentState, Cmd.none 

// Don't do anything initially
let init (clientDispatch:Dispatch<RemoteServerMsg>) () = Nothing, Cmd.none 

// Construct the socketServer as a WebPart
let socketServer =
    Bridge.mkServer Shared.socket init update 
    |> Bridge.withConsoleTrace
    |> Bridge.withServerHub postsHub
    // register the types we can receive
    |> Bridge.register ClientMsg  
    |> Bridge.run Suave.server