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
        | Home -> 
            state, Navigation.newUrl (Urls.hashPrefix Urls.admin)
        
        | NewArticle -> 
            state, Urls.navigate [ Urls.admin; Urls.newPost ]

        | Drafts -> 
            state, Cmd.batch [ Urls.navigate [ Urls.admin; Urls.drafts ];
                               Cmd.ofMsg (DraftsMsg Drafts.Types.Msg.LoadDrafts) ]
        | Articles -> 
            state, Cmd.batch [ Urls.navigate [ Urls.admin; Urls.publishedArticles ];
                               Cmd.ofMsg (ArticlesMsg Articles.Types.Msg.LoadArticles) ]
        | _ -> 
            state, Urls.navigate [ Urls.admin ]
        
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
    
    | EditArticleMsg editArticleMsg ->
        let prevEditArticleState = state.EditArticleState
        let nextEditArticleState, nextEditArticleCmd = EditArticle.State.update editArticleMsg prevEditArticleState
        let nextBackofficeState = { state with EditArticleState = nextEditArticleState }
        let nextBackofficeCmd = Cmd.map EditArticleMsg nextEditArticleCmd
        nextBackofficeState, nextBackofficeCmd
        
    | Logout ->  
        state, Cmd.none
    

let init() = 
    let newArticleState, newArticleCmd = NewArticle.State.init()
    let initialDraftsState, draftsCmd = Drafts.State.init() 
    let initialArticlesState, articlesCmd = Articles.State.init() 
    let initialEditArticleState, editArticleCmd = EditArticle.State.init()

    let initialState = {
        NewArticleState = newArticleState
        DraftsState = initialDraftsState
        ArticlesState = initialArticlesState
        EditArticleState = initialEditArticleState
    }

    initialState, Cmd.batch [ Cmd.map DraftsMsg draftsCmd
                              Cmd.map NewArticleMsg newArticleCmd
                              Cmd.map ArticlesMsg articlesCmd
                              Cmd.map EditArticleMsg editArticleCmd ]