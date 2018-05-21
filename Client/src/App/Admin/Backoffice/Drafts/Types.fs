module Admin.Backoffice.Drafts.Types

open Shared

type State = {
    Drafts : Remote<list<BlogPostItem>> 
    PublishingDraft : Option<int>
    DeletingDraft : Option<int>
    IsTogglingFeatured : Option<int>
}

type Msg = 
    | LoadDrafts 
    | DraftsLoaded of list<BlogPostItem>
    | DraftsLoadingError of exn
    | AuthenticationError of string
    | AskPermissionToDeleteDraft of draftId:int
    | DeleteDraft of draftId:int
    | CancelDraftDeletion
    | PublishDraft of draftId:int
    | DraftPublished 
    | PublishDraftError of string
    | DraftDeleted 
    | DeleteDraftError of string
    | EditDraft of draftId:int
    | ToggleFeatured of postId:int
    | ToggleFeaturedFinished of Result<string, string>
    | DoNothing