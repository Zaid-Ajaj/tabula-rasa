module Admin.Backoffice.EditArticle.Types

open Shared 

type State = 
    { ArticleToEdit : Remote<BlogPostItem> } 

type Msg = 
    | LoadArticleToEdit of int