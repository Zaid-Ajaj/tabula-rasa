module Admin.View

open Admin.Types

let render currentPage (state : State) dispatch =
    match currentPage with
    | Login -> 
        match state.SecurityToken with
        // routed to login page with no token -> show login page
        | None -> Login.View.render state.Login (LoginMsg >> dispatch)
        // routed to login page with an already logged in user -> go to backoffice home
        | Some _ -> Backoffice.View.render Backoffice.Types.Page.Home state.Backoffice (BackofficeMsg >> dispatch)
    | Backoffice backofficePage -> 
        match state.SecurityToken with
        // routed to backoffice page without token -> show login page
        | None -> Login.View.render state.Login (LoginMsg >> dispatch)
        // routed to backoffice page with user logged in -> show requested backoffice page
        | Some _ -> Backoffice.View.render backofficePage state.Backoffice (BackofficeMsg >> dispatch)
