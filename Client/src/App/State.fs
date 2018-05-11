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
  | About -> Urls.about
  | Posts PostsPage.AllPosts -> Urls.posts
  | Posts (PostsPage.Post slug) -> Urls.combine [ Urls.posts; slug ]
  | Admin Admin.Types.Page.Login -> Urls.login 
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Home) -> Urls.admin
  | Admin (Admin.Types.Page.Backoffice BackofficePage.NewArticle) -> Urls.combine [ Urls.admin; Urls.newPost ]
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Drafts) -> Urls.combine [ Urls.drafts; Urls.drafts ]
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Articles) -> Urls.combine [ Urls.admin; Urls.publishedArticles ]
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Subscribers) -> Urls.combine [ Urls.admin; Urls.subscribers ]
  | Admin (Admin.Types.Page.Backoffice BackofficePage.Settings) -> Urls.combine [ Urls.admin; Urls.settings ]
  | Admin (Admin.Types.Page.Backoffice (BackofficePage.EditArticle editArticleId)) -> Urls.combine [ Urls.admin; Urls.editArticle; string editArticleId ]
  |> Urls.hashPrefix

let pageParser: Parser<Page -> Page, Page> =
  oneOf [ map About (s Urls.about)
          map (Admin Admin.Types.Page.Login) (s Urls.login)
          map (PostsPage.Post >> Posts) (s Urls.posts </> str)
          map (Posts PostsPage.AllPosts) (s Urls.posts )
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Home)) (s Urls.admin)
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.NewArticle)) (s Urls.admin </> s Urls.newPost)
          map (fun id -> Admin (Admin.Types.Page.Backoffice (BackofficePage.EditArticle id))) (s Urls.admin </> s Urls.editArticle </> i32)
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Drafts)) (s Urls.admin </> s Urls.drafts)
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Articles)) (s Urls.admin </> s Urls.publishedArticles)
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Subscribers)) (s Urls.admin </> s Urls.subscribers)
          map (Admin (Admin.Types.Page.Backoffice BackofficePage.Settings)) (s Urls.admin </> s Urls.settings) ]

let urlUpdate (parsedPage: Option<Page>) currentState =
  match parsedPage with
  | None ->
      currentState, Urls.navigate [ Urls.posts ]
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
              | Posts.Types.Page.AllPosts  -> Cmd.ofMsg (PostsMsg Posts.Types.Msg.LoadLatestPosts)
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
              | Some _ -> Cmd.batch [ Navigation.newUrl (Urls.hashPrefix Urls.admin);
                                      showInfo "Already logged in" ]
     
          | Admin.Types.Page.Backoffice _ ->
              match state.Admin.SecurityToken with
              | None -> Cmd.batch [ Navigation.newUrl (Urls.hashPrefix Urls.login)
                                    showInfo "You must be logged in first" ]
              | Some _ -> Cmd.none

        let nextState = { state with CurrentPage = Some (Admin adminPage) }
        nextState, nextAdminCmd
