module App.View

open App.Types
open App.State

open Fable.Helpers.React
open Fable.Helpers.React.Props

let menuItem label page currentPage dispatcher =
    div
      [ classList  
         [ "menu-item", true
           "menu-item-selected", page = currentPage ] 
        OnClick (fun e -> dispatcher (ViewPage page)) ]
      [ str label ]

let sidebar currentPage dispatcher =
  aside
    [ ClassName "fit-parent child-space"; Style [ TextAlign "center" ] ]
    [ div 
        [ Style [ TextAlign "center" ] ]
        [ h3 [ Style [ Color "white" ] ] [ str "Zaid Ajaj" ]
          img [ ClassName "profile-img"; Src "/img/default-cuteness.jpg" ] ]
      div 
        [ ClassName "quote" ]
        [ str "F# enthusiast, interested in all kinds of metaprogramming, Coffee Driven Developement, writing and learning just about everything." ]
      
      menuItem "Posts" Posts currentPage dispatcher
      menuItem "About" Page.About currentPage dispatcher ]

let renderPage state dispatch = 
    match state.CurrentPage with
    | Posts -> Posts.View.render state.Posts (PostsMsg >> dispatch)
    | Admin -> Admin.View.render state.Admin (AdminMsg >> dispatch)
    | Page.About -> h1 [] [ str "About" ]

let render state dispatch =
  div
    [ ]
    [ div
        [ ClassName "sidebar" ]
        [ sidebar state.CurrentPage dispatch ]
      div
        [ ClassName "main-content" ]
        [ renderPage state dispatch ] ]