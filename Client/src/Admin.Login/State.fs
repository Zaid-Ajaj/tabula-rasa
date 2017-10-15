module Admin.Login.State

open System
open Elmish
open Admin.Login.Types
open Fable.PowerPack
open Shared.ViewModels

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
        elif state.InputUsername.Length < 5 then
            state, Feedback.usernameTooShort()
        elif String.IsNullOrWhiteSpace(state.InputPassword) then 
            state, Feedback.passwordEmpty()
        elif state.InputPassword.Length < 5 then
            state, Feedback.passwordTooShort()
        else 
          let nextState = { state with LoggingIn = true } 
          let credentials : LoginInfo = 
            { Username = state.InputUsername
              Password = state.InputPassword  }
          nextState, Http.login credentials
    | LoginSuccess token -> 
        let nextState = { state with LoggingIn = false }
        nextState, Feedback.loginSuccess()
    | LoginFailed error ->
        let nextState = 
            { state with LoginError = Some error 
                         LoggingIn = false }
        let feedback = Feedback.errorToast error
        nextState, feedback()

let init() = 
    { InputUsername = ""
      InputPassword = ""
      LoginError = None
      LoggingIn = false }, Cmd.none