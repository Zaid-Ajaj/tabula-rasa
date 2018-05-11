module Admin.Backoffice.Drafts.State

open Shared
open Admin.Backoffice.Drafts.Types 
open Elmish 
open Fable.PowerPack

let init () = 
    { Drafts = Empty
      PublishingDraft = None
      DeletingDraft = None }, Cmd.none

let canTakeAction state = 
    match state.PublishingDraft, state.DeletingDraft with 
    | None, None -> true 
    | _ -> true

let update authToken (msg: Msg) (state: State) = 
    match msg with 
    | LoadDrafts -> 
        let nextState = { state with Drafts = Loading }
        nextState, Cmd.ofAsync Server.api.getDrafts (AuthToken(authToken)) 
                               (function 
                                | Ok drafts -> DraftsLoaded drafts
                                | Error msg ->  AuthenticationError msg) 
                               DraftsLoadingError
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
        let renderModal() = 
            [ SweetAlert.Title "Are you sure you want to delete this draft?"
              SweetAlert.Text "You will not be able to undo this action"
              SweetAlert.Type SweetAlert.ModalType.Question
              SweetAlert.CancelButtonEnabled true ] 
            |> SweetAlert.render 
            |> Promise.map (fun result -> result.value)

        let handleModal = function 
            | true -> DeleteDraft draftId
            | false ->  CancelDraftDeletion 

        state, Cmd.ofPromise renderModal () handleModal (fun ex -> NoOp)

    | DeleteDraft draftId ->
        let request = { Token = authToken; Body = draftId }
        let successHandler = function 
            | DeleteDraftResult.DraftDeleted -> 
                DraftDeleted 
            | DeleteDraftResult.AuthError (UserUnauthorized) ->
                DeleteDraftError "User was unauthorized"
            | DeleteDraftResult.DraftDoesNotExist ->
                DeleteDraftError "Draft does not seem to be in the database anymore"
            | DeleteDraftResult.DatabaseErrorWhileDeletingDraft ->
                DeleteDraftError "Internal error of the server's database while publishing draft"
        
        let deleteCmd = 
            Cmd.ofAsync Server.api.deleteDraftById request 
                successHandler 
                (fun ex -> DeleteDraftError "Network error occured while publishing the draft")
        
        let nextState = { state with DeletingDraft = Some draftId }
        nextState, deleteCmd
    | DraftDeleted ->
       
        state, Cmd.batch [ Toastr.success (Toastr.message "Draft deleted")
                           Cmd.ofMsg LoadDrafts ] 
    | PublishDraft _ when not (canTakeAction state) ->
        state, Toastr.info (Toastr.message "An action is already taking place")
    | PublishDraft draftId ->
        let request = { Token = authToken; Body = draftId }
        let successHandler = function 
            | PublishDraftResult.DraftPublished -> 
                DraftPublished
            | PublishDraftResult.AuthError (UserUnauthorized) -> 
                PublishDraftError "User is not authorized" 
            | PublishDraftResult.DatabaseErrorWhilePublishingDraft ->
                PublishDraftError "Internal error of the server's database while publishing draft"
            | PublishDraftResult.DraftDoesNotExist ->
                PublishDraftError "The draft does not exist anymore" 

        let publishCmd = 
            Cmd.ofAsync Server.api.publishDraft request
                successHandler 
                (fun ex -> PublishDraftError "Network error occured while publishing the draft")
        
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
        state, Toastr.info (Toastr.message "Delete operation was cancelled")
    | EditDraft draftId ->
        state, Cmd.none
    | NoOp ->
        state, Cmd.none