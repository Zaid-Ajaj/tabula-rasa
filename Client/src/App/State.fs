module App.State

open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open App.Types

let toHash page =
  match page with
  | Posts -> "#posts"
  | About -> "#about"
  | Admin -> "#admin"

let pageParser: Parser<Page->Page,Page> =
  oneOf [ map Admin (s "admin")
          map Posts (s "posts")
          map About (s "about") ]

let urlUpdate (result: Option<Page>) model =
  match result with
  | None ->
      model, Cmd.none
  | Some page ->
      { model with CurrentPage = page }, Cmd.ofMsg (ViewPage page)

let init result =
  let initialPage = Posts
  let (posts, postsCmd) = Posts.State.init()
  let admin, adminCmd = Admin.State.init()
  let (model, cmd) =
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
              (fun info -> 
                BlogInfoLoaded 
                  { Name = info.Name
                    About = info.About
                    ProfileImageUrl = info.ProfileImageUrl })
              (fun _ -> BlogInfoLoadFailed)

let update msg state =
  match msg with
  | PostsMsg msg ->
      let postsState, postsCmd = Posts.State.update state.Posts msg 
      let appState = { state with Posts = postsState }
      let appCmd = Cmd.map PostsMsg postsCmd
      appState, appCmd
  | AdminMsg msg ->
      let adminState, adminCmd = Admin.State.update msg state.Admin
      { state with Admin = adminState }, Cmd.map AdminMsg adminCmd
  | LoadBlogInfo ->
      let nextState = { state with LoadingBlogInfo = true }
      nextState, loadBlogInfoCmd
  | BlogInfoLoaded info ->
      let nextState = { state with BlogInfo = Some info; LoadingBlogInfo = false }
      nextState, Cmd.none
  | BlogInfoLoadFailed ->
      let nextState = { state with BlogInfo = None; LoadingBlogInfo = false }
      nextState, Cmd.none
  | ViewPage page ->
      let nextState = { state with CurrentPage = page }
      let modifyUrlCmd = Navigation.newUrl (toHash page)
      let reloadCmd =
        match page with 
        | Posts -> Cmd.ofMsg (PostsMsg Posts.Types.Msg.LoadLatestPosts)
        | _ -> Cmd.none
      nextState, Cmd.batch [ modifyUrlCmd; reloadCmd ]