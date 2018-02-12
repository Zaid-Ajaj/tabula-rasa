module App.State

open Elmish
open Elmish.Browser.UrlParser
open App.Types
open Elmish.Browser.Navigation

type BackofficePage = Admin.Backoffice.Types.Page

let toHash page =
  match page with
  | Posts -> "#posts"
  | About -> "#about"
  | Admin Admin.Types.Page.Login -> "#login"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Home) -> "#admin"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.NewArticle) -> "#admin/posts/new"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Drafts) -> "#admin/posts/drafts"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Published) -> "#admin/posts/published"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Subscribers) -> "#admin/subscribers"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Settings) -> "#admin/settings"

let pageParser: Parser<AppPage -> AppPage, AppPage> =
  oneOf [ map (Admin Admin.Types.Page.Login) (s "login")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Home)) (s "admin")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Settings)) (s "admin" </> s "settings")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Drafts)) (s "admin" </> s "posts" </> s "drafts")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Published)) (s "admin" </> s "posts" </> s "published")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.NewArticle)) (s "admin" </> s "posts" </> s "new")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Subscribers)) (s "admin" </> s "subscribers")
          map Posts (s "posts")
          map About (s "about") ]

let urlUpdate (result: Option<AppPage>) model =
  match result with
  | None ->
      model, Cmd.none
  | Some page ->
      model, Cmd.ofMsg (UrlUpdated page)

let init result =
  let initialPage = Posts
  let posts, postsCmd = Posts.State.init()
  let admin, adminCmd = Admin.State.init()
  let model, cmd =
    urlUpdate result
      { LoadingBlogInfo = false
        CurrentPage = Some initialPage
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
      let nextAdminPage = nextAdminState.CurrentPage |> Option.map AppPage.Admin
      let nextAppState = { state with Admin = nextAdminState
                                      CurrentPage = nextAdminPage }
      let nextAppCmd = Cmd.batch [ Cmd.map AdminMsg adminCmd; 
                                   Cmd.ofMsg (SetCurrentPage nextAdminPage) ]
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
  | SetCurrentPage (Some page) -> 
      state, Navigation.newUrl (toHash page)
  | SetCurrentPage None ->
      state, Cmd.none
  | UrlUpdated page -> 
      match page with 
      | Posts -> 
           // make sure to load posts anytime the posts page is requested
           let nextAppState = { state with CurrentPage = Some page }
           let nextCmd = Cmd.ofMsg (PostsMsg Posts.Types.Msg.LoadLatestPosts)
           nextAppState, nextCmd
      | AppPage.About ->
           // just show the About page
           let nextAppState = { state with CurrentPage = Some page }
           nextAppState, Cmd.none
      | Admin adminPage ->
           // tell child to update current page by sending an admin message
           let adminMsg = Admin.Types.SetCurrentPage adminPage
           state, Cmd.ofMsg (AdminMsg adminMsg)  
