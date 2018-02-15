module Admin.View

open Admin.Types


let render (state: State) dispatch = 
    match state.CurrentPage with
    | Login ->
        match state.SecurityToken with
        // routed to login page with no token -> show login page
        | None -> 
            Login.View.render state.Login (LoginMsg >> dispatch)
        // routed to login page with an already logged in user -> go to backoffice home
        | Some token -> 
            let backofficeState = { state.Backoffice with CurrentPage = Backoffice.Types.Page.Home }
            Backoffice.View.render backofficeState (BackofficeMsg >> dispatch)
    | Backoffice backofficePage ->
        match state.SecurityToken with
        // routed to backoffice page without token -> show login page
        | None -> 
            Login.View.render state.Login (LoginMsg >> dispatch)
        // routed to backoffice page with user logged in -> show requested backoffice page
        | Some token ->
            let backoffice = { state.Backoffice with CurrentPage = backofficePage }
            Backoffice.View.render backoffice (BackofficeMsg >> dispatch)
            