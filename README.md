# Tabula Rasa [![Build Status](https://travis-ci.org/Zaid-Ajaj/tabula-rasa.svg?branch=master)](https://travis-ci.org/Zaid-Ajaj/tabula-rasa)

A minimalistic real-worldish blog engine written entirely in F#. Specifically made as a learning resource when building apps with the [SAFE](https://safe-stack.github.io/) stack. This application features many concerns of large apps such as:
 - Using third-party react libraries via interop
 - Deep nested views
 - Deep nested routing
 - Message interception as means for component communication
 - Logging
 - Database access
 - User security: authentication and authorization
 - Type-safe RPC communication  
 - Realtime type-safe messaging via web sockets 

# Screen recordings

![first.gif](/gifs/first.gif) 

![second.gif](/gifs/second.gif) 

![bridge.gif](/gifs/bridge.gif)

### The server uses the following tech 
 - [Suave](https://github.com/SuaveIO/suave) as a lightweight web server 
 - [LiteDB](https://github.com/mbdavid/LiteDB) as a lightweight embedded database through [LiteDB.FSharp](https://github.com/Zaid-Ajaj/LiteDB.FSharp)  
 - [Serilog](https://github.com/serilog/serilog) for logging through [Suave.SerilogExtensiosn](https://github.com/Zaid-Ajaj/Suave.SerilogExtensions)
 - [Jose](https://github.com/dvsekhvalnov/jose-jwt) for generating secure JSON web tokens 
 - [Fable.Remoting](https://github.com/Zaid-Ajaj/Fable.Remoting) for type-safe communication 
 - [Elmish.Bridge](https://github.com/Nhowka/Elmish.Bridge) for real-time type-safe messaging in an elmish model 
 - [Expecto](https://github.com/haf/expecto) for testing 

### The client uses the following tech 
 - [Elmish](https://github.com/elmish) for building the client architecture with react
 - [Bootstrap](https://getbootstrap.com/) for styling 
 - [Elmish.Toastr](https://github.com/Zaid-Ajaj/Elmish.Toastr) for toasts/notifications 
 - [Elmish.SweetAlert](https://github.com/Zaid-Ajaj/Elmish.SweetAlert) for simple and sweet elmish dialogs prompts 
 - [Fable.Remoting](https://github.com/Zaid-Ajaj/Fable.Remoting) for type-safe communication
 - [Elmish.Bridge](https://github.com/Nhowka/Elmish.Bridge) for real-time type-safe messaging in an elmish model 
 - Third-party javascript libraries 
   - [react-select](https://github.com/JedWatson/react-select) for adding tags to posts 
   - [react-event-timeline](https://github.com/rcdexta/react-event-timeline) for a timeline view of the blog posts 
   - [react-marked-markdown](https://github.com/Vincent-P/react-marked-markdown) for rendering markdown
   - [react-responsive](https://github.com/contra/react-responsive) for making the app responsive

# Communication Protocol
To understand how the application works and what it does, you simply take a look the protocol between the client and server:
```fs
type IBlogApi = {  
    getBlogInfo : unit -> Async<Result<BlogInfo, string>>
    login : LoginInfo -> Async<LoginResult>
    getPosts : unit -> Async<list<BlogPostItem>>
    getPostBySlug : string -> Async<Option<BlogPostItem>>
    getDrafts : AuthToken -> Async<Result<list<BlogPostItem>, string>>
    publishNewPost : SecureRequest<NewBlogPostReq> -> Async<AddPostResult> 
    savePostAsDraft : SecureRequest<NewBlogPostReq> -> Async<AddPostResult>
    deleteDraftById : SecureRequest<int> ->  Async<DeleteDraftResult>
    publishDraft : SecureRequest<int> -> Async<PublishDraftResult>
    deletePublishedArticleById : SecureRequest<int> -> Async<DeletePostResult>
    turnArticleToDraft: SecureRequest<int> ->  Async<MakeDraftResult>
    getPostById : SecureRequest<int> -> Async<Result<BlogPostItem,string>>
    savePostChanges : SecureRequest<BlogPostItem> ->  Async<Result<bool,string>>
    updateBlogInfo : SecureRequest<BlogInfo> -> Async<Result<SuccessMsg,ErrorMsg>>
    togglePostFeatured : SecureRequest<int> -> Async<Result<string,string>>
    updatePassword : SecureRequest<UpdatePasswordInfo> -> Async<Result<string, string>> 
}
```
Thanks to [Fable.Remoting](https://github.com/Zaid-Ajaj/Fable.Remoting), this application does not need to handle data serialization/deserialization and routing between client and server, it is all done for us which means that the code is 99% domain models and domain logic.

You will often see calls made to server like these:
```fs
let request = { Token = authToken; Body = article }

let saveChangesCmd = 
    Cmd.fromAsync 
        { Value = Server.api.savePostChanges request
          Error = fun ex -> SaveChangesError "Network error while saving changes to blog post"
          Success = function
            | Ok true -> SavedChanges
            | Error errorMsg -> SaveChangesError errorMsg 
            | otherwise -> DoNothing }
```
# Client Application Layout
The client application layout is how the components are structured in the project. The components are written in a consistent pattern that is reflected by the file system as follows:
```
ParentComponent 
   | 
   | - Types.fs
   | - State.fs
   | - View.fs
   | - ChildComponent
        | 
        | - Types.fs
        | - State.fs
        | - View.fs
```
Where the client is a tree of UI components:
```
App 
 |
 | - About
 | - Posts 
      | 
      | - SinglePost
      | - AllPosts 
 |
 | - Admin
      | 
      | - Login
      | - Backoffice
           | 
           | - PublishedArticles
           | - Drafts 
           | - Settings 
           | - NewArticle
           | - EditArticle 
```
# Component Types 
Every component comes with a `Types.fs` file that contains mostly three things 
- `State` data model that the component keeps track of
- `Msg` type that represents the events that can occur 
- `Pages` represnets the current page and sub pages that a component can have

The `State` keeps track of the `CurrentPage` but it will never update it by hand: the `CurrentPage` is only updated in response to url changes and these changes will dipatch a message to change the value of the `CurrentPage` along with dispatching other messages related to loading the data for the component in subject

# Important Concepts: Data Locality and Message Interception

Following these principles to help us write components in isolation:
 - Child components don't know anything about their parents
 - Child components don't know anything about their siblings
 - Parent components manage child state and communication between children 

The best example of these concepts is the interaction between the following components:
```
        Admin
          |
   ---------------
   |             |
Backoffice     Login     
```
## Message Interception by example

> Definition: Message intercption is having control over how messages flow in your application, allowing for communication between components that don't know each other even exist.  

`Login` doesn't know anything going on in the application as a whole, it just has a form for the user to input his credentials and try to login to the server to obtain an authorization token. When the token is obtained, a `LoginSuccess token` message is dispatched. However, this very message is *intercepted* by `Admin` (the parent of `Login`), updating the state of `Admin`:
```fs
// Admin/State.fs

let update msg (state: State) =
    match msg with
    | LoginMsg loginMsg ->
        match loginMsg with 
        // intercept the LoginSuccess message dispatched by the child component
        | Login.Types.Msg.LoginSuccess token ->
            let nextState = 
                { state with Login = state.Login
                             SecurityToken = Some token }
            nextState, Urls.navigate [ Urls.admin ] 
        // propagate other messages to child component
        | _ -> 
            let nextLoginState, nextLoginCmd = Admin.Login.State.update loginMsg state.Login
            let nextAdminState = { state with Login = nextLoginState }
            nextAdminState, Cmd.map LoginMsg nextLoginCmd
```
After updating the state of `Admin` to include the security token obtained from `Login`, the application navigates to the admin pages using `Urls.navigate [ Urls.admin ]`. Now the navigation will succeed, because navigating to the admin is allowed only if the admin has a security token defined:
```fs
// App/State.fs -> inside handleUpdatedUrl

| Admin.Types.Page.Backoffice backofficePage ->
    match state.Admin.SecurityToken with
    | None -> 
        // navigating to one of the admins backoffice pages 
        // without a security token? then you need to login first
        Cmd.batch [ Urls.navigate [ Urls.login ]
                    showInfo "You must be logged in first" ] 
    | Some userSecurityToken ->
        // then user is already logged in 
        // for each specific page, dispatch the appropriate message 
        // for initial loading of that data of that page
        match backofficePage with 
        | Admin.Backoffice.Types.Page.Drafts -> 
            Admin.Backoffice.Drafts.Types.LoadDrafts
            |> Admin.Backoffice.Types.Msg.DraftsMsg
            |> Admin.Types.Msg.BackofficeMsg 
            |> AdminMsg 
            |> Cmd.ofMsg
        
        | Admin.Backoffice.Types.Page.PublishedPosts -> 
            Admin.Backoffice.PublishedPosts.Types.LoadPublishedPosts
            |> Admin.Backoffice.Types.Msg.PublishedPostsMsg
            |> Admin.Types.Msg.BackofficeMsg
            |> AdminMsg
            |> Cmd.ofMsg 

        | Admin.Backoffice.Types.Page.Settings ->
            Admin.Backoffice.Settings.Types.Msg.LoadBlogInfo
            |> Admin.Backoffice.Types.Msg.SettingsMsg
            |> Admin.Types.Msg.BackofficeMsg
            |> AdminMsg
            |> Cmd.ofMsg 
         
        | Admin.Backoffice.Types.Page.EditArticle postId ->
            Admin.Backoffice.EditArticle.Types.Msg.LoadArticleToEdit postId 
            |> Admin.Backoffice.Types.Msg.EditArticleMsg 
            |> Admin.Types.Msg.BackofficeMsg
            |> AdminMsg 
            |> Cmd.ofMsg 
        
        | otherPage -> 
            Cmd.none
```

Another concrete example in this application: when you update the settings, the root component intercepts the "Changed settings" message and reloads it's blog information with the new settings accordingly

## Data Locality by example

> Definition: Data Locality is having control over the data that is available to certain components, without access to global state. 

Fact: Components of `Backoffice` need to make secure requests, hence they need a security token available whenever a request is to be made.

Requirement: Once the user is inside a component of `Backoffice`, there will always be a `SecurityToken` available to that component. This means I don't want to check whether there is a security token or not everytime I want to make a web request, because if there isn't one, there is an internal inconsistency: the user shouldn't have been able to reach the `Backoffice` component in the first place. 

Problem: The security token is only acquired *after* the user logs in from `Login`, but before that there isn't a security token, hence the type of the token will be `SecurityToken: string option` but we don't want an optional token, we want an actual token once we are logged in.

Solution: `Login` and components of `Backoffice` cannot be siblings, `Login` is happy with the security token being optional, while `Backoffice` insists on having a token at any given time. So we introduce a parent: `Admin` that handles the *optionalness* of the security token! The `Admin` will disallow the user from reaching `Backoffice` if there isn't a security token, and if there is one, it will be propagated to the backoffice:
```fs
// Admin/State.fs -> update
| BackofficeMsg msg ->
    match msg with 
    | Backoffice.Types.Msg.Logout -> 
        // intercept logout message of the backoffice child
        let nextState, _ = init()
        nextState, Urls.navigate [ Urls.posts ]
    | _ -> 
        match state.SecurityToken with 
        | Some token -> 
            let prevBackofficeState = state.Backoffice
            let nextBackofficeState, nextBackofficeCmd = 
                // pass security token down to backoffice
                Backoffice.State.update token msg prevBackofficeState
            let nextAdminState = { state with Backoffice = nextBackofficeState }
            nextAdminState, Cmd.map BackofficeMsg nextBackofficeCmd
        | None ->
            state, Cmd.none
``` 

# Unit-testable at the composition root level:
The composition root is where the application functionality gets all the dependencies it needs to run to application like the database and a logger. In this application, the composition root is where we contruct an implementation for the `IBlogApi` protocol: 
```fs
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
        togglePostFeatured = BlogPosts.togglePostFeatured database >> liftAsync 
        updatePassword = Admin.updatePassword logger database >> liftAsync
    }

    blogApi
```
Because LiteDB already includes an in-memory database and Serilog provides a simple no-op logger, you can write unit tests right off the bat at the application level:
```fs
// creates a disposable in memory database
let useDatabase (f: LiteDatabase -> unit) = 
    let mapper = FSharpBsonMapper()
    use memoryStream = new MemoryStream()
    use db = new LiteDatabase(memoryStream, mapper)
    f db

testCase "Login with default credentials works" <| fun _ -> 
    useDatabase <| fun db -> 
        let logger = Serilog.Log.Logger 
        let testBlogApi = WebApp.createBlogApi logger db 
        let loginInfo = { Username = "guest"; Password = "guest" }
        let result = Async.RunSynchronously (testBlogApi.login loginInfo)
        match result with 
        | LoginResult.Success token -> pass() 
        | _ -> fail()
```
Of course you can also test the individual function seperately because every function is also unit testable as long as you provide a database instance and a logger. 


# Responsive using different UI's
As opposed to using CSS to show or hide elements based on screen size, I used react-responsive to make a completely different app for small-sized screens, implemented as 
```fs
let app blogInfo state dispatch =
  div 
   [ ]
   [ mediaQuery 
      [ MinWidth 601 ]
      [ desktopApp blogInfo state dispatch ]
     mediaQuery 
      [ MaxWidth 600 ] 
      [ mobileApp blogInfo state dispatch ] ]
```
# Security with JWT
User authentication and authorization happen though secure requests, these requests include the JSON web token to authorize the user. The user acquires these JWT's when logging in and everything is stateless. An example of a secure request with it's handler on the server:
```fs
// Client

let nextState = { state with IsTogglingFeatured = Some postId }
let request = { Token = authToken; Body = postId }
let toggleFeatureCmd = 
    Cmd.fromAsync {
        Value = Server.api.togglePostFeatured request
        Error = fun ex -> ToggleFeaturedFinished (Error "Network error while toggling post featured")
        Success = function 
            | Ok successMsg -> ToggleFeaturedFinished (Ok successMsg)
            | Error errorMsg -> ToggleFeaturedFinished (Error errorMsg)
    } 
```
And it is handled like this on the server:
```fs
// Server

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
```
See [Modeling Authentication and Authorization](https://zaid-ajaj.github.io/Fable.Remoting/src/modeling-authentication.html) in Fable.Remoting to learn more
# Try out on your machine

Requirements: 
 - [.NET Core](https://www.microsoft.com/net/download)
 - [Node.js](https://nodejs.org/en/) 

Start watch build on windows:
```
git clone https://github.com/Zaid-Ajaj/tabula-rasa.git 
cd tabula-rasa
build.cmd Watch 
```
On linux/mac you can use bash
```bash
git clone https://github.com/Zaid-Ajaj/tabula-rasa.git 
cd tabula-rasa
./build.sh Watch
```
This will start the build and create the [LiteDb](https://github.com/Zaid-Ajaj/LiteDB.FSharp) (single file) database for the first time if it does not already exists. The database will be in the application data directory of your OS under the `TabulaRasa` directory with name `tabula-rasa.db` along with the newly generated secret key used for generating secure Json web tokens. 

When the build finishes, you can navigate to `http://localhost:8090` to start using the application. Once you make changes to either server or client, it will automatically re-compile the app.

Once the application starts, the home page will tell you *"There aren't any stories published yet"* because the database is still empty. You can then navigate to `http://localhost:8090/#login` to login in as an admin who can write stories. The default credentials are Username = `guest` and Password = `guest`. 


# More
There is a lot to talk about with this application, but the best to learn from it is by actually trying it out and going through the code yourself. If you need clarification or explanation on why a code snippet is written the way it is, just open an issue with your question :) 