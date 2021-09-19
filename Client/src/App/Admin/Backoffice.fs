module Backoffice

type Page =
    | HomePage
    | NewPostPage
    | SettingsPage
    | DraftsPage
    | PublishedPostsPage
    | EditArticlePage of id: int

type Msg =
    | Logout
    | NewArticleMsg of NewArticle.Msg
    | DraftsMsg of Drafts.Msg
    | PublishedPostsMsg of PublishedPosts.Msg
    | EditArticleMsg of EditArticle.Msg
    | SettingsMsg of Settings.Msg
    | NavigateTo of Page

type State =
    { NewArticleState: NewArticle.NewArticleState
      EditArticleState: EditArticle.State
      DraftsState: Drafts.State
      PublishedPostsState: PublishedPosts.State
      SettingsState: Settings.State }


open Fable.Helpers.React.Props
open Fable.Helpers.React

let leftIcon name =
    span [ Style [ Margin 10 ] ] [
        i [ ClassName(sprintf "fa fa-%s" name) ] []
    ]

let cardContainer child =
    div [ ClassName "card admin-section" ] [
        div [ ClassName "card-block" ] [
            div [ ClassName "card-title"
                  Style [ Margin 20 ] ] [
                child
            ]
        ]
    ]

let stories =
    div [] [
        h3 [] [ leftIcon "book"; str "Stories" ]
        p [] [
            str "Stories that you have published for the world to see."
        ]
    ]
    |> cardContainer

let drafts =
    div [] [
        h3 [] [
            leftIcon "file-text-o"
            str "Drafts"
        ]
        p [] [
            str "Articles that you are still working on and havn't published yet."
        ]
    ]
    |> cardContainer

let settings =
    div [] [
        h3 [] [
            leftIcon "cogs"
            str "Settings"
        ]
        p [] [
            str "View and edit the settings of the blog and your profile."
        ]
    ]
    |> cardContainer

let writeArticle =
    div [] [
        h3 [] [
            leftIcon "plus"
            str "New Article"
        ]
        p [] [
            str "A story is the best way to share your ideas with the world."
        ]
    ]
    |> cardContainer

let subscribers =
    div [] [
        h3 [] [
            leftIcon "users"
            str "Subscribers"
        ]
        p [] [
            str "View who subscribes to your blog"
        ]
    ]
    |> cardContainer

let oneThirdPage child page dispatch =
    div [ ClassName "col-md-4"
          OnClick(fun _ -> dispatch (NavigateTo page)) ] [
        child
    ]

let logout dispatch =
    div [ ClassName "col-md-4"
          OnClick(fun _ -> dispatch Logout) ] [
        cardContainer
        <| div [] [
            h3 [] [
                leftIcon "power-off"
                str "Logout"
            ]
            p [] [ str "Return to your home page" ]
           ]
    ]

let homePage dispatch =
    div [ Style [ PaddingLeft 30 ] ] [
        div [ ClassName "row" ] [
            oneThirdPage stories PublishedPostsPage dispatch
            oneThirdPage drafts DraftsPage dispatch
            oneThirdPage settings SettingsPage dispatch
            oneThirdPage writeArticle NewPostPage dispatch
            logout dispatch
        ]
    ]

let render currentPage (state: State) dispatch =
    match currentPage with
    | HomePage -> homePage dispatch
    | NewPostPage -> NewArticle.render state.NewArticleState (NewArticleMsg >> dispatch)
    | DraftsPage -> Drafts.render state.DraftsState (DraftsMsg >> dispatch)
    | PublishedPostsPage -> PublishedPosts.render state.PublishedPostsState (PublishedPostsMsg >> dispatch)
    | EditArticlePage articleId -> EditArticle.render state.EditArticleState (EditArticleMsg >> dispatch)
    | SettingsPage -> Settings.render state.SettingsState (SettingsMsg >> dispatch)


open Elmish

let update authToken msg state =
    match msg with
    | NavigateTo page ->
        match page with
        | HomePage -> state, Urls.navigate [ Urls.admin ]
        | NewPostPage ->
            state,
            Urls.navigate [ Urls.admin
                            Urls.newPost ]
        | DraftsPage ->
            state,
            Urls.navigate [ Urls.admin
                            Urls.drafts ]
        | PublishedPostsPage ->
            state,
            Urls.navigate [ Urls.admin
                            Urls.publishedPosts ]
        | SettingsPage ->
            state,
            Urls.navigate [ Urls.admin
                            Urls.settings ]
        | _ -> state, Urls.navigate [ Urls.admin ]
    | NewArticleMsg newArticleMsg ->
        let prevArticleState = state.NewArticleState

        let nextArticleState, nextNewArticleCmd =
            NewArticle.update authToken newArticleMsg prevArticleState

        let nextBackofficeState =
            { state with
                  NewArticleState = nextArticleState }

        let nextBackofficeCmd = Cmd.map NewArticleMsg nextNewArticleCmd
        nextBackofficeState, nextBackofficeCmd
    | DraftsMsg draftsMsg ->
        let prevDraftsState = state.DraftsState

        let nextDraftsState, nextDraftsCmd =
            Drafts.update authToken draftsMsg prevDraftsState

        let nextBackofficeState =
            { state with
                  DraftsState = nextDraftsState }

        let nextBackofficeCmd = Cmd.map DraftsMsg nextDraftsCmd
        nextBackofficeState, nextBackofficeCmd
    | PublishedPostsMsg articlesMsg ->
        let prevArticlesState = state.PublishedPostsState

        let nextArticlesState, nextArticlesCmd =
            PublishedPosts.update authToken articlesMsg prevArticlesState

        let nextBackofficeState =
            { state with
                  PublishedPostsState = nextArticlesState }

        let nextBackofficeCmd =
            Cmd.map PublishedPostsMsg nextArticlesCmd

        nextBackofficeState, nextBackofficeCmd
    | EditArticleMsg editArticleMsg ->
        let prevEditArticleState = state.EditArticleState

        let nextEditArticleState, nextEditArticleCmd =
            EditArticle.update authToken editArticleMsg prevEditArticleState

        let nextBackofficeState =
            { state with
                  EditArticleState = nextEditArticleState }

        let nextBackofficeCmd =
            Cmd.map EditArticleMsg nextEditArticleCmd

        nextBackofficeState, nextBackofficeCmd
    | SettingsMsg settingsMsg ->
        let prevSettingState = state.SettingsState

        let nextSettingState, nextSettingsCmd =
            Settings.update authToken settingsMsg prevSettingState

        let nextBackofficeState =
            { state with
                  SettingsState = nextSettingState }

        let nextBackofficeCmd = Cmd.map SettingsMsg nextSettingsCmd
        nextBackofficeState, nextBackofficeCmd
    | Logout -> state, Cmd.none

let init () =
    let newArticleState, newArticleCmd = NewArticle.init ()
    let initialDraftsState, draftsCmd = Drafts.init ()
    let initialArticlesState, articlesCmd = PublishedPosts.init ()
    let initialEditArticleState, editArticleCmd = EditArticle.init ()
    let initialSettingState, settingCmd = Settings.init ()

    let initialState =
        { NewArticleState = newArticleState
          DraftsState = initialDraftsState
          PublishedPostsState = initialArticlesState
          EditArticleState = initialEditArticleState
          SettingsState = initialSettingState }

    initialState,
    Cmd.batch [ Cmd.map DraftsMsg draftsCmd
                Cmd.map NewArticleMsg newArticleCmd
                Cmd.map PublishedPostsMsg articlesCmd
                Cmd.map EditArticleMsg editArticleCmd
                Cmd.map SettingsMsg settingCmd ]
