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
    | SetCurrentPage of Page

type State = { 
    NewArticleState : NewArticle.Types.NewArticleState
    CurrentPage : Page
}