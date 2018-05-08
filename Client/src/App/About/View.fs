module App.About.View

open Fable.Helpers.React
open Shared

let render (info: Remote<BlogInfo>) = 
    match info with
    | Loading -> Common.spinner
    | Empty -> div [ ] [ ]
    | LoadError msg -> h1 [ ] [ str msg ]
    | Body blogInfo ->
        div [ ] 
            [ Marked.marked [ Marked.Content blogInfo.About; 
                              Marked.Options [ Marked.Sanitize false ] ] ]