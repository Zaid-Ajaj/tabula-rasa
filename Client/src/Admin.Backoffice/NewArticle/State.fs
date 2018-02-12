module Admin.Backoffice.NewArticle.State

open Elmish
open Admin.Backoffice.NewArticle.Types

let init() = 
    let initialState : NewArticleState = {
        Content = ""
        Summary = ""
        Slug = ""
        Tags = []
        Title = ""
    }

    initialState, Cmd.none


let update msg state = state, Cmd.none