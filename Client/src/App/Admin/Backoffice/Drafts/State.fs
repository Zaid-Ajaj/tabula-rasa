module Admin.Backoffice.Drafts.State

open Shared.DomainModels
open Shared.ViewModels
open Admin.Backoffice.Drafts.Types 
open Elmish 

let server = Server.createProxy()

let init () = 
    { Drafts = Empty }, Cmd.none

let update authToken (msg: Msg) (state: State) = 
    match msg with 
    | LoadDrafts -> 
        let nextState = { state with Drafts = Loading }
        nextState, Cmd.ofAsync server.getDrafts (AuthToken(authToken)) 
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