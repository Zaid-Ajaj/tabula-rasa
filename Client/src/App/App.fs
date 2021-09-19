module App

open Shared
open Elmish
open System
open Fable.Import.Browser

open Fable.Helpers.React
open Fable.Helpers.React.Props
open React.Responsive

type Page =
    // Pages of App
    | AboutPage
    // Sub pages of App
    | PostsPage of Posts.Page
    | AdminPage of Admin.Page

type AppMsg =
    // The messages coming from children
    | PostsMsg of Posts.Msg
    | AdminMsg of Admin.Msg
    // the app messages
    | LoadBlogInfo
    | BlogInfoLoaded of Result<BlogInfo, string>
    | BlogInfoLoadFailed of error: string
    | UrlUpdated of Page
    | NavigateTo of Page
    // specialized message coming from server via Elmish.Bridge
    | ServerMsg of RemoteServerMsg
    | DoNothing

type AppState =
    { // the state of the children
      Posts: Posts.State
      Admin: Admin.State
      // App's own state
      BlogInfo: Remote<BlogInfo>
      CurrentPage: Option<Page> }


let loadBlogInfo =
    Cmd.ofAsync
        Server.api.getBlogInfo
        ()
        BlogInfoLoaded
        (fun _ -> BlogInfoLoadFailed "Network error: could not retrieve initial blog information from server")


type PostsPage = Posts

let pageHash =
    function
    | Page.AboutPage -> Urls.about
    | PostsPage page ->
        match page with
        | Posts.AllPosts -> Urls.posts
        | Posts.Post postSlug -> Urls.combine [ Urls.posts; postSlug ]
    | AdminPage adminPage ->
        match adminPage with
        | Admin.LoginPage -> Urls.login
        | Admin.BackofficePage backofficePage ->
            match backofficePage with
            | Backoffice.HomePage -> Urls.admin
            | Backoffice.NewPostPage ->
                Urls.combine [ Urls.admin
                               Urls.newPost ]
            | Backoffice.DraftsPage -> Urls.combine [ Urls.admin; Urls.drafts ]
            | Backoffice.PublishedPostsPage ->
                Urls.combine [ Urls.admin
                               Urls.publishedPosts ]
            | Backoffice.SettingsPage ->
                Urls.combine [ Urls.admin
                               Urls.settings ]
            | Backoffice.EditArticlePage postId ->
                Urls.combine [ Urls.admin
                               Urls.editPost
                               string postId ]

/// Tries to parse a url into a page
let parseUrl (urlHash: string) =
    let segments =
        if urlHash.StartsWith "#" then
            urlHash.Substring(1, urlHash.Length - 1) // remove the hash sign
        else
            urlHash
        |> fun hash -> hash.Split '/' // split the url segments
        |> List.ofArray
        |> List.filter (String.IsNullOrWhiteSpace >> not)

    match segments with
    | [ Urls.about ] ->
        // the about page
        AboutPage |> Some
    | [ Urls.posts ] ->
        // all posts page
        Posts.AllPosts |> PostsPage |> Some
    | [ Urls.posts; postSlug ] ->
        // matches against a specific post by it's slug
        Posts.Post postSlug |> PostsPage |> Some
    | [ Urls.admin ] ->
        // the home page of the backoffice
        Backoffice.HomePage
        |> Admin.BackofficePage
        |> AdminPage
        |> Some
    | [ Urls.login ] ->
        // the login page
        Admin.LoginPage |> AdminPage |> Some
    | [ Urls.admin; Urls.drafts ] ->
        // the drafts page
        Backoffice.DraftsPage
        |> Admin.BackofficePage
        |> AdminPage
        |> Some
    | [ Urls.admin; Urls.publishedPosts ] ->
        // the page of published stories
        Backoffice.PublishedPostsPage
        |> Admin.BackofficePage
        |> AdminPage
        |> Some
    | [ Urls.admin; Urls.newPost ] ->
        // the new post page
        Backoffice.NewPostPage
        |> Admin.BackofficePage
        |> AdminPage
        |> Some
    | [ Urls.admin; Urls.settings ] ->
        // the settings page
        Backoffice.SettingsPage
        |> Admin.BackofficePage
        |> AdminPage
        |> Some
    | [ Urls.admin; Urls.editPost; Urls.Int postId ] ->
        // editing a post by the post id
        Backoffice.EditArticlePage postId
        |> Admin.BackofficePage
        |> AdminPage
        |> Some
    | _ -> None

let init () =
    let posts, postsCmd = Posts.init ()
    let admin, adminCmd = Admin.init ()

    let model =
        { BlogInfo = Remote.Empty
          CurrentPage = None
          Admin = admin
          Posts = posts }

    let initialPageCmd =
        // parse the current location and navigate to that location
        // directly when application starts up
        match parseUrl window.location.hash with
        | Some page -> Cmd.ofMsg (UrlUpdated page)
        | None ->
            // if unable to parse the location (-> unknown url)
            // then navigate to allPosts, here
            Posts.AllPosts
            |> PostsPage
            |> UrlUpdated
            |> Cmd.ofMsg

    model,
    Cmd.batch [ initialPageCmd
                Cmd.map PostsMsg postsCmd
                Cmd.map AdminMsg adminCmd
                Cmd.ofMsg LoadBlogInfo ]

let showInfo msg =
    Toastr.message msg
    |> Toastr.withTitle "Tabula Rasa"
    |> Toastr.info

/// What happens when the URL is updated, either from the application's components
/// or manually by the user isn't just simply changing the current view of the specific child
/// but rather, when a specific child is requested, then dispatch an appropriate message for
/// loading initial data of that, also here is where you define the logic of checking whether
/// a request to an admin page should be redirected to the login page if there is no user logged in
let handleUpdatedUrl nextPage state =
    match nextPage with
    | AboutPage ->
        let nextState =
            { state with
                  CurrentPage = Some AboutPage }

        nextState, Cmd.none
    | PostsPage postsPage ->
        match postsPage with
        | Posts.AllPosts ->
            // asking for all posts? the dispatch the LoadLatestPosts message to reload them
            let nextState =
                { state with
                      CurrentPage = Some(PostsPage postsPage) }

            let nextCmd =
                Cmd.ofMsg (PostsMsg Posts.LoadLatestPosts)

            nextState, nextCmd
        | Posts.Post postSlug ->
            // asking for a specific post by it's slug in the url?
            // then dispatch a message to load that post via the "LoadSinglePost" message
            let nextState =
                { state with
                      CurrentPage = Some(PostsPage postsPage) }

            let nextCmd =
                Cmd.ofMsg (PostsMsg(Posts.LoadSinglePost postSlug))

            nextState, nextCmd
    | AdminPage adminPage ->
        let nextAdminCmd =
            match adminPage with
            | Admin.LoginPage ->
                match state.Admin.SecurityToken with
                | None ->
                    // going to login page and there is no security token?
                    // then just login
                    Cmd.none
                | Some _ ->
                    // going to login page and there is already a security token
                    // then just navigate to admin home page because
                    // we are already logged in
                    Cmd.batch [ Urls.navigate [ Urls.admin ]
                                showInfo "Already logged in" ]
            | Admin.BackofficePage backofficePage ->
                match state.Admin.SecurityToken with
                | None ->
                    // navigating to one of the admins backoffice pages
                    // without a security token? then you need to login first
                    Cmd.batch [ Urls.navigate [ Urls.login ]
                                showInfo "You must be logged in first" ]
                | Some userSecurityToken ->
                    // then user is already logged in
                    // for each specific page, dispatch the appropriate message
                    // for initial loading of that data of that page
                    match backofficePage with
                    | Backoffice.DraftsPage ->
                        Drafts.LoadDrafts
                        |> Backoffice.DraftsMsg
                        |> Admin.BackofficeMsg
                        |> AdminMsg
                        |> Cmd.ofMsg
                    | Backoffice.PublishedPostsPage ->
                        PublishedPosts.LoadPublishedPosts
                        |> Backoffice.PublishedPostsMsg
                        |> Admin.BackofficeMsg
                        |> AdminMsg
                        |> Cmd.ofMsg
                    | Backoffice.SettingsPage ->
                        Settings.LoadBlogInfo
                        |> Backoffice.SettingsMsg
                        |> Admin.BackofficeMsg
                        |> AdminMsg
                        |> Cmd.ofMsg
                    | Backoffice.EditArticlePage postId ->
                        EditArticle.LoadArticleToEdit postId
                        |> Backoffice.EditArticleMsg
                        |> Admin.BackofficeMsg
                        |> AdminMsg
                        |> Cmd.ofMsg
                    | otherPage -> Cmd.none

        let nextState =
            { state with
                  CurrentPage = Some(AdminPage adminPage) }

        nextState, nextAdminCmd

let update msg state =
    match msg with
    | ServerMsg msg ->
        match msg with
        | ReloadPosts ->
            match state.CurrentPage with
            | Some (PostsPage Posts.AllPosts) ->
                let reloadPostsCmd =
                    Cmd.ofMsg (PostsMsg Posts.LoadLatestPosts)

                state, reloadPostsCmd
            | _ -> state, Cmd.none
    | PostsMsg msg ->
        let postsState, postsCmd =
            Posts.update state.Admin.SecurityToken state.Posts msg

        let appState = { state with Posts = postsState }
        let appCmd = Cmd.map PostsMsg postsCmd
        appState, appCmd
    // here, we are intercepting a "ChangesSaved" message triggered from settings
    // at this point, we have new blog info -> reload the blog info
    | AdminMsg (Admin.BackofficeMsg (Backoffice.SettingsMsg ((Settings.ChangesSaved msg)))) ->
        state, Cmd.ofMsg LoadBlogInfo
    | AdminMsg msg ->
        let nextAdminState, adminCmd = Admin.update msg state.Admin
        let nextAppState = { state with Admin = nextAdminState }
        let nextAppCmd = Cmd.map AdminMsg adminCmd
        nextAppState, nextAppCmd
    | LoadBlogInfo ->
        let nextState = { state with BlogInfo = Loading }
        nextState, loadBlogInfo
    | BlogInfoLoaded (Ok blogInfo) ->
        let nextState = { state with BlogInfo = Body blogInfo }

        let setPageTitle title =
            Fable.Import.Browser.document.title <- title

        nextState, Cmd.attemptFunc setPageTitle blogInfo.BlogTitle (fun ex -> DoNothing)
    | BlogInfoLoaded (Error errorMsg) ->
        let nextState =
            { state with
                  BlogInfo = LoadError errorMsg }

        nextState, Toastr.error (Toastr.message errorMsg)
    | BlogInfoLoadFailed msg ->
        let nextState = { state with BlogInfo = LoadError msg }
        nextState, Cmd.none
    | NavigateTo page ->
        let nextUrl = Urls.hashPrefix (pageHash page)
        state, Urls.newUrl nextUrl
    | DoNothing -> state, Cmd.none
    | UrlUpdated nextPage -> handleUpdatedUrl nextPage state

let menuItem label page currentPage dispatcher =
    div [ classList [ "menu-item", true
                      "menu-item-selected", Some page = currentPage ]
          OnClick(fun _ -> dispatcher (NavigateTo page)) ] [
        str label
    ]

/// Menu items that are only shown when an admin is logged in
let adminMenuItems state dispatch =
    [ menuItem "Home" (AdminPage(Admin.BackofficePage(Backoffice.HomePage))) state.CurrentPage dispatch

      menuItem
          "Published Posts"
          (AdminPage(Admin.BackofficePage(Backoffice.PublishedPostsPage)))
          state.CurrentPage
          dispatch

      menuItem "Drafts" (AdminPage(Admin.BackofficePage(Backoffice.DraftsPage))) state.CurrentPage dispatch

      menuItem "New Article" (AdminPage(Admin.BackofficePage(Backoffice.NewPostPage))) state.CurrentPage dispatch

      menuItem "Settings" (AdminPage(Admin.BackofficePage(Backoffice.SettingsPage))) state.CurrentPage dispatch ]

let sidebar (blogInfo: BlogInfo) state dispatch =
    aside [ ClassName "fit-parent child-space"
            Style [ TextAlign "center" ] ] [
        div [ Style [ TextAlign "center" ] ] [
            h3 [ Style [ Color "white" ] ] [
                str blogInfo.Name
            ]
            br []
            img [ ClassName "profile-img"
                  Src blogInfo.ProfileImageUrl ]
        ]
        div [ ClassName "quote" ] [
            str blogInfo.Bio
        ]
        menuItem "Posts" (PostsPage Posts.AllPosts) state.CurrentPage dispatch
        menuItem "About" (AboutPage) state.CurrentPage dispatch
        ofList [ if state.Admin.SecurityToken.IsSome then
                     yield! adminMenuItems state dispatch ]
    ]

let mobileHeader (blogInfo: BlogInfo) state dispatch =
    let navButton label selectedPage =
        div [ classList [ "btn btn-default", true
                          "btn-success", Some selectedPage = state.CurrentPage ]
              Style [ Margin 10; Width "40%" ]
              OnClick(fun _ -> dispatch (NavigateTo selectedPage)) ] [
            str label
        ]

    div [ ClassName "mobile-header" ] [
        h3 [] [ str blogInfo.Name ]
        img [ ClassName "profile-img"
              Src blogInfo.ProfileImageUrl ]
        div [] [ str blogInfo.Bio ]
        div [ ClassName "col-xs-12"
              Style [ TextAlign "center" ] ] [
            span [] [
                navButton "Posts" (PostsPage Posts.AllPosts)
                navButton "About" (AboutPage)
            ]
        ]
    ]

let main state dispatch =
    match state.CurrentPage with
    | Some AboutPage -> About.render state.BlogInfo
    | Some (PostsPage postsPage) ->
        let isAdminLoggedIn = state.Admin.SecurityToken.IsSome
        Posts.render postsPage isAdminLoggedIn state.Posts (PostsMsg >> dispatch)
    | Some (AdminPage adminPage) -> Admin.render adminPage state.Admin (AdminMsg >> dispatch)
    | None -> div [] []

let desktopApp blogInfo state dispatch =
    div [] [
        div [ ClassName "sidebar" ] [
            sidebar blogInfo state dispatch
        ]
        div [ ClassName "main-content" ] [
            main state dispatch
        ]
    ]

/// The mobile app view will be a simpler implementation
/// with no backoffice, just the posts and about page
let mobileApp blogInfo state dispatch =
    match state.CurrentPage with
    | None -> div [] []
    | Some page ->
        match page with
        | AboutPage ->
            div [] [
                mobileHeader blogInfo state dispatch
                div [ Style [ Padding 20 ] ] [
                    About.render (Body blogInfo)
                ]
            ]
        | PostsPage (Posts.AllPosts) ->
            // when viewing all posts, the same main view is re-used for mobile
            div [] [
                mobileHeader blogInfo state dispatch
                div [ Style [ Padding 20 ] ] [
                    main state dispatch
                ]
            ]
        | PostsPage (Posts.Post postSlug) ->
            match state.Posts.Post with
            | Remote.Empty -> div [] []
            | Loading -> Common.spinner
            | LoadError error -> Common.errorMsg error
            | Body post ->
                let goBackButton =
                    button [ ClassName "btn btn-success"
                             OnClick(fun _ -> dispatch (NavigateTo(PostsPage Posts.AllPosts))) ] [
                        span [] [
                            i [ ClassName "fa fa-arrow-left"
                                Style [ Margin 5 ] ] []
                            str "Go Back"
                        ]
                    ]

                div [ Style [ Padding 20 ] ] [
                    div [ ClassName "row" ] [
                        h4 [ Style [ Margin 20 ] ] [
                            str blogInfo.Name
                        ]
                        goBackButton
                    ]
                    hr []
                    Marked.marked [ Marked.Content post.Content
                                    Marked.Options [ Marked.Sanitize false ] ]
                ]
        | otherPage ->
            div [] [
                str "map other pages to just an empty view"
                br []
                str "because we don't to support backoffice from mobile"
                br []
                str "at least for now..."
            ]

let app blogInfo state dispatch =
    div [] [
        mediaQuery [ MinWidth 601 ] [
            desktopApp blogInfo state dispatch
        ]
        mediaQuery [ MaxWidth 600 ] [
            mobileApp blogInfo state dispatch
        ]
    ]

let render state dispatch =
    match state.BlogInfo with
    | Remote.Empty -> div [] []
    | Loading -> div [] []
    | LoadError ex ->
        h1 [] [
            str "Error loading initial blog data"
        ]
    | Body blogInfo -> app blogInfo state dispatch
