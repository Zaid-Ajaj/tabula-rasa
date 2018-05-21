module Admin.Backoffice.PublishedPosts.Types

open Shared 

type State = {
    PublishedPosts : Remote<list<BlogPostItem>>
    DeletingPost : Option<int> 
    MakingDraft : Option<int>
    IsTogglingFeatured : Option<int>
}

type Msg = 
    | LoadPublishedPosts
    | LoadedPublishedPosts of Result<list<BlogPostItem>, string>
    | AskPermissionToDeletePost of articleId:int
    | DeletePost of articleId:int
    | CancelPostDeletion 
    | PostDeleted
    | MakeIntoDraft of articleId:int 
    | DraftMade 
    | MakeDraftError of errorMsg:string
    | DeletePostError of string
    | EditPost of postId:int 
    | ToggleFeatured of postId:int
    | ToggleFeaturedFinished of Result<string, string>
    | DoNothing