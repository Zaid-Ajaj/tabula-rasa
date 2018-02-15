module Posts.View

open Posts.Types

open Fable.Helpers.React
open Fable.Helpers.React.Props

let spinner = 
  div 
    [ ClassName "cssload-container" ]
    [ div [ ClassName "cssload-whirlpool" ] [ ] ]

let render (model: Model) dispatch = 
    let title = h1 [ ClassName "title" ] [ str "Latest Posts" ]
    let body =
        match model.Error with
        | Some errorMsg -> 
            h1 [ Style [ Color "red" ] ] 
               [ str errorMsg ]
        | None ->
            match model.IsLoadingPosts with
            | true -> spinner
                 // h1 [ Style [ Color "lightblue" ] ] [ str "Loading...!" ]
            | false -> 
                div     
                  [ ]
                  [ for post in model.Posts -> 
                        div []  
                            [ h3 [] [ str post ] ] ]
    div 
      [ ]
      [ title
        hr []
        body ]