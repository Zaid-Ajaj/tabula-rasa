module Posts.View

open Posts.Types
open System
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Date.Local
open Shared.ViewModels

let spinner = 
  div 
    [ ClassName "cssload-container" ]
    [ div [ ClassName "cssload-whirlpool" ] [ ] ]

let formatDate (date : DateTime) = 
    sprintf "%d/%d/%d" date.Year date.Month date.Day
    
let postItem (post: BlogPostItem) = 
    div [ ClassName "card"; Style [ Padding 15; Margin 20 ] ]  
        [ h3 [] [ str post.Title ]
          p [ ] [ str (sprintf "Published %s" (formatDate post.DateAdded)) ] ] 
          
let render (model: Model) dispatch = 
    let title = h1 [ ClassName "title" ] [ str "Latest Posts" ]
    let body =
        match model.Error with
        | Some errorMsg -> 
            h1 [ Style [ Color "red" ] ] 
               [ str errorMsg ]
        | None ->
            if model.IsLoadingPosts 
            then spinner
            else div [ ] [ yield! List.map postItem model.Posts ]                
    div 
      [ ]
      [ title
        hr []
        body ]