module Admin.Login.State

open Elmish
open Admin.Login.Types

let update msg (state: State) = 
    match msg with 
    | ChangeUsername name ->
        let nextState = { state with InputUsername = name }
        nextState, Cmd.none
    | ChangePassword pass ->
        let nextState = { state with InputPassword = pass }
        nextState, Cmd.none
    | otherMsg -> 
        state, Cmd.none
        
let init() = 
    { InputUsername = "";
      InputPassword = "";
      LoggingIn = false }, Cmd.none