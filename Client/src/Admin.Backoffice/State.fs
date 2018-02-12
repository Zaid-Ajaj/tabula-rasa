module Admin.Backoffice.State

open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation

open Admin.Backoffice.Types
open Admin.Backoffice

let update msg state = 
    match msg with
    | SetCurrentPage page ->
        let nextState = { state with CurrentPage = page }
        nextState, Cmd.none 
        
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