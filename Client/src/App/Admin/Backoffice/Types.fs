module Admin.Backoffice.Types

open Admin.Backoffice 

type Page = 
    | Home
    | NewArticle
    | Settings
    | Drafts
    | Published
    | Subscribers

type Msg = 
    | Logout 
    | NewArticleMsg of NewArticle.Types.Msg 
    | DraftsMsg of Drafts.Types.Msg 
    | NavigateTo of Page

type State = { 
    NewArticleState : NewArticle.Types.NewArticleState
    DraftsState : Drafts.Types.State
}