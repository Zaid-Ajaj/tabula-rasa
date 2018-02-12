module App.View

open App.Types

open Fable.Helpers.React
open Fable.Helpers.React.Props

let menuItem label page currentPage dispatcher =
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
      
      menuItem "Posts" Posts state.CurrentPage dispatcher
      menuItem "About" AppPage.About state.CurrentPage dispatcher ]

let renderPage state dispatch = 
    match state.CurrentPage with
    | Posts -> 
        Posts.View.render state.Posts (PostsMsg >> dispatch)
    | Admin _ -> 
        Admin.View.render state.Admin (AdminMsg >> dispatch)
    | AppPage.About -> h1 [] [ str "About" ]

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