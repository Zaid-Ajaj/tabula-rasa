module Admin.Backoffice.NewArticle.View

open Admin.Backoffice.NewArticle.Types

open Fable.Helpers.React.Props
open Fable.Helpers.React

let render (model : NewArticleState) dispatch = 
    h1 [ ] [ str "Edit me" ]