module Admin.View

open Admin.Types


let render (state: State) dispatch = 
    match state.SecurityToken with
    | None -> 
        let loginState = state.Login
        let loginView = Login.View.render loginState (LoginMsg >> dispatch)
        loginView
    | Some token -> 
        let backofficeState = state.Backoffice
        let backofficeView = Backoffice.View.render backofficeState (BackofficeMsg >> dispatch)
        backofficeView