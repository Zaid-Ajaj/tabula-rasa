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
          OnClick (fun _ -> dispatch (ReadPost post.Slug)) ]  
        [ h3 [] [ str post.Title ]
          p [ ] [ str (sprintf "Published %s" (formatDate post.DateAdded)) ] ] 

let posts (state: State) dispatch = 
    let sortedPosts = List.sortByDescending (fun post -> post.DateAdded) state.Posts
    let allPosts = List.map (fun post -> postItem post dispatch) sortedPosts
    if state.IsLoadingPosts 
    then spinner
    else div [ ] [ yield! allPosts ]
             
let allPostsPage state dispatch =
    let title = h1 [ ClassName "title" ] [ str "Latest Posts" ]
    let body =
      match state.Error with
      | Some errorMsg -> 
          h1 [ Style [ Color "red" ] ] 
             [ str errorMsg ]
      | None -> posts state dispatch
            
    div [ ] [ title; hr []; body ]   

open React.Marked

let render currentPage (state: State) dispatch = 
    match currentPage with
    | AllPosts -> allPostsPage state dispatch
    | Post _ -> 
        if state.IsLoadingSinglePost 
        then spinner
        else match state.PostContent with 
             | Some content -> marked [ Content content ]
             | None -> div [ ] [ ]
    

    
    
    
    
    
    
    
    