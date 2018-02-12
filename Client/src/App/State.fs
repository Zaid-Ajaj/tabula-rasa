module App.State

open Elmish
open Elmish.Browser.UrlParser
open App.Types
open Elmish.Browser.Navigation


let toHash page =
  match page with
  | Posts -> "#posts"
  | About -> "#about"
  | Admin Login -> "#login"
  | Admin (Backoffice Home) -> "#admin"
  | Admin (Backoffice NewArticle) -> "#admin/posts/new"
  | Admin (Backoffice Drafts) -> "#admin/posts/drafts"
  | Admin (Backoffice Published) -> "#admin/posts/published"
  | Admin (Backoffice Subscribers) -> "#admin/subscribers"
  | Admin (Backoffice Settings) -> "#admin/settings"

let pageParser: Parser<AppPage -> AppPage, AppPage> =
  oneOf [ map (Admin Login) (s "login")
          map (Admin (Backoffice Home)) (s "admin")
          map (Admin (Backoffice Settings)) (s "admin" </> s "settings")
          map (Admin (Backoffice Drafts)) (s "admin" </> s "posts" </> s "drafts")
          map (Admin (Backoffice Published)) (s "admin" </> s "posts" </> s "published")
          map (Admin (Backoffice NewArticle)) (s "admin" </> s "posts" </> s "new")
          map (Admin (Backoffice Subscribers)) (s "admin" </> s "subscribers")
          map Posts (s "posts")
          map About (s "about") ]

let urlUpdate (result: Option<AppPage>) model =
  match result with
  | None ->
      model, Cmd.none
  | Some page ->
      model, Cmd.ofMsg (UrlUpdated page)

let mapAppToAdmin (adminPage : AdminPage) = 
    match adminPage with
    | Login -> 
        Admin.Types.Page.Login
    | Backoffice backofficePage -> 
        match backofficePage with
        | Home -> Admin.Backoffice.Types.Page.Home
        | NewArticle -> Admin.Backoffice.Types.Page.NewArticle
        | Settings -> Admin.Backoffice.Types.Page.Settings
        | Published -> Admin.Backoffice.Types.Page.Published
        | Drafts -> Admin.Backoffice.Types.Page.Drafts
        | Subscribers -> Admin.Backoffice.Types.Page.Subscribers
        |> Admin.Types.Page.Backoffice

let mapAdminToApp (adminPage: Option<Admin.Types.Page>) defaultPage = 
    match adminPage with
    | Some Admin.Types.Page.Login -> Admin Login
    | Some (Admin.Types.Page.Backoffice backofficePage) ->
        match backofficePage  with
        | Admin.Backoffice.Types.Page.Home -> Admin (Backoffice Home)
        | Admin.Backoffice.Types.Page.NewArticle -> Admin (Backoffice NewArticle)
        | Admin.Backoffice.Types.Page.Settings -> Admin (Backoffice Settings)
        | Admin.Backoffice.Types.Page.Published -> Admin (Backoffice Published)
        | Admin.Backoffice.Types.Page.Drafts -> Admin (Backoffice Drafts)
        | Admin.Backoffice.Types.Page.Subscribers -> Admin (Backoffice Subscribers)
    | None -> defaultPage

let init result =
  let initialPage = Posts
  let posts, postsCmd = Posts.State.init()
  let admin, adminCmd = Admin.State.init()
  let model, cmd =
    urlUpdate result
      { LoadingBlogInfo = false
        CurrentPage = initialPage
        Admin = admin
        Posts = posts
        BlogInfo = None }

  model, Cmd.batch [ cmd
                     Cmd.map PostsMsg postsCmd
                     Cmd.map AdminMsg adminCmd
                     Cmd.ofMsg LoadBlogInfo ]

let server = Server.createProxy()

let loadBlogInfoCmd = 
  Cmd.ofAsync server.getBlogInfo ()
              BlogInfoLoaded
              (fun _ -> BlogInfoLoadFailed)

let update msg state =
  match msg with
  | PostsMsg msg ->
      let postsState, postsCmd = Posts.State.update state.Posts msg 
      let appState = { state with Posts = postsState }
      let appCmd = Cmd.map PostsMsg postsCmd
      appState, appCmd

  | AdminMsg msg ->
      let nextAdminState, adminCmd = Admin.State.update msg state.Admin
      let nextAppPage = mapAdminToApp nextAdminState.CurrentPage state.CurrentPage
      let nextAppState = { state with Admin = nextAdminState
                                      CurrentPage = nextAppPage }
      let nextAppCmd = Cmd.batch [ Cmd.map AdminMsg adminCmd; 
                                   Cmd.ofMsg (SetCurrentPage nextAppPage) ]
      nextAppState, nextAppCmd
      
  | LoadBlogInfo ->
      let nextState = { state with LoadingBlogInfo = true }
      nextState, loadBlogInfoCmd
  | BlogInfoLoaded info ->
      let nextState = { state with BlogInfo = Some info; LoadingBlogInfo = false }
      nextState, Cmd.none
  | BlogInfoLoadFailed ->
      let nextState = { state with BlogInfo = None; LoadingBlogInfo = false }
      nextState, Cmd.none
  | SetCurrentPage page -> 
      state, Navigation.modifyUrl (toHash page)
  | UrlUpdated page -> 
      match page with 
      | Posts -> 
           // make sure to load posts anytime the posts page is requested
           let nextAppState = { state with CurrentPage = page }
           let nextCmd = Cmd.ofMsg (PostsMsg Posts.Types.Msg.LoadLatestPosts)
           nextAppState, nextCmd
      | AppPage.About ->
           // just show the About page
           let nextAppState = { state with CurrentPage = page }
           nextAppState, Cmd.none
      | Admin adminPage ->
           // tell child to update current page by sending an admin message
           let adminMsg = Admin.Types.SetCurrentPage (mapAppToAdmin adminPage)
           state, Cmd.ofMsg (AdminMsg adminMsg)  
