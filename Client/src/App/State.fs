module App.State

open Elmish
open Elmish.Browser.UrlParser
open Elmish.Browser.Navigation
open App.Types
open Shared

type BackofficePage = Admin.Backoffice.Types.Page
type PostsPage = Posts.Types.Page

let toHash page =
  match page with
  | About -> "#about"
  | Posts PostsPage.AllPosts -> "#posts"
  | Posts (PostsPage.Post slug) -> "#posts/" + slug
  | Admin Admin.Types.Page.Login -> "#login"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Home) -> "#admin"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.NewArticle) -> "#admin/new-post"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Drafts) -> "#admin/drafts"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Published) -> "#admin/published"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Subscribers) -> "#admin/subscribers"
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Settings) -> "#admin/settings"

let pageParser: Parser<Page -> Page, Page> =
  oneOf [ map About (s "about")
          map (Admin Admin.Types.Page.Login) (s "login")
          map (PostsPage.Post >> Posts) (s "posts" </> str)
          map (Posts PostsPage.AllPosts) (s "posts")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Home)) (s "admin")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.NewArticle)) (s "admin" </> s "new-post")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Drafts)) (s "admin" </> s "drafts")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Published)) (s "admin" </> s "published")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Subscribers)) (s "admin" </> s "subscribers")
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Settings)) (s "admin" </> s "settings") ]

let urlUpdate (parsedPage: Option<Page>) currentState =
  match parsedPage with
  | None ->
      currentState, Navigation.newUrl "#posts"
  | Some page ->
      currentState, Cmd.ofMsg (UrlUpdated page)

let init result =
  let posts, postsCmd = Posts.State.init()
  let admin, adminCmd = Admin.State.init()
  let model, cmd =
    urlUpdate result
      { BlogInfo = Empty
        CurrentPage = None
        Admin = admin
        Posts = posts }

  model, Cmd.batch [ cmd
                     Cmd.map PostsMsg postsCmd
                     Cmd.map AdminMsg adminCmd
                     Cmd.ofMsg LoadBlogInfo ]

let showInfo msg = 
     Toastr.message msg
     |> Toastr.withTitle "Tabula Rasa"
     |> Toastr.info  

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
      let nextState = { state with BlogInfo = Loading }
      nextState, Http.loadBlogInfo
      
  | BlogInfoLoaded info ->
      let nextState = { state with BlogInfo = Body info }
      nextState, Cmd.none
      
  | BlogInfoLoadFailed msg ->
      let nextState = { state with BlogInfo = LoadError msg }
      nextState, Cmd.none
      
  | NavigateTo (Some page) ->
      state, Navigation.newUrl (toHash page)
      
  | NavigateTo None ->
      state, Cmd.none
      
  | UrlUpdated page -> 
      match page with 
      | Posts page -> 
           // make sure to load posts anytime the posts page is requested
           let nextAppState = { state with CurrentPage = Some (Posts page) }
           let nextCmd =
              match page with
              | Posts.Types.Page.AllPosts ->  Cmd.ofMsg (PostsMsg Posts.Types.Msg.LoadLatestPosts)
              | Posts.Types.Page.Post slug -> Cmd.ofMsg (PostsMsg (Posts.Types.Msg.LoadSinglePost slug))
              
           nextAppState, nextCmd

      | Page.About ->
           let nextState = { state with CurrentPage = Some Page.About }
           nextState, Cmd.none

      | Admin adminPage ->
        let nextAdminCmd = 
          match adminPage with
          | Admin.Types.Page.Login ->
              match state.Admin.SecurityToken with
              | None -> Cmd.none
              | Some _ -> Cmd.batch [ Navigation.newUrl "#admin";
                                      showInfo "Already logged in" ]
     
          | Admin.Types.Page.Backoffice _ ->
              match state.Admin.SecurityToken with
              | None -> Cmd.batch [ Navigation.newUrl "#login"
                                    showInfo "You must be logged in first" ]
              | Some _ -> Cmd.none

        let nextState = { state with CurrentPage = Some (Admin adminPage) }
        nextState, nextAdminCmd
