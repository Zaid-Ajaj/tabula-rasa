module Admin.State

open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Admin.Types

let init() = 
    let login, loginCmd = Login.State.init()
    let backoffice, backofficeCmd = Backoffice.State.init()
    
    let initialAdminState =
      { SecurityToken = None
        Login = login
        Backoffice = backoffice
        CurrentPage = Login } 

    let initialAdminCmd = 
        Cmd.batch [ Cmd.map LoginMsg loginCmd
                    Cmd.map BackofficeMsg backofficeCmd ]

    initialAdminState, initialAdminCmd
    
let update msg (state: State) =
    match msg with
    | SetCurrentPage page -> 
        // parent tells admin to change page 
        // admin will decide whether it is a valid operation or not and change accordingly
        match page with
        | Login ->
            match state.SecurityToken with
            | None -> { state with CurrentPage = page }, Cmd.none
            | Some token -> state, Navigation.newUrl "#admin"
        | Backoffice backoffice ->
            match state.SecurityToken with
            | None -> state, Navigation.newUrl "#login"
            | Some token -> { state with CurrentPage = page }, Cmd.none
    | LoginMsg loginMsg ->
        match loginMsg with 
        // intercept the LoginSuccess message dispatched by the child component
        | Login.Types.Msg.LoginSuccess token ->
            let nextState = 
                { state with Login = state.Login
                             SecurityToken = Some token }
            nextState, Navigation.newUrl "#admin"
        // propagate other messages to child component
        | _ -> 
            let nextLoginState, nextLoginCmd = Admin.Login.State.update loginMsg state.Login
            let nextAdminState = { state with Login = nextLoginState }
            nextAdminState, Cmd.map LoginMsg nextLoginCmd
    | BackofficeMsg msg ->
        match msg with 
        | Backoffice.Types.Msg.Logout -> 
            init()
        | _ -> 
            let prevBackofficeState = state.Backoffice
            let nextBackofficeState, nextBackofficeCmd = Backoffice.State.update msg prevBackofficeState
            let nextAdminPage = Admin.Types.Page.Backoffice nextBackofficeState.CurrentPage
            let nextAdminState = { state with Backoffice = nextBackofficeState }
            nextAdminState, Cmd.map BackofficeMsg nextBackofficeCmd