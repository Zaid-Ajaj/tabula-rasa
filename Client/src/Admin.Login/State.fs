module Admin.Login.State

open Elmish
open Admin.Login.Types
open Fable.PowerPack

let loginAsync() = 
    promise { 
        do! Promise.sleep 1500
        return "my secure token"
    }

let loginAsyncCmd = 
    Cmd.ofPromise 
        loginAsync ()
        LoginSuccess
        (fun ex -> LoginFailed "Error")

let update msg (state: State) = 
    match msg with 
    | ChangeUsername name ->
        let nextState = { state with InputUsername = name }
        nextState, Cmd.none
    | ChangePassword pass ->
        let nextState = { state with InputPassword = pass }
        nextState, Cmd.none
    | Login ->
        let nextState = { state with LoggingIn = true } 
        nextState, loginAsyncCmd
    | LoginSuccess token -> 
        let nextState = { state with LoggingIn = false }
        nextState, Cmd.none
    | LoginFailed error ->
        let nextState = 
            { state with LoginError = Some error 
                         LoggingIn = false }
        nextState, Cmd.none

let init() = 
    { InputUsername = ""
      InputPassword = ""
      LoginError = None
      LoggingIn = false }, Cmd.none