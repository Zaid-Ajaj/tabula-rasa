module Admin.Backoffice.NewArticle.Types


type Msg = 
    | SetTitle of string
    | SetSlug of string
    | AddTag of string
    | SetSummry of string
    | ShowInlineEditor 
    | SetInlineMarkdown of string


type NewArticleState = {
    Title: string
    Slug: string
    Tags: string list
    Summary: string
    Content: string
}


