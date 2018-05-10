module Admin.Backoffice.Articles.State

open Shared
open Elmish
open Admin.Backoffice.Articles.Types
open Fable.PowerPack

let init() = 
    let initState = 
       {  Articles = Remote.Empty
          DeletingArticle = None
          MakingDraft = None }
    initState, Cmd.none 

let update authToken msg state = 
    match msg with 
    | LoadArticles -> 
        let nextState = { state with Articles = Loading }
        nextState, Cmd.ofAsync Server.api.getPosts () ArticlesLoaded (fun ex -> LoadArticlesError "Network error while retrieving blog posts")
    
    | ArticlesLoaded articles -> 
        let nextState = { state with Articles = Body articles }
        nextState, Cmd.none
    
    | LoadArticlesError errorMsg ->
        state, Toastr.error (Toastr.message errorMsg)
    
    | AskPermissionToDeleteArticle articleId ->
        let renderModal() = 
            [ SweetAlert.Title "Are you sure you want to delete this article?"
              SweetAlert.Text "You will not be able to undo this action"
              SweetAlert.Type SweetAlert.ModalType.Question
              SweetAlert.CancelButtonEnabled true ] 
            |> SweetAlert.render 
            |> Promise.map (fun result -> result.value)

        let handleModal = function 
            | true -> DeleteArticle articleId
            | false ->  CancelArticleDeletion 

        state, Cmd.ofPromise renderModal () handleModal (fun _ -> DoNothing)        
    
    | CancelArticleDeletion -> 
        state, Toastr.info (Toastr.message "Delete operation was cancelled")
    
    | DeleteArticle articleId -> 
        let nextState = { state with DeletingArticle = Some articleId }
        let request = { Token = authToken; Body = articleId }
        let successHandler = function 
            | DeleteArticleResult.ArticleDeleted ->     
                ArticleDeleted 
            | DeleteArticleResult.AuthError (UserUnauthorized) -> 
                DeleteArticleError "User was unauthorized to delete the article"
            | DeleteArticleResult.ArticleDoesNotExist ->
                DeleteArticleError "It seems that the article does not exist any more"
            | DeleteArticleResult.DatabaseErrorWhileDeletingArticle ->
                DeleteArticleError "Internal error of the server's database while deleting the article"
        
        let deleteCmd = 
            Cmd.ofAsync 
                Server.api.deletePublishedArticleById request
                successHandler
                (fun _ -> DeleteArticleError "Network error while occured while deleting the article") 
        nextState,  deleteCmd   
    
    | ArticleDeleted -> 
        let nextState = { state with DeletingArticle = None }
        nextState, Cmd.batch [ Cmd.ofMsg LoadArticles; Toastr.success (Toastr.message "Article was deleted") ] 
     
    | DeleteArticleError errorMsg ->
        let nextState = { state with DeletingArticle = None }
        nextState, Toastr.error (Toastr.message errorMsg)
    
    | MakeIntoDraft articleId -> 
        let request = { Token = authToken; Body = articleId }
        let nextState = { state with MakingDraft = Some articleId }
        let successHandler = function 
            | MakeDraftResult.ArticleTurnedToDraft -> DraftMade
            | MakeDraftResult.ArticleDoesNotExist -> MakeDraftError "The article does not exist any more"
            | MakeDraftResult.AuthError UserUnauthorized -> MakeDraftError "User was unauthorized"
            | MakeDraftResult.DatabaseErrorWhileMakingDraft -> MakeDraftError "Internal error occured at the server's database while making draft"
        let cmd = 
            Cmd.ofAsync 
                Server.api.turnArticleToDraft request 
                successHandler
                (fun _ -> MakeDraftError "Network error occured while tuning the article into a draft")
        nextState, cmd
    
    | MakeDraftError errorMsg -> 
        let nextState = { state with DeletingArticle = None }
        nextState, Toastr.error (Toastr.message errorMsg)
    
    | DraftMade -> 
        let nextState = { state with MakingDraft = None }
        nextState, Cmd.batch [ Cmd.ofMsg LoadArticles; Toastr.success (Toastr.message "Article was turned into a draft") ]
    
    | DoNothing ->
        state, Cmd.none