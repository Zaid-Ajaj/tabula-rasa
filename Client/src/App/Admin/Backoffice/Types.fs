module Admin.Backoffice.Types

open Admin.Backoffice

type Page =
    | Home
    | NewPost
    | Settings
    | Drafts
    | PublishedPosts
    | EditArticle of id : int

type Msg =
    | Logout
    | NewArticleMsg of NewArticle.Types.Msg
    | DraftsMsg of Drafts.Types.Msg
    | PublishedPostsMsg of PublishedPosts.Types.Msg
    | EditArticleMsg of EditArticle.Types.Msg
    | SettingsMsg of Settings.Types.Msg
    | NavigateTo of Page

type State =
    { NewArticleState : NewArticle.Types.NewArticleState
      EditArticleState : EditArticle.Types.State
      DraftsState : Drafts.Types.State
      PublishedPostsState : PublishedPosts.Types.State
      SettingsState : Settings.Types.State }
