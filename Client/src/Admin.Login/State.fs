module Admin.Login.State

open System
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
        if String.IsNullOrWhiteSpace(state.InputUsername) then 
            state, Feedback.usernameEmpty()
        elif state.InputUsername.Length < 6 then
            state, Feedback.usernameTooShort()
        elif String.IsNullOrWhiteSpace(state.InputPassword) then 
            state, Feedback.passwordEmpty()
        elif state.InputPassword.Length < 6 then
            state, Feedback.passwordTooShort()
        else 
          let nextState = { state with LoggingIn = true } 
          nextState, loginAsyncCmd
    | LoginSuccess token -> 
        let nextState = { state with LoggingIn = false }
        nextState, Feedback.loginSuccess()
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