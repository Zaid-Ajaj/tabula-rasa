module App.View

open App.Types
open Shared
open Fable.Helpers.React
open Fable.Helpers.React.Props
open React.Responsive

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

let main state dispatch  = 
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

let desktopApp blogInfo state dispatch = 
  div [ ]
      [ div [ ClassName "sidebar" ]
            [ sidebar blogInfo state dispatch ]
        div [ ClassName "main-content" ]
            [ main state dispatch ] ] 

/// The mobile app view will be a simpler implementation
/// with no backoffice, just the posts and about page         
let mobileApp blogInfo state dispatch = 
  match state.CurrentPage with 
  | None -> div [ ] [ ] 
  | Some page -> 
      match page with 
      | Page.About -> 
          div [ ]
              [ mobileHeader blogInfo state dispatch
                div [ Style [ Padding 20 ] ] 
                    [ About.View.render (Body blogInfo) ] ]
      
      | Page.Posts (Posts.Types.Page.AllPosts) -> 
          // when viewing all posts, the same main view is re-used for mobile
          div [ ]
              [ mobileHeader blogInfo state dispatch
                div [ Style [ Padding 20 ] ] 
                    [ main state dispatch ] ]
      
      | Page.Posts (Posts.Types.Page.Post postSlug) ->
          match state.Posts.Post with 
          | Remote.Empty -> div [ ] [ ] 
          | Loading -> Common.spinner
          | LoadError error -> Common.errorMsg error 
          | Body post -> 
              let goBackButton = 
                button 
                  [ ClassName "btn btn-success"
                    OnClick (fun _ -> dispatch (NavigateTo (Posts Posts.Types.Page.AllPosts))) ] 
                  [ span [ ] [ i [ ClassName "fa fa-arrow-left"; Style [ Margin 5 ] ] [ ]; str "Go Back" ] ]
              
              div [ Style [ Padding 20 ] ] 
                  [ div [ ClassName "row" ] [ h4 [ Style[Margin 20] ] [ str blogInfo.Name ];  goBackButton ]
                    hr [ ] 
                    Marked.marked [ Marked.Content post.Content; 
                                    Marked.Options [ Marked.Sanitize false ] ]  ]

        | otherPage ->
          div [ ] [
            str "map other pages to just an empty view"
            br [ ]
            str "because we don't to support backoffice from mobile"
            br [ ]
            str "at least for now..."
          ]

  
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
