module Admin.Backoffice.State

open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation

open Admin.Backoffice.Types
open Admin.Backoffice

let update msg state = 
    match msg with
    | NavigateTo page ->
        match page with 
        | Home -> state, Navigation.newUrl "#admin"
        | NewArticle -> state, Navigation.newUrl "#admin/posts/new"
        | _ -> state, Navigation.newUrl "#admin"
        
    | NewArticleMsg newArticleMsg ->
        let prevArticleState = state.NewArticleState
        let nextState, nextCmd = NewArticle.State.update newArticleMsg prevArticleState
        let nextBackofficeState = 
            { state with NewArticleState = nextState }
        let nextCmd = Cmd.map NewArticleMsg nextCmd

        nextBackofficeState, nextCmd
    | Logout -> state, Cmd.none
    

let init() = 
    let newArticleState, newArticleCmd = NewArticle.State.init()
    
    let initialState = {
        NewArticleState = newArticleState
        CurrentPage = Home
    }

    initialState, Cmd.batch [ Cmd.map NewArticleMsg newArticleCmd ]