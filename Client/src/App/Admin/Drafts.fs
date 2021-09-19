module Drafts

open Shared
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Elmish
open Elmish.SweetAlert
open Fable.PowerPack
open Common

type State =
    { Drafts: Remote<list<BlogPostItem>>
      PublishingDraft: Option<int>
      DeletingDraft: Option<int>
      IsTogglingFeatured: Option<int> }

type Msg =
    | LoadDrafts
    | DraftsLoaded of list<BlogPostItem>
    | DraftsLoadingError of exn
    | AuthenticationError of string
    | AskPermissionToDeleteDraft of draftId: int
    | DeleteDraft of draftId: int
    | CancelDraftDeletion
    | PublishDraft of draftId: int
    | DraftPublished
    | PublishDraftError of string
    | DraftDeleted
    | DeleteDraftError of string
    | EditDraft of draftId: int
    | ToggleFeatured of postId: int
    | ToggleFeaturedFinished of Result<string, string>
    | DoNothing


let draftActions isDeleting isPublishing (draft: BlogPostItem) dispatch =
    [ button [ ClassName "btn btn-info"
               OnClick(fun _ -> dispatch (EditDraft draft.Id))
               Style [ Margin 5 ] ] [
        span [] [
            Common.icon false "edit"
            str "Edit"
        ]
      ]
      button [ ClassName "btn btn-success"
               OnClick(fun _ -> dispatch (PublishDraft draft.Id))
               Style [ Margin 5 ] ] [
          span [] [
              Common.icon isPublishing "rocket"
              str "Publish"
          ]
      ]
      button [ ClassName "btn btn-danger"
               Style [ Margin 5 ]
               OnClick(fun _ -> dispatch (AskPermissionToDeleteDraft draft.Id)) ] [
          span [] [
              Common.icon isDeleting "times"
              str "Delete"
          ]
      ] ]

let render state dispatch =
    match state.Drafts with
    | Remote.Empty -> div [] [ str "Still empty" ]
    | Loading -> Common.spinner
    | LoadError msg -> Common.errorMsg msg
    | Body loadedDrafts ->
        div [] [
            h1 [] [ str "Drafts" ]

            table [ ClassName "table table-bordered" ] [
                thead [] [
                    tr [] [
                        th [] [ str "ID" ]
                        th [] [ str "Title" ]
                        th [] [ str "Tags" ]
                        th [] [ str "Feauted?" ]
                        th [] [ str "Slug" ]
                        th [] [ str "Actions" ]
                    ]
                ]
                tbody [] [
                    for draft in loadedDrafts ->
                        let isPublishing = (state.PublishingDraft = Some draft.Id)
                        let isDeleting = (state.DeletingDraft = Some draft.Id)

                        let actionSection =
                            draftActions isDeleting isPublishing draft dispatch

                        let featuredButton =
                            let className =
                                if draft.Featured then
                                    "btn btn-success"
                                else
                                    "btn btn-secondary"

                            button [ ClassName className
                                     Style [ Margin 10 ]
                                     OnClick(fun ev -> dispatch (ToggleFeatured draft.Id)) ] [
                                str "Featured"
                            ]

                        tr [] [
                            td [] [ str (string draft.Id) ]
                            td [] [ str draft.Title ]
                            td [] [
                                str (String.concat ", " draft.Tags)
                            ]
                            td [] [ featuredButton ]
                            td [] [ str draft.Slug ]
                            td [ Style [ Width "340px" ] ] actionSection
                        ]
                ]
            ]
        ]

let init () =
    { Drafts = Remote.Empty
      PublishingDraft = None
      DeletingDraft = None
      IsTogglingFeatured = None },
    Cmd.none

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
                  Success =
                      function
                      | Ok drafts -> DraftsLoaded drafts
                      | Error authError -> AuthenticationError "User was unauthorize to view drafts" }

        nextState, loadDraftsCmd
    | DraftsLoaded draftsFromServer ->
        let nextState =
            { state with
                  Drafts = Body draftsFromServer }

        nextState, Cmd.none
    | DraftsLoadingError error ->
        let nextState =
            { state with
                  Drafts = LoadError error.Message }

        nextState, Cmd.none
    | AuthenticationError error -> state, Toastr.error (Toastr.message error)
    | AskPermissionToDeleteDraft _ when not (canTakeAction state) ->
        state, Toastr.info (Toastr.message "An action is already taking place")
    | AskPermissionToDeleteDraft draftId ->
        let handleConfirm =
            function
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
                  Success =
                      function
                      | Error authError -> DeleteDraftError "User was unauthorized"
                      | Ok DeleteDraftResult.DraftDeleted -> DraftDeleted
                      | Ok DeleteDraftResult.DraftDoesNotExist ->
                          DeleteDraftError "Draft does not seem to be in the database anymore"
                      | Ok DeleteDraftResult.DatabaseErrorWhileDeletingDraft ->
                          DeleteDraftError "Internal error of the server's database while publishing draft" }

        let nextState =
            { state with
                  DeletingDraft = Some draftId }

        nextState, deleteCmd
    | DraftDeleted ->
        state,
        Cmd.batch [ Toastr.success (Toastr.message "Draft deleted")
                    Cmd.ofMsg LoadDrafts ]
    | PublishDraft _ when not (canTakeAction state) ->
        state, Toastr.info (Toastr.message "An action is already taking place")
    | PublishDraft draftId ->
        let request = { Token = authToken; Body = draftId }

        let publishCmd =
            Cmd.fromAsync
                { Value = Server.api.publishDraft request
                  Error = fun ex -> PublishDraftError "Network error occured while publishing the draft"
                  Success =
                      function
                      | Error authError -> PublishDraftError "User is not authorized"
                      | Ok PublishDraftResult.DraftPublished -> DraftPublished
                      | Ok PublishDraftResult.DatabaseErrorWhilePublishingDraft ->
                          PublishDraftError "Internal error of the server's database while publishing draft"
                      | Ok PublishDraftResult.DraftDoesNotExist -> PublishDraftError "The draft does not exist anymore" }

        let nextState =
            { state with
                  PublishingDraft = Some draftId }

        nextState, publishCmd
    | DeleteDraftError errorMsg ->
        let nextState = { state with DeletingDraft = None }
        nextState, Toastr.error (Toastr.message errorMsg)
    | DraftPublished ->
        let nextState = { state with PublishingDraft = None }

        nextState,
        Cmd.batch [ Cmd.ofMsg LoadDrafts // reload
                    Toastr.success (Toastr.message "Draft published") ]
    | PublishDraftError errorMsg ->
        let nextState = { state with PublishingDraft = None }
        nextState, Toastr.error (Toastr.message errorMsg)
    | CancelDraftDeletion -> state, Toastr.info (Toastr.message "Phew, so you changed your mind after all.")
    | EditDraft draftId ->
        state,
        Urls.navigate [ Urls.admin
                        Urls.editPost
                        string draftId ]
    | ToggleFeatured postId ->
        let nextState =
            { state with
                  IsTogglingFeatured = Some postId }

        let request = { Token = authToken; Body = postId }

        let nextCmd =
            Cmd.fromAsync
                { Value = Server.api.togglePostFeatured request
                  Error = fun ex -> ToggleFeaturedFinished(Error "Network error while toggling post featured")
                  Success =
                      function
                      | Error authError -> ToggleFeaturedFinished(Error "User was unauthorized")
                      | Ok (Ok successMsg) -> ToggleFeaturedFinished(Ok successMsg)
                      | Ok (Error errorMsg) -> ToggleFeaturedFinished(Error errorMsg) }

        nextState, nextCmd
    | ToggleFeaturedFinished (Ok msg) ->
        match state.IsTogglingFeatured, state.Drafts with
        | Some postId, Body loadedPosts ->
            let nextCmd = Toastr.success (Toastr.message msg)

            // update the posts
            let updatedPosts =
                loadedPosts
                |> List.map
                    (fun post ->
                        if post.Id <> postId then
                            post
                        else
                            { post with
                                  Featured = not post.Featured })

            let nextState =
                { state with
                      IsTogglingFeatured = None
                      Drafts = Body updatedPosts }

            nextState, nextCmd
        | _, _ -> state, Cmd.none
    | ToggleFeaturedFinished (Error msg) ->
        let nextState = { state with IsTogglingFeatured = None }
        nextState, Toastr.error (Toastr.message msg)
    | DoNothing -> state, Cmd.none
