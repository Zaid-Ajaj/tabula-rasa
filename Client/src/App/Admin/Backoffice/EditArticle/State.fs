module Admin.Backoffice.EditArticle.State

open Shared
open Elmish
open Elmish.Toastr
open Admin.Backoffice.EditArticle.Types
open Common

let init() = 
    { ArticleToEdit = Empty
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
        | SaveChanges -> 
            let nextState = { state with SavingChanges = true }
            let request = { Token = authToken; Body = article }
            let saveChangesCmd = 
                Cmd.fromAsync 
                    { Value = Server.api.savePostChanges request
                      Error = fun ex -> SaveChangesError "Network error while saving changes to blog post"
                      Success = function
                        | Ok true -> SavedChanges
                        | Error errorMsg -> SaveChangesError errorMsg 
                        | otherwise -> DoNothing }
            
            nextState, saveChangesCmd
        
        | SaveChangesError errorMsg -> 
            let nextState = { state with SavingChanges = false }
            nextState, Toastr.error (Toastr.message errorMsg)
        
        | SavedChanges ->
            let nextState = { state with SavingChanges = false }
            nextState, Toastr.success (Toastr.message "Changes have been successfully updated")
        
        | _ -> state, Cmd.none
    
    | _ -> 
        match msg with 
        | TogglePreview -> 
            let nextState = { state with Preview = not state.Preview }
            nextState, Cmd.none
        
        | LoadArticleToEdit postId -> 
            let nextState = { state with ArticleToEdit = Loading }
            let request = { Token = authToken; Body = postId }
            let successHandler = function 
                | Ok article -> ArticleLoaded article 
                | Error errorMsg -> LoadArticleError errorMsg
            nextState, Cmd.ofAsync Server.api.getPostById request successHandler (fun ex -> DoNothing) 
                             
        | LoadArticleError errorMsg ->  
            state, Toastr.error (Toastr.message errorMsg)
        
        | ArticleLoaded article -> 
            let nextState = { state with ArticleToEdit = Body article }
            nextState, Cmd.none

        | _ -> state, Cmd.none