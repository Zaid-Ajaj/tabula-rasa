# Tabula Rasa

A minimalistic real-worldish blog engine written entirely in F#. Specifically made as a learning resource when building apps with the [SAFE](https://safe-stack.github.io/) stack. This application features many concerns of large apps such as:
 - Using third-party react libraries via interop
 - Deep nested views
 - Deep nested routing
 - Message interception as means for component communication
 - Logging
 - Database access
 - User security: authentication and authorization
 - Type-safe RPC communication  

# Screen recordings

![first.gif](/gifs/first.gif) 

![second.gif](/gifs/second.gif) 

### The server uses the following tech 
 - [Suave](https://github.com/SuaveIO/suave) as a lightweight web server 
 - [LiteDB](https://github.com/mbdavid/LiteDB) as a lightweight embedded database through [LiteDB.FSharp](https://github.com/Zaid-Ajaj/LiteDB.FSharp)  
 - [Serilog](https://github.com/serilog/serilog) for logging through [Suave.SerilogExtensiosn](https://github.com/Zaid-Ajaj/Suave.SerilogExtensions)
 - [Jose](https://github.com/dvsekhvalnov/jose-jwt) for generating secure JSON web tokens 
 - [Fable.Remoting](https://github.com/Zaid-Ajaj/Fable.Remoting) for type-safe communication 
 - [Expecto](https://github.com/haf/expecto) for testing 

### The client uses the following tech 
 - [Elmish](https://github.com/elmish) for building the client architecture with react
 - [Bootstrap](https://getbootstrap.com/) for styling 
 - [Elmish.Toastr](https://github.com/Zaid-Ajaj/Elmish.Toastr) for toasts/notifications 
 - [Fable.Remoting](https://github.com/Zaid-Ajaj/Fable.Remoting) for type-safe communication
 - Third-party javascript libraries 
   - [react-select](https://github.com/JedWatson/react-select) for adding tags to posts 
   - [react-event-timeline](https://github.com/rcdexta/react-event-timeline) for a timeline view of the blog posts 
   - [react-marked-markdown](https://github.com/Vincent-P/react-marked-markdown) for rendering markdown
   - [react-responsive](https://github.com/contra/react-responsive) for making the app responsive
   - [SweetAlert2](https://github.com/sweetalert2/sweetalert2) for sweet sweet prompt dialogs 

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
    togglePostFeauted : SecureRequest<int> -> Async<Result<string,string>>
    updatePassword : SecureRequest<UpdatePasswordInfo> -> Async<Result<string, string>> 
}
```
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

# Data Locality and Message Interception

Following these principles to help us write components in isolation:
 - Child components don't know anything about their parents
 - Child components don't know anything about their siblings
 - Parent components manage child state and communication between children 

For example, in order for `ChildA` to send a message to `ChildB`, the message has to be dispatched from `ChildA` -> *intercepted* by the parent of `ChildA` -> propagated as another message to `ChildB` from parent. 

The best example of this concept is the interaction between the components:
```
      Admin
        |
  ---------------
  |             |
Backoffice    Login     

```
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
        Value = Server.api.togglePostFeauted request
        Error = fun ex -> ToggleFeaturedFinished (Error "Network error while toggling post featured")
        Success = function 
            | Ok successMsg -> ToggleFeaturedFinished (Ok successMsg)
            | Error errorMsg -> ToggleFeaturedFinished (Error errorMsg)
    } 
```
And it is handles like this on the server:
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
# More
There is a lot to talk about with this application, but the best to learn from it is by actually trying it out and going through the code yourself. If you need clarification or explanation on why a code snippet is written the way it is, just open an issue with your question :) 