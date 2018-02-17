module Admin.Backoffice.NewArticle.State

open Elmish
open Admin.Backoffice.NewArticle.Types

let init() = 
    let initialState : NewArticleState = {
        Content = ""
        Slug = ""
        Tags = []
        Title = ""
        Preview = false
    }

    initialState, Cmd.none


let update msg state = 
    match msg with 
    | SetTitle title ->
        let nextState = { state with Title = title }
        nextState, Cmd.none

    | SetSlug slug ->
        let nextState = { state with Slug = slug }
        nextState, Cmd.none
    
    | SetContent content ->
        let nextState = { state with Content = content }
        nextState, Cmd.none
        
    | TogglePreview -> 
        let nextState = { state with Preview = not state.Preview }
        nextState, Cmd.none 

    | AddTag tag ->
        let existingTag = List.tryFind ((=) tag) state.Tags
        match existingTag with
        | Some _ -> state, Cmd.none
        | None -> 
            let nextTags = tag :: state.Tags
            let nextState = { state with Tags = nextTags }
            nextState, Cmd.none 