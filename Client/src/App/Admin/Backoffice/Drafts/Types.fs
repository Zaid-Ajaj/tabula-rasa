module Admin.Backoffice.Drafts.Types

open Shared.ViewModels

type State = {
    Drafts : Remote<list<BlogPostItem>> 
}

type Msg = 
    | LoadDrafts 
    | DraftsLoaded of list<BlogPostItem>
    | DraftsLoadingError of exn
    | AuthenticationError of string