module Admin

open Elmish

type Page =
    | LoginPage
    | BackofficePage of Backoffice.Page

type Msg =
    | LoginMsg of Login.Msg
    | BackofficeMsg of Backoffice.Msg

type State =
    { SecurityToken: string option
      Backoffice: Backoffice.State
      Login: Login.State }


let render currentPage (state: State) dispatch =
    match currentPage with
    | LoginPage ->
        match state.SecurityToken with
        // routed to login page with no token -> show login page
        | None -> Login.render state.Login (LoginMsg >> dispatch)
        // routed to login page with an already logged in user -> go to backoffice home
        | Some _ -> Backoffice.render Backoffice.Page.HomePage state.Backoffice (BackofficeMsg >> dispatch)
    | BackofficePage backofficePage ->
        match state.SecurityToken with
        // routed to backoffice page without token -> show login page
        | None -> Login.render state.Login (LoginMsg >> dispatch)
        // routed to backoffice page with user logged in -> show requested backoffice page
        | Some _ -> Backoffice.render backofficePage state.Backoffice (BackofficeMsg >> dispatch)

let init () =
    let login, loginCmd = Login.init ()
    let backoffice, backofficeCmd = Backoffice.init ()

    let initialAdminState =
        { SecurityToken = None
          Login = login
          Backoffice = backoffice }

    let initialAdminCmd =
        Cmd.batch [ Cmd.map LoginMsg loginCmd
                    Cmd.map BackofficeMsg backofficeCmd ]

    initialAdminState, initialAdminCmd

let update msg (state: State) =
    match msg with
    | LoginMsg loginMsg ->
        match loginMsg with
        // intercept the LoginSuccess message dispatched by the child component
        | Login.Msg.LoginSuccess token ->
            let nextState =
                { state with
                      Login = state.Login
                      SecurityToken = Some token }

            nextState, Urls.navigate [ Urls.admin ]
        // propagate other messages to child component
        | _ ->
            let nextLoginState, nextLoginCmd = Login.update loginMsg state.Login

            let nextAdminState = { state with Login = nextLoginState }
            nextAdminState, Cmd.map LoginMsg nextLoginCmd
    | BackofficeMsg msg ->
        match msg with
        | Backoffice.Msg.Logout ->
            // intercept logout message of the backoffice child
            let nextState, _ = init ()
            nextState, Urls.navigate [ Urls.posts ]
        | _ ->
            match state.SecurityToken with
            | Some token ->
                let prevBackofficeState = state.Backoffice

                let nextBackofficeState, nextBackofficeCmd =
                    // pass auth token down to backoffice
                    Backoffice.update token msg prevBackofficeState

                let nextAdminState =
                    { state with
                          Backoffice = nextBackofficeState }

                nextAdminState, Cmd.map BackofficeMsg nextBackofficeCmd
            | None -> state, Cmd.none
