module Admin.Backoffice.EditArticle.State

open Shared
open Elmish
open Admin.Backoffice.EditArticle.Types

let init() = 
    { ArticleId = None
      ArticleToEdit = Empty
      Preview = false
      SavingChanges = false }, Cmd.none
    
let update authToken msg state = 
    match state.ArticleToEdit with
    | Body article  ->
        match msg with 
        | SetSlug slug -> { state with ArticleToEdit = Body ({ article with Slug = slug })  }, Cmd.none
        | SetTitle title -> { state with ArticleToEdit = Body ({ article with Title = title }) }, Cmd.none 
        | SetContent content -> { state with ArticleToEdit = Body ({ article with Content = content }) }, Cmd.none 
        | AddTags tags -> { state with ArticleToEdit = Body ({ article with Tags = List.ofArray tags }) }, Cmd.none 
        | TogglePreview -> { state with Preview = not state.Preview }, Cmd.none
        | _ -> state, Cmd.none
    | _ -> 
        match msg with 
        | TogglePreview ->  { state with Preview = not state.Preview }, Cmd.none
        | LoadArticleToEdit -> 
            match state.ArticleId with 
            | None -> state, Toastr.error (Toastr.message "No article was selected")
            | Some articleId -> 
                let nextState = { state with ArticleToEdit = Loading }
                let request = { Token = authToken; Body = articleId }
                let successHandler = function 
                    | Ok article -> ArticleLoaded article 
                    | Error errorMsg -> LoadArticleError errorMsg
                nextState, Cmd.ofAsync Server.api.getPostById request successHandler (fun ex -> DoNothing) 
                             
        | LoadArticleError errorMsg ->  
            state, Toastr.error (Toastr.message errorMsg)
        
        | ArticleLoaded article -> 
            let nextState = { state with ArticleToEdit = Body article }
            nextState, Toastr.success (Toastr.message "Article loaded")

        | _ -> state, Cmd.none