module App.State

open Elmish
open Elmish.Browser.UrlParser
open Elmish.Browser.Navigation

open App.Types

type BackofficePage = Admin.Backoffice.Types.Page

let toHash page =
  match page with
  | Posts -> "#posts"
  | About -> "#about"
  | Admin Admin.Types.Page.Login -> "#login"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Home) -> "#admin"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.NewArticle) -> "#admin/new-post"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Drafts) -> "#admin/drafts"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Published) -> "#admin/published"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Subscribers) -> "#admin/subscribers"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Settings) -> "#admin/settings"

let pageParser: Parser<Page -> Page, Page> =
  oneOf [ map (Admin Admin.Types.Page.Login) (s "login")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.NewArticle)) (s "admin" </> s "new-post")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Drafts)) (s "admin" </> s "drafts")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Published)) (s "admin" </> s "published")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Subscribers)) (s "admin" </> s "subscribers")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Home)) (s "admin")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Settings)) (s "admin" </> s "settings")
          map Posts (s "posts")
          map About (s "about") ]

let urlUpdate (parsedPage: Option<Page>) currentState =
  match parsedPage with
  | None ->
      currentState, Cmd.none
  | Some page ->
      currentState, Cmd.ofMsg (UrlUpdated page)

let init result =
  let posts, postsCmd = Posts.State.init()
  let admin, adminCmd = Admin.State.init()
  let model, cmd =
    urlUpdate result
      { LoadingBlogInfo = false
        CurrentPage = None
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
      let nextAppState = { state with Admin = nextAdminState }
      let nextAppCmd = Cmd.map AdminMsg adminCmd
      nextAppState, nextAppCmd
      
  | LoadBlogInfo ->
      let nextState = { state with LoadingBlogInfo = true }
      nextState, loadBlogInfoCmd
      
  | BlogInfoLoaded info ->
      let nextState = { state with BlogInfo = Some info; LoadingBlogInfo = false }
      nextState, Cmd.ofMsg (NavigateTo (Some Posts))
      
  | BlogInfoLoadFailed ->
      let nextState = { state with BlogInfo = None; LoadingBlogInfo = false }
      nextState, Cmd.none
      
  | NavigateTo (Some page) ->
      state, Navigation.newUrl (toHash page)
      
  | NavigateTo None ->
      state, Cmd.none
      
  | UrlUpdated page -> 
      match page with 
      | Posts -> 
           // make sure to load posts anytime the posts page is requested
           let nextAppState = { state with CurrentPage = Some Posts }
           let nextCmd = Cmd.ofMsg (PostsMsg Posts.Types.Msg.LoadLatestPosts)
           nextAppState, nextCmd
      | Page.About ->
           let nextState = { state with CurrentPage = Some Page.About }
           nextState, Cmd.none
      | Admin adminPage ->
           let nextState = { state with CurrentPage = Some (Admin adminPage) }
           nextState, Cmd.ofMsg (AdminMsg (Admin.Types.Msg.SetCurrentPage adminPage))
