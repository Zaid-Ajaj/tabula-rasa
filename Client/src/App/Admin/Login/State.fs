module Admin.Login.State

open System
open Elmish
open Admin.Login.Types
open Shared

let init() =
    { InputUsername = ""
      InputPassword = ""
      UsernameValidationErrors = []
      PasswordValidationErrors = []
      HasTriedToLogin = false
      LoginError = None
      CanLogin = false
      LoggingIn = false }, Cmd.batch [ Cmd.ofMsg UpdateValidationErrors ]

let validateInput (state : State) =
    let usernameRules =
        [ String.IsNullOrWhiteSpace(state.InputUsername), "Field 'Username' cannot be empty"
          state.InputUsername.Trim().Length < 5, "Field 'Username' must at least have 5 characters" ]
    
    let passwordRules =
        [ String.IsNullOrWhiteSpace(state.InputPassword), "Field 'Password' cannot be empty"
          state.InputPassword.Trim().Length < 5, "Field 'Password' must at least have 5 characters" ]
    
    let usernameValidationErrors =
        usernameRules
        |> List.filter fst
        |> List.map snd
    
    let passwordValidationErrors =
        passwordRules
        |> List.filter fst
        |> List.map snd
    
    usernameValidationErrors, passwordValidationErrors

let update msg (state : State) =
    match msg with
    | ChangeUsername name -> 
        let nextState = { state with InputUsername = name }
        nextState, Cmd.ofMsg UpdateValidationErrors
    | ChangePassword pass -> 
        let nextState = { state with InputPassword = pass }
        nextState, Cmd.ofMsg UpdateValidationErrors
    | UpdateValidationErrors -> 
        let usernameErrors, passwordErrors = validateInput state
        
        let nextState =
            { state with UsernameValidationErrors = usernameErrors
                         PasswordValidationErrors = passwordErrors }
        nextState, Cmd.ofMsg UpdateCanLogin
    | UpdateCanLogin -> 
        let canLogin =
            [ state.InputUsername.Trim().Length >= 5
              state.InputPassword.Trim().Length >= 5
              List.isEmpty state.UsernameValidationErrors
              List.isEmpty state.PasswordValidationErrors ]
            |> Seq.forall id
        
        let nextState = { state with CanLogin = canLogin }
        nextState, Cmd.none
    | Login -> 
        let state = { state with HasTriedToLogin = true }
        let usernameErrors, passwordErrors = validateInput state
        let startLogin = List.isEmpty usernameErrors && List.isEmpty passwordErrors
        if not startLogin then state, Cmd.none
        else 
            let nextState = { state with LoggingIn = true }
            
            let credentials =
                { Username = state.InputUsername
                  Password = state.InputPassword }
            nextState, Http.login credentials
    | LoginSuccess token -> 
        // this message is *intercepted* by parent component
        let nextState = { state with LoggingIn = false }
        
        let successFeedback =
            Toastr.message "Login succesful"
            |> Toastr.withTitle "Login"
            |> Toastr.success
        nextState, successFeedback
    | LoginFailed error -> 
        let nextState =
            { state with LoginError = Some error
                         LoggingIn = false }
        
        let showErrorMsg =
            Toastr.message error
            |> Toastr.withTitle "Login"
            |> Toastr.error
        
        nextState, showErrorMsg
