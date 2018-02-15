module Admin.Backoffice.NewArticle.Types


type Msg = 
    | SetTitle of string
    | SetSlug of string
    | SetContent of string
    | AddTag of string
    | SetSummary of string
    | ShowInlineEditor 
    | SetInlineMarkdown of string


type NewArticleState = {
    Title: string
    Slug: string
    Tags: string list
    Summary: string
    Content: string
}


