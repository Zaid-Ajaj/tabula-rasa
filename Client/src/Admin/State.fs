module Admin.State

open Elmish
open Admin.Types

let update msg (state: State) =
    match msg with
    | LoginMsg msg ->
        let prevLoginState = state.Login
        let nextLoginState, nextLoginCmd = 
            Admin.Login.State.update msg prevLoginState
        { state with Login = nextLoginState }, Cmd.map LoginMsg nextLoginCmd
    | BackofficeMsg msg ->
        let prevBackofficeState = state.Backoffice
        let nextBackofficeState, nextBackofficeCmd = 
            Admin.Backoffice.State.update msg prevBackofficeState
        { state with Backoffice = nextBackofficeState }, Cmd.map BackofficeMsg nextBackofficeCmd

let init() = 
    let login, loginCmd = Admin.Login.State.init()
    let backoffice, backofficeCmd = Admin.Backoffice.State.init()
    let initialAdminState =
      { SecurityToken = None
        Login = login
        Backoffice = backoffice } 
    let initialAdminCmd = 
        Cmd.batch [ Cmd.map LoginMsg loginCmd
                    Cmd.map BackofficeMsg backofficeCmd ]
    initialAdminState, initialAdminCmd
