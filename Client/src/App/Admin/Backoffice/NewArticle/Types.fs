module Admin.Backoffice.NewArticle.Types

type Msg = 
    | SetTitle of string
    | SetSlug of string
    | SetContent of string
    | AddTag of string
    | TogglePreview


type NewArticleState = {
    Title: string
    Slug: string
    Tags: string list
    Content: string
    Preview : bool
}
 

