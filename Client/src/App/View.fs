module App.View

open App.Types

open Fable.Helpers.React
open Fable.Helpers.React.Props

let menuItem label (page: Option<AppPage>) currentPage dispatcher =
    div
      [ classList  
         [ "menu-item", true
           "menu-item-selected", page = currentPage ] 
        OnClick (fun _ -> dispatcher (SetCurrentPage page)) ]
      [ str label ]

let sidebar state dispatcher =
  aside
    [ ClassName "fit-parent child-space"; Style [ TextAlign "center" ] ]
    [ div
        [ Style [ TextAlign "center" ] ]
        [ h3 [ Style [ Color "white" ] ] [ str state.BlogInfo.Value.Name ]
          br []
          img [ ClassName "profile-img"; Src state.BlogInfo.Value.ProfileImageUrl ] ]
      div
        [ ClassName "quote" ]
        [ str state.BlogInfo.Value.About ]
      
      menuItem "Posts" (Some Posts) state.CurrentPage dispatcher
      menuItem "About" (Some AppPage.About) state.CurrentPage dispatcher ]

let renderPage state dispatch = 
    match state.CurrentPage with
    | Some Posts -> 
        // The posts page
        Posts.View.render state.Posts (PostsMsg >> dispatch)
    | Some (Admin _) -> 
        // The Admin page
        Admin.View.render state.Admin (AdminMsg >> dispatch)
    | Some AppPage.About -> 
        // The about page
        h1 [] [ str "About" ]
    | None ->
        // Default when navigating to root 
        Posts.View.render state.Posts (PostsMsg >> dispatch)

let render state dispatch =
  if state.LoadingBlogInfo 
  then div [ ] [ ]
  elif not state.LoadingBlogInfo && state.BlogInfo.IsNone 
  then h1 [ ] [ str "Error loading initial blog data" ]
  else
  div
    [ ]
    [ div
        [ ClassName "sidebar" ]
        [ sidebar state dispatch ]
      div
        [ ClassName "main-content" ]
        [ renderPage state dispatch ] ]