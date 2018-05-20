module Admin.Backoffice.State

open Elmish

open Admin.Backoffice.Types
open Admin.Backoffice

let update authToken msg state = 
    match msg with
    | NavigateTo page ->
        match page with 
        | Home ->  state, Urls.navigate [  Urls.admin ]
        | NewPost ->  state, Urls.navigate [ Urls.admin; Urls.newPost ]
        | Drafts -> state, Urls.navigate [ Urls.admin; Urls.drafts ]
        | PublishedPosts -> state, Urls.navigate [ Urls.admin; Urls.publishedPosts ];
        | Settings -> state, Urls.navigate [ Urls.admin; Urls.settings ];
        | _ -> state, Urls.navigate [ Urls.admin ]
          
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
    
    | PublishedPostsMsg articlesMsg ->
        let prevArticlesState = state.PublishedPostsState
        let nextArticlesState, nextArticlesCmd = PublishedPosts.State.update authToken articlesMsg prevArticlesState
        let nextBackofficeState = { state with PublishedPostsState = nextArticlesState }   
        let nextBackofficeCmd = Cmd.map PublishedPostsMsg nextArticlesCmd
        nextBackofficeState, nextBackofficeCmd
    
    | EditArticleMsg editArticleMsg ->
        let prevEditArticleState = state.EditArticleState
        let nextEditArticleState, nextEditArticleCmd = EditArticle.State.update authToken editArticleMsg prevEditArticleState
        let nextBackofficeState = { state with EditArticleState = nextEditArticleState }
        let nextBackofficeCmd = Cmd.map EditArticleMsg nextEditArticleCmd
        nextBackofficeState, nextBackofficeCmd

    | SettingsMsg settingsMsg ->
        let prevSettingState = state.SettingsState 
        let nextSettingState, nextSettingsCmd = Settings.State.update authToken settingsMsg prevSettingState 
        let nextBackofficeState = { state with SettingsState = nextSettingState }
        let nextBackofficeCmd = Cmd.map SettingsMsg nextSettingsCmd
        nextBackofficeState, nextBackofficeCmd
    
    | Logout ->  
        state, Cmd.none
    

let init() = 
    let newArticleState, newArticleCmd = NewArticle.State.init()
    let initialDraftsState, draftsCmd = Drafts.State.init() 
    let initialArticlesState, articlesCmd = PublishedPosts.State.init() 
    let initialEditArticleState, editArticleCmd = EditArticle.State.init()
    let initialSettingState, settingCmd = Settings.State.init()

    let initialState = {
        NewArticleState = newArticleState
        DraftsState = initialDraftsState
        PublishedPostsState = initialArticlesState
        EditArticleState = initialEditArticleState
        SettingsState = initialSettingState
    }

    initialState, Cmd.batch [ Cmd.map DraftsMsg draftsCmd
                              Cmd.map NewArticleMsg newArticleCmd
                              Cmd.map PublishedPostsMsg articlesCmd
                              Cmd.map EditArticleMsg editArticleCmd
                              Cmd.map SettingsMsg settingCmd ]