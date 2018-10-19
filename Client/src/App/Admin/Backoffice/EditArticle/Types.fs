module Admin.Backoffice.EditArticle.Types

open Shared

type State =
    { ArticleToEdit : Remote<BlogPostItem>
      Preview : bool
      SavingChanges : bool }

type Msg =
    | LoadArticleToEdit of postId : int
    | ArticleLoaded of BlogPostItem
    | LoadArticleError of string
    | SetSlug of string
    | SetTitle of string
    | SetContent of string
    | TogglePreview
    | SaveChanges
    | SavedChanges
    | SaveChangesError of string
    | AddTags of string []
    | DoNothing
