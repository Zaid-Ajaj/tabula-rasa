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
        | Articles -> state, Cmd.batch [ Navigation.newUrl "#admin/published-articles"
                                         Cmd.ofMsg (ArticlesMsg Articles.Types.Msg.LoadArticles) ]
        | _ -> state, Navigation.newUrl "#admin"
        
    | NewArticleMsg newArticleMsg ->
        let prevArticleState = state.NewArticleState
        let nextArticleState, nextNewArticleCmd = NewArticle.State.update authToken newArticleMsg prevArticleState
        let nextBackofficeState = { state with NewArticleState = nextArticleState }
        let nextBackofficeCmd = Cmd.map NewArticleMsg nextNewArticleCmd
        nextBackofficeState, nextBackofficeCmd

    | DraftsMsg draftsMsg ->
        let prevDraftsState = state.DraftsState
        let nextDraftsState, nextDraftsCmd = Drafts.State.update authToken draftsMsg prevDraftsState
        let nextBackofficeState = { state with DraftsState = nextDraftsState }   
        let nextBackofficeCmd = Cmd.map DraftsMsg nextDraftsCmd
        nextBackofficeState, nextBackofficeCmd
    
    | ArticlesMsg articlesMsg ->
        let prevArticlesState = state.ArticlesState
        let nextArticlesState, nextArticlesCmd = Articles.State.update authToken articlesMsg prevArticlesState
        let nextBackofficeState = { state with ArticlesState = nextArticlesState }   
        let nextBackofficeCmd = Cmd.map ArticlesMsg nextArticlesCmd
        nextBackofficeState, nextBackofficeCmd

    | Logout ->  
        state, Cmd.none
    

let init() = 
    let newArticleState, newArticleCmd = NewArticle.State.init()
    let initialDraftsState, draftsCmd = Drafts.State.init() 
    let initialArticlesState, articlesCmd = Articles.State.init() 

    let initialState = {
        NewArticleState = newArticleState
        DraftsState = initialDraftsState
        ArticlesState = initialArticlesState
    }

    initialState, Cmd.batch [ Cmd.map DraftsMsg draftsCmd
                              Cmd.map NewArticleMsg newArticleCmd
                              Cmd.map ArticlesMsg articlesCmd ]