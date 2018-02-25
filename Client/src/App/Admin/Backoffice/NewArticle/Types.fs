module Admin.Backoffice.NewArticle.Types

type Msg = 
    | SetTitle of string
    | SetSlug of string
    | SetContent of string
    | SetTag of string
    | AddTag
    | AddTags of string []
    | RemoveTag of string
    | Publish
    | Published 
    | PublishError of string
    | TogglePreview


type NewArticleState = {
    Title: string
    Slug: string
    Tags: string list
    NewTag : string 
    Content: string
    Preview : bool
    IsPublishing : bool
}
 

 