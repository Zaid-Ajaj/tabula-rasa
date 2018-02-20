module Posts.View

open Posts.Types
open System
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Shared.ViewModels
open React

let spinner = 
  div 
    [ ClassName "cssload-container" ]
    [ div [ ClassName "cssload-whirlpool" ] [ ] ]

let formatDate (date : DateTime) = 
    sprintf "%d/%d/%d" date.Year date.Month date.Day
    
let postItem (post: BlogPostItem) dispatch = 
    div [ ClassName "card blogpost"; 
          Style [ Padding 15; Margin 20 ]
          OnClick (fun _ -> dispatch (NavigateToPost post.Slug)) ]  
        [ h3 [] [ str post.Title ]
          p [ ] [ str (sprintf "Published %s" (formatDate post.DateAdded)) ] ] 
             
let latestPosts (blogPosts : list<BlogPostItem>) dispatch =
    let title = h1 [ ClassName "title" ] [ str "Latest Posts" ]
    let body =
      let sortedPosts = List.sortByDescending (fun post -> post.DateAdded) blogPosts
      let allPosts = List.map (fun post -> postItem post dispatch) sortedPosts
      div [ ] [ yield! allPosts ] 
            
    div [ ] [ title; hr []; body ]   

open React.Marked

let render currentPage (state: State) dispatch = 
    match currentPage with
    | AllPosts -> 
        match state.LatestPosts with
        | Body posts -> latestPosts posts dispatch
        | Loading -> spinner
        | Empty -> div [ ] [ ]
        | LoadError msg -> h1 [ ] [ str msg ]  
    | Post _ -> 
        match state.PostContent with
        | Body post -> marked [ Content post ]
        | Loading -> spinner
        | Empty -> div [ ] [ ]
        | LoadError msg -> h1 [ ] [ str msg ]