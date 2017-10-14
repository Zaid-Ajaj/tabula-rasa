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
        let validUsername = 
            String.IsNullOrWhiteSpace(state.InputUsername) |> not
            && state.InputUsername.Length > 5
        let validPassword = 
            String.IsNullOrWhiteSpace(state.InputPassword) |> not
            && state.InputPassword.Length > 5
        if not validUsername then 
          let errorMsg = 
            "Username must have at least 6 characters"
            |> Toastr.message 
            |> Toastr.withTitle "Client"
            |> Toastr.error
          state, errorMsg()
        elif not validPassword then
          let errorMsg = 
            "Password must have at least 6 characters"
            |> Toastr.message 
            |> Toastr.withTitle "Client"
            |> Toastr.error
          state, errorMsg()
        else 
          let nextState = { state with LoggingIn = true } 
          nextState, loginAsyncCmd
    | LoginSuccess token -> 
        let nextState = { state with LoggingIn = false }

        let successMsg = 
            "Succesfully logged in"
            |> Toastr.message 
            |> Toastr.withTitle "Client"
            |> Toastr.success
 
        nextState, successMsg()
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