module Admin.Backoffice.State

open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation

open Admin.Backoffice.Types
open Admin.Backoffice

let update authToken msg state = 
    match msg with
    | NavigateTo page ->
        match page with 
        | Home -> state, Navigation.newUrl "#admin"
        | NewArticle -> state, Navigation.newUrl "#admin/new-post"
        | _ -> state, Navigation.newUrl "#admin"
        
    | NewArticleMsg newArticleMsg ->
        let prevArticleState = state.NewArticleState
        let nextArticleState, nextCmd = 
            NewArticle.State.update authToken newArticleMsg prevArticleState
        let nextBackofficeState = { state with NewArticleState = nextArticleState }
        let nextCmd = Cmd.map NewArticleMsg nextCmd

        nextBackofficeState, nextCmd
    | Logout -> 
        state, Cmd.none
    

let init() = 
    let newArticleState, newArticleCmd = NewArticle.State.init()
    
    let initialState = {
        NewArticleState = newArticleState
        CurrentPage = Home
    }

    initialState, Cmd.batch [ Cmd.map NewArticleMsg newArticleCmd ]