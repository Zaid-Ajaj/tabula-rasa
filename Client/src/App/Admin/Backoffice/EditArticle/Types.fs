module Admin.Backoffice.EditArticle.Types

open Shared 

type State = 
    { ArticleId : Option<int>
      ArticleToEdit : Remote<BlogPostItem>
      Preview : bool
      SavingChanges : bool } 

type Msg = 
    | LoadArticleToEdit
    | ArticleLoaded of BlogPostItem 
    | LoadArticleError of string
    | SetSlug of string 
    | SetTitle of string 
    | SetContent of string
    | TogglePreview
    | SaveChanges
    | AddTags of string []
    | DoNothing