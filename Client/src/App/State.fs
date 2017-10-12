module App.State

open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open App.Types

let toHash page =
  match page with
  | Posts -> "#latest-posts"
  | Featured -> "#featured"
  | Archive -> "#archive"
  | Contact -> "#contact"
  | Home -> ""

let pageParser: Parser<Page->Page,Page> =
  oneOf [
    map Posts (s "latest-posts")
    map Featured (s "featured")
    map Archive (s "archive")
    map Contact (s "contact")
    map Home (s "")
  ]

let urlUpdate (result: Option<Page>) model =
  match result with
  | None ->
      model, Navigation.modifyUrl (toHash model.CurrentPage)
  | Some page ->
      { model with CurrentPage = page }, Cmd.none

let init result =
  let initialPage = Home
  let (posts, postsCmd) = Posts.State.init()
  let (home, homeCmd) = Home.State.init()
  let (model, cmd) =
    urlUpdate result
      { CurrentPage = initialPage
        SecurityToken = None
        Posts = posts
        Home = home }
  model, Cmd.batch [ cmd
                     Cmd.map PostsMsg postsCmd
                     Cmd.map HomeMsg homeCmd ]

let update msg state =
  match msg with
  | PostsMsg msg ->
      let postsState, postsCmd = Posts.State.update state.Posts msg 
      let appState = { state with Posts = postsState }
      let appCmd = Cmd.map PostsMsg postsCmd
      appState, appCmd
  | HomeMsg msg ->
      let (home, homeCmd) = Home.State.update msg state.Home
      { state with Home = home }, Cmd.map HomeMsg homeCmd
  | ViewPage page ->
      let nextState = { state with CurrentPage = page }
      let modifyUrlCmd = Navigation.modifyUrl (toHash page)
      let reloadCmd =   
        match page with 
        | Posts -> Cmd.ofMsg (PostsMsg Posts.Types.Msg.LoadLatestPosts)
        | _ -> Cmd.none
      nextState, Cmd.batch [ modifyUrlCmd; reloadCmd ]
