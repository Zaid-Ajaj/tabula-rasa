module Admin.Backoffice.PublishedPosts.State

open Shared
open Elmish
open Admin.Backoffice.PublishedPosts.Types
open Fable.PowerPack

let init() = 
    let initState = 
       {  PublishedPosts = Remote.Empty
          DeletingPost = None
          MakingDraft = None }
    initState, Cmd.none 

let update authToken msg state = 
    match msg with 
    | LoadPublishedPosts -> 
        let nextState = { state with PublishedPosts = Loading }
        nextState, Cmd.ofAsync Server.api.getPosts () PublishedPostsLoaded (fun ex -> LoadPublishedPostsError "Network error while retrieving blog posts")
    
    | PublishedPostsLoaded articles -> 
        let nextState = { state with PublishedPosts = Body articles }
        nextState, Cmd.none
    
    | LoadPublishedPostsError errorMsg ->
        state, Toastr.error (Toastr.message errorMsg)
    
    | AskPermissionToDeletePost postId ->
        let renderModal() = 
            [ SweetAlert.Title "Are you sure you want to delete this article?"
              SweetAlert.Text "You will not be able to undo this action"
              SweetAlert.Type SweetAlert.ModalType.Question
              SweetAlert.CancelButtonEnabled true ] 
            |> SweetAlert.render 
            |> Promise.map (fun result -> result.value)

        let handleModal = function 
            | true -> DeletePost postId
            | false ->  CancelPostDeletion 

        state, Cmd.ofPromise renderModal () handleModal (fun _ -> DoNothing)        
    
    | CancelPostDeletion -> 
        state, Toastr.info (Toastr.message "Delete operation was cancelled")
    
    | DeletePost postId -> 
        let nextState = { state with DeletingPost = Some postId }
        let request = { Token = authToken; Body = postId }
        let successHandler = function 
            | DeleteArticleResult.ArticleDeleted ->     
                PostDeleted 
            | DeleteArticleResult.AuthError (UserUnauthorized) -> 
                DeletePostError "User was unauthorized to delete the article"
            | DeleteArticleResult.ArticleDoesNotExist ->
                DeletePostError "It seems that the article does not exist any more"
            | DeleteArticleResult.DatabaseErrorWhileDeletingArticle ->
                DeletePostError "Internal error of the server's database while deleting the article"
        
        let deleteCmd = 
            Cmd.ofAsync 
                Server.api.deletePublishedArticleById request
                successHandler
                (fun _ -> DeletePostError "Network error while occured while deleting the article") 
        nextState,  deleteCmd   
    
    | PostDeleted -> 
        let nextState = { state with DeletingPost = None }
        nextState, Cmd.batch [ Cmd.ofMsg LoadPublishedPosts; Toastr.success (Toastr.message "Article was deleted") ] 
     
    | DeletePostError errorMsg ->
        let nextState = { state with DeletingPost = None }
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
        let nextState = { state with DeletingPost = None }
        nextState, Toastr.error (Toastr.message errorMsg)
    
    | DraftMade -> 
        let nextState = { state with MakingDraft = None }
        nextState, Cmd.batch [ Cmd.ofMsg LoadPublishedPosts; Toastr.success (Toastr.message "Article was turned into a draft") ]
    
    | EditPost postId ->    
        state, Cmd.none
    
    | DoNothing ->
        state, Cmd.none