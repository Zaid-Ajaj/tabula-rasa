module App.View

open App.Types
open Shared.ViewModels
open Fable.Helpers.React
open Fable.Helpers.React.Props

let menuItem label (page: Option<Page>) currentPage dispatcher =
    div
      [ classList  
         [ "menu-item", true
           "menu-item-selected", page = currentPage ] 
        OnClick (fun _ -> dispatcher (NavigateTo page)) ]
      [ str label ]

let sidebar (blogInfo: BlogInfo) state dispatcher =
  aside
    [ ClassName "fit-parent child-space"; Style [ TextAlign "center" ] ]
    [ div
        [ Style [ TextAlign "center" ] ]
        [ h3 [ Style [ Color "white" ] ] [ str blogInfo.Name ]
          br []
          img [ ClassName "profile-img"; Src blogInfo.ProfileImageUrl ] ]
      div
        [ ClassName "quote" ]
        [ str blogInfo.Bio ]
      
      menuItem "Posts" (Some (Posts Posts.Types.Page.AllPosts)) state.CurrentPage dispatcher
      menuItem "About" (Some Page.About) state.CurrentPage dispatcher ]

let main state dispatch = 
    match state.CurrentPage with
    | Some Page.About -> 
        About.View.render state.BlogInfo
    | Some (Posts postsPage) -> 
        Posts.View.render postsPage state.Posts (PostsMsg >> dispatch)
    | Some (Admin adminPage) -> 
        Admin.View.render adminPage state.Admin (AdminMsg >> dispatch)
    | None -> 
        div [ ] [ ]

let render state dispatch =
  match state.BlogInfo with
  | Empty -> div [ ] [ ] 
  | Loading -> div [ ] [ ]
  | LoadError ex -> h1 [ ] [ str "Error loading initial blog data" ]
  | Body blogInfo ->
     div
      [ ]
      [ div
          [ ClassName "sidebar" ]
          [ sidebar blogInfo state dispatch ]
        div
          [ ClassName "main-content" ]
          [ main state dispatch ] ]