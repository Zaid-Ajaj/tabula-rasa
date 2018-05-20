module App.View

open App.Types
open Shared
open Fable.Helpers.React
open Fable.Helpers.React.Props
open React.Responsive

type Screen = Desktop | Mobile 

let menuItem label page currentPage dispatcher =
    div
      [ classList  
         [ "menu-item", true
           "menu-item-selected", Some page = currentPage ] 
        OnClick (fun _ -> dispatcher (NavigateTo page)) ]
      [ str label ]

/// Menu items that are only shown when an admin is logged in
let adminMenuItems state dispatch = 
  [ menuItem "Home" (Page.Admin (Admin.Types.Page.Backoffice (Admin.Backoffice.Types.Page.Home))) state.CurrentPage dispatch
    menuItem "Published Posts" (Page.Admin (Admin.Types.Page.Backoffice (Admin.Backoffice.Types.Page.PublishedPosts))) state.CurrentPage dispatch 
    menuItem "Drafts" (Page.Admin (Admin.Types.Page.Backoffice (Admin.Backoffice.Types.Page.Drafts))) state.CurrentPage dispatch 
    menuItem "New Article" (Page.Admin (Admin.Types.Page.Backoffice (Admin.Backoffice.Types.Page.NewPost))) state.CurrentPage dispatch 
    menuItem "Settings" (Page.Admin (Admin.Types.Page.Backoffice (Admin.Backoffice.Types.Page.Settings))) state.CurrentPage dispatch ]

let sidebar (blogInfo: BlogInfo) state dispatch =
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
      
      menuItem "Posts" (Posts Posts.Types.Page.AllPosts) state.CurrentPage dispatch
      menuItem "About" (Page.About) state.CurrentPage dispatch 
      ofList [ if state.Admin.SecurityToken.IsSome then yield! adminMenuItems state dispatch ] ]

let mobileHeader (blogInfo: BlogInfo) state dispatch = 
  let navButton label selectedPage  = 
    let foreground = 
      if Some selectedPage = state.CurrentPage 
      then "lightgreen" else "white"
    div 
      [ classList [ "btn btn-default", true 
                    "btn-success", Some selectedPage = state.CurrentPage  ]
        Style [ Margin 10; Width "40%";  ]
        OnClick (fun _ -> dispatch (NavigateTo selectedPage)) ]
      [ str label ] 
  div 
   [ ClassName "mobile-header" ]
   [ h3 [ ] [ str blogInfo.Name ]
     img [ ClassName "profile-img"; Src blogInfo.ProfileImageUrl ]
     div [ ] [ str blogInfo.Bio ]
     div [ ClassName "col-xs-12"
           Style [ TextAlign "center" ] ] 
           [ span [ ] 
                  [ navButton "Posts" (Page.Posts Posts.Types.Page.AllPosts) 
                    navButton "About" (Page.About)  ] ] ] 

let main state dispatch screen = 
    match screen with 
    | Desktop -> 
      match state.CurrentPage with
      | Some Page.About -> 
          About.View.render state.BlogInfo
      | Some (Posts postsPage) -> 
          let isAdminLoggedIn = state.Admin.SecurityToken.IsSome
          Posts.View.render postsPage isAdminLoggedIn state.Posts (PostsMsg >> dispatch)
      | Some (Admin adminPage) -> 
          Admin.View.render adminPage state.Admin (AdminMsg >> dispatch)
      | None -> 
          div [ ] [ ]
    
    | Mobile -> 
      match state.CurrentPage with
      | Some Page.About -> 
          About.View.render state.BlogInfo
      | Some (Posts postsPage) -> 
          let isAdminLoggedIn = state.Admin.SecurityToken.IsSome
          Posts.View.render postsPage isAdminLoggedIn state.Posts (PostsMsg >> dispatch)
      | _ -> div [ ] [ ] 

let desktopApp blogInfo state dispatch = 
  div [ ]
      [ div [ ClassName "sidebar" ]
            [ sidebar blogInfo state dispatch ]
        div [ ClassName "main-content" ]
            [ main state dispatch Desktop ] ] 
         
let mobileApp blogInfo state dispatch = 
  div [ ]
      [ mobileHeader blogInfo state dispatch
        div [ Style [ Padding 20 ] ] 
            [ main state dispatch Mobile ] ]
  
let app blogInfo state dispatch =
  div 
   [ ]
   [ mediaQuery 
      [ MinWidth 601 ]
      [ desktopApp blogInfo state dispatch ]
     mediaQuery 
      [ MaxWidth 600 ] 
      [ mobileApp blogInfo state dispatch ] ]

let render state dispatch =
  match state.BlogInfo with
  | Remote.Empty -> div [ ] [ ] 
  | Loading -> div [ ] [ ]
  | LoadError ex -> h1 [ ] [ str "Error loading initial blog data" ]
  | Body blogInfo -> app blogInfo state dispatch    