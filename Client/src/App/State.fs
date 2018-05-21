module App.State

open System
open Elmish
open App.Types
open Shared
open Fable.Import.Browser


type BackofficePage = Admin.Backoffice.Types.Page
type PostsPage = Posts.Types.Page

let pageHash = function 
    | Page.About -> Urls.about 
    | Page.Posts page -> 
        match page with 
        | Posts.Types.Page.AllPosts -> Urls.posts
        | Posts.Types.Page.Post postSlug -> Urls.combine [ Urls.posts; postSlug ]   
    | Page.Admin adminPage ->
        match adminPage with 
        | Admin.Types.Page.Login -> Urls.login
        | Admin.Types.Page.Backoffice backofficePage ->
            match backofficePage with 
            | BackofficePage.Home -> Urls.admin
            | BackofficePage.NewPost -> Urls.combine [ Urls.admin; Urls.newPost ]   
            | BackofficePage.Drafts -> Urls.combine [ Urls.admin; Urls.drafts ]  
            | BackofficePage.PublishedPosts -> Urls.combine [ Urls.admin; Urls.publishedPosts ]
            | BackofficePage.Settings -> Urls.combine [ Urls.admin; Urls.settings ]
            | BackofficePage.EditArticle postId -> Urls.combine [ Urls.admin; Urls.editPost; string postId ]

/// Tries to parse a url into a page 
let parseUrl (urlHash: string) = 
    let segments = 
        if urlHash.StartsWith "#" 
        then urlHash.Substring(1, urlHash.Length - 1) // remove the hash sign
        else urlHash
        |> fun hash -> hash.Split '/' // split the url segments
        |> List.ofArray
        |> List.filter (String.IsNullOrWhiteSpace >> not)  

    match segments with
    | [ Urls.about ] -> 
        // the about page
        App.Types.Page.About
        |> Some 

    | [ Urls.posts ] -> 
        // all posts page
        Posts.Types.Page.AllPosts
        |> App.Types.Page.Posts
        |> Some

    | [ Urls.posts; postSlug ] -> 
        // matches against a specific post by it's slug
        Posts.Types.Page.Post postSlug
        |> App.Types.Page.Posts
        |> Some  
    
    | [ Urls.admin ] -> 
        // the home page of the backoffice
        Admin.Backoffice.Types.Page.Home
        |> Admin.Types.Page.Backoffice
        |> App.Types.Page.Admin
        |> Some

    | [ Urls.login ] -> 
        // the login page 
        Admin.Types.Page.Login
        |> App.Types.Page.Admin
        |> Some 

    | [ Urls.admin; Urls.drafts ] ->
        // the drafts page 
        Admin.Backoffice.Types.Page.Drafts
        |> Admin.Types.Page.Backoffice
        |> App.Types.Page.Admin
        |> Some 

    | [ Urls.admin; Urls.publishedPosts ] ->
        // the page of published stories
        Admin.Backoffice.Types.Page.PublishedPosts
        |> Admin.Types.Page.Backoffice
        |> App.Types.Page.Admin
        |> Some 

    | [ Urls.admin; Urls.newPost ] ->
        // the new post page
        Admin.Backoffice.Types.Page.NewPost
        |> Admin.Types.Page.Backoffice
        |> App.Types.Page.Admin
        |> Some 
    
    | [ Urls.admin; Urls.settings ] ->
        // the settings page
        Admin.Backoffice.Types.Page.Settings
        |> Admin.Types.Page.Backoffice
        |> App.Types.Page.Admin
        |> Some 

    | [ Urls.admin; Urls.editPost; Urls.Int postId ] ->
        // editing a post by the post id 
        Admin.Backoffice.Types.Page.EditArticle postId 
        |> Admin.Types.Page.Backoffice
        |> App.Types.Page.Admin
        |> Some

    | _ -> None 


let init() =
  let posts, postsCmd = Posts.State.init()
  let admin, adminCmd = Admin.State.init()
  let model =
      { BlogInfo = Empty
        CurrentPage = None
        Admin = admin
        Posts = posts }

  let initialPageCmd = 
    // parse the current location and navigate to that location 
    // directly when application starts up
    match parseUrl window.location.hash with  
    | Some page -> Cmd.ofMsg (UrlUpdated page)
    | None -> 
        // if unable to parse the location (-> unknown url)
        // then navigate to allPosts, here 
        Posts.Types.Page.AllPosts
        |> App.Types.Page.Posts
        |> UrlUpdated
        |> Cmd.ofMsg 

  model, Cmd.batch [ initialPageCmd
                     Cmd.map PostsMsg postsCmd
                     Cmd.map AdminMsg adminCmd
                     Cmd.ofMsg LoadBlogInfo ]

let showInfo msg = 
    Toastr.message msg
    |> Toastr.withTitle "Tabula Rasa"
    |> Toastr.info  

/// What happens when the URL is updated, either from the application's components 
/// or manually by the user isn't just simply changing the current view of the specific child
/// but rather, when a specific child is requested, then dispatch an appropriate message for 
/// loading initial data of that, also here is where you define the logic of checking whether 
/// a request to an admin page should be redirected to the login page if there is no user logged in
let handleUpdatedUrl nextPage state = 
    match nextPage with 
    | Page.About ->
        let nextState = { state with CurrentPage = Some Page.About }
        nextState, Cmd.none
    
    | Page.Posts postsPage -> 
        match postsPage with 
        | Posts.Types.Page.AllPosts -> 
           // asking for all posts? the dispatch the LoadLatestPosts message to reload them
           let nextState = { state with CurrentPage = Some (Posts postsPage) }
           let nextCmd = Cmd.ofMsg (PostsMsg Posts.Types.Msg.LoadLatestPosts)
           nextState, nextCmd 
        
        | Posts.Types.Page.Post postSlug -> 
           // asking for a specific post by it's slug in the url?
           // then dispatch a message to load that post via the "LoadSinglePost" message
           let nextState = { state with CurrentPage = Some (Posts postsPage) }
           let nextCmd = Cmd.ofMsg (PostsMsg (Posts.Types.Msg.LoadSinglePost postSlug))
           nextState, nextCmd 

    | Page.Admin adminPage ->
      let nextAdminCmd = 
        match adminPage with
        | Admin.Types.Page.Login ->
            match state.Admin.SecurityToken with
            | None ->
                // going to login page and there is no security token?
                // then just login 
                Cmd.none
            | Some _ ->
                // going to login page and there is already a security token
                // then just navigate to admin home page because
                // we are already logged in 
                Cmd.batch [ Urls.navigate [ Urls.admin ];
                            showInfo "Already logged in" ]
    
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

      let nextState = { state with CurrentPage = Some (Admin adminPage) }
      nextState, nextAdminCmd

let update msg state =
  match msg with
  | PostsMsg msg ->
      let postsState, postsCmd = Posts.State.update state.Admin.SecurityToken state.Posts msg 
      let appState = { state with Posts = postsState }
      let appCmd = Cmd.map PostsMsg postsCmd
      appState, appCmd

  // here, we are intercepting a "ChangesSaved" message triggered from settings
  // at this point, we have new blog info -> reload the blog info
  | AdminMsg (Admin.Types.BackofficeMsg (Admin.Backoffice.Types.SettingsMsg ((Admin.Backoffice.Settings.Types.ChangesSaved msg)))) ->
        state, Cmd.ofMsg LoadBlogInfo
  
  | AdminMsg msg ->
      let nextAdminState, adminCmd = Admin.State.update msg state.Admin
      let nextAppState = { state with Admin = nextAdminState }
      let nextAppCmd = Cmd.map AdminMsg adminCmd
      nextAppState, nextAppCmd
      
  | LoadBlogInfo ->
      let nextState = { state with BlogInfo = Loading }
      nextState, Http.loadBlogInfo
      
  | BlogInfoLoaded (Ok blogInfo) ->
      let nextState = { state with BlogInfo = Body blogInfo }
      let setPageTitle title = 
        Fable.Import.Browser.document.title <- title 
      nextState, Cmd.attemptFunc setPageTitle blogInfo.BlogTitle (fun ex -> DoNothing)
      
  | BlogInfoLoaded (Error errorMsg) ->
     let nextState = { state with BlogInfo = LoadError errorMsg }
     nextState, Toastr.error (Toastr.message errorMsg)

  | BlogInfoLoadFailed msg ->
      let nextState = { state with BlogInfo = LoadError msg }
      nextState, Cmd.none
      
  | NavigateTo page ->
      let nextUrl = Urls.hashPrefix (pageHash page)
      state, Urls.newUrl nextUrl

  | DoNothing ->
      state, Cmd.none
      
  | UrlUpdated nextPage -> 
      handleUpdatedUrl nextPage state
