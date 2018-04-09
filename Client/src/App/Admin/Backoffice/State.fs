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
        | Drafts -> state, Cmd.batch [ Navigation.newUrl "#admin/drafts";
                                       Cmd.ofMsg (DraftsMsg Drafts.Types.Msg.LoadDrafts) ]
        | _ -> state, Navigation.newUrl "#admin"
        
    | NewArticleMsg newArticleMsg ->
        let prevArticleState = state.NewArticleState
        let nextArticleState, nextNewArticleCmd = 
            NewArticle.State.update authToken newArticleMsg prevArticleState
        let nextBackofficeState = { state with NewArticleState = nextArticleState }
        let nextBackofficeCmd = Cmd.map NewArticleMsg nextNewArticleCmd

        nextBackofficeState, nextBackofficeCmd

    | DraftsMsg draftsMsg ->
        let prevDraftsState = state.DraftsState
        let nextDraftsState, nextDraftsCmd = 
            Drafts.State.update authToken draftsMsg prevDraftsState
        let nextBackofficeState = { state with DraftsState = nextDraftsState }   
        let nextBackofficeCmd = Cmd.map DraftsMsg nextDraftsCmd

        nextBackofficeState, nextBackofficeCmd
    | Logout ->  
        state, Cmd.none
    

let init() = 
    let newArticleState, newArticleCmd = NewArticle.State.init()
    let initialDraftsState, draftsCmd = Drafts.State.init() 
    
    let initialState = {
        NewArticleState = newArticleState
        DraftsState = initialDraftsState
    }

    initialState, Cmd.batch [ Cmd.map DraftsMsg draftsCmd
                              Cmd.map NewArticleMsg newArticleCmd ]