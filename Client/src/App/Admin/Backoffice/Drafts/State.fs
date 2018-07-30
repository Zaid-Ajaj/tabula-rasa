module Admin.Backoffice.Drafts.State

open Shared
open Admin.Backoffice.Drafts.Types 
open Elmish 
open Elmish.SweetAlert
open Fable.PowerPack
open Common

let init () = 
    { Drafts = Empty
      PublishingDraft = None
      DeletingDraft = None
      IsTogglingFeatured = None }, Cmd.none

let canTakeAction state = 
    match state.PublishingDraft, state.DeletingDraft, state.IsTogglingFeatured with 
    | None, None, None -> true 
    | _ -> false

let update authToken (msg: Msg) (state: State) = 
    match msg with 
    | LoadDrafts -> 
        let nextState = { state with Drafts = Loading }
        let loadDraftsCmd = 
            Cmd.fromAsync 
              { Value = Server.api.getDrafts (SecurityToken(authToken)) 
                Error = fun ex -> DraftsLoadingError ex 
                Success = function
                   | Ok drafts -> DraftsLoaded drafts
                   | Error msg -> AuthenticationError msg }

        nextState, loadDraftsCmd

    | DraftsLoaded draftsFromServer ->
        let nextState = { state with Drafts = Body draftsFromServer }
        nextState, Cmd.none 
    
    | DraftsLoadingError error -> 
        let nextState = { state with Drafts = LoadError error.Message }
        nextState, Cmd.none 
    
    | AuthenticationError error ->
        state, Toastr.error (Toastr.message error)
    
    | AskPermissionToDeleteDraft _ when not (canTakeAction state) ->
        state, Toastr.info (Toastr.message "An action is already taking place") 
    
    | AskPermissionToDeleteDraft draftId -> 
        let handleConfirm = function
        | ConfirmAlertResult.Confirmed -> DeleteDraft draftId 
        | ConfirmAlertResult.Dismissed reason -> CancelDraftDeletion

        let confirmAlert = 
            ConfirmAlert("You will not be able to undo this action", handleConfirm)
                .Title("Are you sure you want to delete this draft?")
                .Type(AlertType.Question)

        state, SweetAlert.Run(confirmAlert)   

    | DeleteDraft draftId ->
        let request = { Token = authToken; Body = draftId }
        let deleteCmd = 
            Cmd.fromAsync 
                { Value = Server.api.deleteDraftById request
                  Error = fun ex -> DeleteDraftError "Network error occured while publishing the draft"
                  Success = function
                    | DeleteDraftResult.DraftDeleted -> 
                          DraftDeleted 
                    | DeleteDraftResult.AuthError (UserUnauthorized) ->
                        DeleteDraftError "User was unauthorized"
                    | DeleteDraftResult.DraftDoesNotExist ->
                        DeleteDraftError "Draft does not seem to be in the database anymore"
                    | DeleteDraftResult.DatabaseErrorWhileDeletingDraft ->
                        DeleteDraftError "Internal error of the server's database while publishing draft" }
        
        let nextState = { state with DeletingDraft = Some draftId }
        nextState, deleteCmd
    
    | DraftDeleted ->   
        state, Cmd.batch [ Toastr.success (Toastr.message "Draft deleted")
                           Cmd.ofMsg LoadDrafts ] 
    
    | PublishDraft _ when not (canTakeAction state) ->
        state, Toastr.info (Toastr.message "An action is already taking place")
    
    | PublishDraft draftId ->
        let request = { Token = authToken; Body = draftId }
        let publishCmd = 
            Cmd.fromAsync 
                { Value = Server.api.publishDraft request
                  Error = fun ex -> PublishDraftError "Network error occured while publishing the draft"
                  Success = function 
                    | PublishDraftResult.DraftPublished -> 
                        DraftPublished
                    | PublishDraftResult.AuthError (UserUnauthorized) -> 
                        PublishDraftError "User is not authorized" 
                    | PublishDraftResult.DatabaseErrorWhilePublishingDraft ->
                        PublishDraftError "Internal error of the server's database while publishing draft"
                    | PublishDraftResult.DraftDoesNotExist ->
                        PublishDraftError "The draft does not exist anymore" }
        
        let nextState = { state with PublishingDraft = Some draftId }
        nextState, publishCmd
    
    | DeleteDraftError errorMsg -> 
        let nextState = { state with DeletingDraft = None }
        nextState, Toastr.error (Toastr.message errorMsg)
    
    | DraftPublished ->
        let nextState = { state with PublishingDraft = None }
        nextState, Cmd.batch [ Cmd.ofMsg LoadDrafts // reload
                               Toastr.success (Toastr.message "Draft published") ]
    
    | PublishDraftError errorMsg ->
        let nextState = { state with PublishingDraft = None }
        nextState, Toastr.error (Toastr.message errorMsg)
    
    | CancelDraftDeletion -> 
        state, Toastr.info (Toastr.message "Phew, so you changed your mind after all.")
    
    | EditDraft draftId ->
        state, Urls.navigate [ Urls.admin; Urls.editPost; string draftId ]

    | ToggleFeatured postId ->
        let nextState = { state with IsTogglingFeatured = Some postId }
        let request = { Token = authToken; Body = postId }
        let nextCmd = 
            Cmd.fromAsync
                { Value = Server.api.togglePostFeatured request
                  Error = fun ex -> ToggleFeaturedFinished (Error "Network error while toggling post featured")
                  Success = function 
                    | Ok successMsg -> ToggleFeaturedFinished (Ok successMsg)
                    | Error errorMsg -> ToggleFeaturedFinished (Error errorMsg) } 
        
        nextState, nextCmd
    
    | ToggleFeaturedFinished (Ok msg) -> 
        match state.IsTogglingFeatured, state.Drafts with 
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
                    Drafts = Body updatedPosts  }
                  
            nextState, nextCmd

        | _, _ -> state, Cmd.none 
    
    | ToggleFeaturedFinished (Error msg) ->
        let nextState = { state with IsTogglingFeatured = None }
        nextState, Toastr.error (Toastr.message msg)     
            
    | DoNothing ->
        state, Cmd.none