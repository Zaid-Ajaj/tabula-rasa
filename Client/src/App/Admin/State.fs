module Admin.State

open Elmish
open Admin.Types

let init() =
    let login, loginCmd = Login.State.init()
    let backoffice, backofficeCmd = Backoffice.State.init()
    
    let initialAdminState =
        { SecurityToken = None
          Login = login
          Backoffice = backoffice }
    
    let initialAdminCmd =
        Cmd.batch [ Cmd.map LoginMsg loginCmd
                    Cmd.map BackofficeMsg backofficeCmd ]
    
    initialAdminState, initialAdminCmd

let update msg (state : State) =
    match msg with
    | LoginMsg loginMsg -> 
        match loginMsg with
        // intercept the LoginSuccess message dispatched by the child component
        | Login.Types.Msg.LoginSuccess token -> 
            let nextState =
                { state with Login = state.Login
                             SecurityToken = Some token }
            nextState, Urls.navigate [ Urls.admin ]
        // propagate other messages to child component
        | _ -> 
            let nextLoginState, nextLoginCmd = Admin.Login.State.update loginMsg state.Login
            let nextAdminState = { state with Login = nextLoginState }
            nextAdminState, Cmd.map LoginMsg nextLoginCmd
    | BackofficeMsg msg -> 
        match msg with
        | Backoffice.Types.Msg.Logout -> 
            // intercept logout message of the backoffice child
            let nextState, _ = init()
            nextState, Urls.navigate [ Urls.posts ]
        | _ -> 
            match state.SecurityToken with
            | Some token -> 
                let prevBackofficeState = state.Backoffice
                let nextBackofficeState, nextBackofficeCmd =
                    // pass auth token down to backoffice
                    Backoffice.State.update token msg prevBackofficeState
                let nextAdminState = { state with Backoffice = nextBackofficeState }
                nextAdminState, Cmd.map BackofficeMsg nextBackofficeCmd
            | None -> state, Cmd.none
