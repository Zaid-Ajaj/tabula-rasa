module App.About.View

open Fable.Helpers.React
open React.Marked
open Shared.ViewModels

let render (info: Remote<BlogInfo>) = 
    match info with
    | Loading -> Posts.View.spinner
    | Empty -> div [ ] [ ]
    | LoadError msg -> h1 [ ] [ str msg ]
    | Body blogInfo ->
        div [ ] 
            [ marked [ Content blogInfo.About; Options [ Sanitize false ] ] ]