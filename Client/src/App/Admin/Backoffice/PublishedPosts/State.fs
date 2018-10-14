module Admin.Backoffice.PublishedPosts.State

open Shared
open Elmish
open Elmish.SweetAlert
open Admin.Backoffice.PublishedPosts.Types
open Fable.PowerPack
open Fable
open Urls
open Common 

let init() = 
    let initState = 
       {  PublishedPosts = Remote.Empty
          DeletingPost = None
          MakingDraft = None
          IsTogglingFeatured = None }
    initState, Cmd.none 

let update authToken msg state = 
    match msg with 
    | LoadPublishedPosts -> 
        let nextState = { state with PublishedPosts = Loading }
        let nextCmd = 
            Cmd.ofAsync
              Server.api.getPosts ()
              (Ok >> LoadedPublishedPosts)
              (fun ex -> LoadedPublishedPosts (Error "Network error while retrieving blog posts"))
        nextState,  nextCmd
    
    | LoadedPublishedPosts (Ok loadedPosts) -> 
        let nextState = { state with PublishedPosts = Body loadedPosts }
        nextState, Cmd.none
    
    | LoadedPublishedPosts (Error errorMsg) ->
        let nextState = { state with PublishedPosts = LoadError errorMsg }
        nextState, Toastr.error (Toastr.message errorMsg)
    
    | AskPermissionToDeletePost postId ->
        let handleConfirm = function
        | ConfirmAlertResult.Confirmed -> DeletePost postId 
        | ConfirmAlertResult.Dismissed reason -> CancelPostDeletion
        
        let confirmAlert = 
            ConfirmAlert("You will not be able to undo this action", handleConfirm)
                .Title("Are you sure you want to delete this article?")
                .Type(AlertType.Question)
        
        state, SweetAlert.Run(confirmAlert)   
    
    | CancelPostDeletion -> 
        state, Toastr.info (Toastr.message "Delete operation was cancelled")
    
    | DeletePost postId -> 
        let nextState = { state with DeletingPost = Some postId }
        let request = { Token = authToken; Body = postId }
        let successHandler = function 
            | Error authError -> 
                DeletePostError "User was unauthorized to delete the article"
            | Ok DeletePostResult.PostDeleted ->     
                PostDeleted 
            | Ok DeletePostResult.PostDoesNotExist ->
                DeletePostError "It seems that the article does not exist any more"
            | Ok DeletePostResult.DatabaseErrorWhileDeletingPost ->
                DeletePostError "Internal error of the server's database while deleting the article"
        
        let deleteCmd = 
            Cmd.ofAsync 
                Server.api.deletePublishedArticleById request
                successHandler
                (fun _ -> DeletePostError "Network error while occured while deleting the article") 
        nextState,  deleteCmd   
    
    | PostDeleted -> 
        let nextState = { state with DeletingPost = None }
        nextState, Cmd.batch [ Cmd.ofMsg LoadPublishedPosts ; 
                               Toastr.success (Toastr.message "Article was deleted") ] 
     
    | DeletePostError errorMsg ->
        let nextState = { state with DeletingPost = None }
        nextState, Toastr.error (Toastr.message errorMsg)
    
    | MakeIntoDraft articleId -> 
        let request = { Token = authToken; Body = articleId }
        let nextState = { state with MakingDraft = Some articleId }
        let successHandler = function 
            | Error authError -> MakeDraftError "User was unauthorized"
            | Ok MakeDraftResult.ArticleTurnedToDraft -> DraftMade
            | Ok MakeDraftResult.ArticleDoesNotExist -> MakeDraftError "The article does not exist any more"
            | Ok MakeDraftResult.DatabaseErrorWhileMakingDraft -> MakeDraftError "Internal error occured at the server's database while making draft"
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
        state, Urls.navigate [ Urls.admin; Urls.editPost; string postId ]
    
    | ToggleFeatured postId ->
        let nextState = { state with IsTogglingFeatured = Some postId }
        let request = { Token = authToken; Body = postId }
        let toggleFeatureCmd = 
            Cmd.fromAsync {
                Value = Server.api.togglePostFeatured request
                Error = fun ex -> ToggleFeaturedFinished (Error "Network error while toggling post featured")
                Success = function 
                    | Error authError -> ToggleFeaturedFinished (Error "User was unauthorized")
                    | Ok toggleResult -> ToggleFeaturedFinished toggleResult
            } 

        nextState, toggleFeatureCmd
    
    | ToggleFeaturedFinished (Ok msg) -> 
        match state.IsTogglingFeatured, state.PublishedPosts with 
        | Some postId, Body loadedPosts -> 
            let nextCmd = Toastr.success (Toastr.message msg)
            // update the posts 
            let updatedPosts = 
                loadedPosts
                |> List.map (fun post ->    
                    if post.Id <> postId then post
                    else { post with Featured = not post.Featured }) 
            let nextState = 
                { state with 
                    IsTogglingFeatured = None 
                    PublishedPosts = Body updatedPosts  }
                  
            nextState, nextCmd

        | _, _ -> state, Cmd.none 
    
    | ToggleFeaturedFinished (Error msg) ->
        let nextState = { state with IsTogglingFeatured = None }
        nextState, Toastr.error (Toastr.message msg) 

    | DoNothing ->
        state, Cmd.none