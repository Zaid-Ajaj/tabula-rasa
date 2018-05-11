module Admin.Backoffice.EditArticle.State

open Shared
open Elmish
open Admin.Backoffice.EditArticle.Types

let init() = { ArticleToEdit = Empty }, Cmd.none
    
let update msg state = state, Cmd.none