module Admin.State

open Elmish
open Admin.Types

let init() = 
    let login, loginCmd = Login.State.init()
    let backoffice, backofficeCmd = Backoffice.State.init()
    
    let initialAdminState =
      { SecurityToken = None
        Login = login
        Backoffice = backoffice
        CurrentPage = None } 

    let initialAdminCmd = 
        Cmd.batch [ Cmd.map LoginMsg loginCmd
                    Cmd.map BackofficeMsg backofficeCmd ]

    initialAdminState, initialAdminCmd
    
let update msg (state: State) =
    match msg with
    | SetCurrentPage page ->
        let nextState = 
          match state.SecurityToken with
          | None -> { state with CurrentPage = Some Login }
          | Some _ ->
            match page with
            | Login -> { state with CurrentPage = Some Login }
            | Backoffice backofficePage -> 
                { state with CurrentPage = Some page
                             Backoffice = { state.Backoffice with CurrentPage = backofficePage }  }
              
        nextState, Cmd.none
    | LoginMsg loginMsg ->
        match loginMsg with 
        // intercept a login success message
        | Login.Types.Msg.LoginSuccess token ->
            let nextState = 
                { state with Login = state.Login
                             SecurityToken = Some token
                             Backoffice = { state.Backoffice with CurrentPage = Backoffice.Types.Page.Home }
                             CurrentPage = Some (Backoffice (Backoffice.Types.Page.Home)) }
            nextState, Cmd.none
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
            let nextAdminState = 
              { state with Backoffice = nextBackofficeState
                           CurrentPage = Some nextAdminPage  }
            nextAdminState, Cmd.map BackofficeMsg nextBackofficeCmd