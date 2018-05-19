module Admin.Backoffice.PublishedPosts.Types

open Shared 

type State = {
    PublishedPosts : Remote<list<BlogPostItem>>
    DeletingPost : Option<int> 
    MakingDraft : Option<int>
}

type Msg = 
    | LoadPublishedPosts
    | PublishedPostsLoaded of list<BlogPostItem>
    | LoadPublishedPostsError of string 
    | AskPermissionToDeletePost of articleId:int
    | DeletePost of articleId:int
    | CancelPostDeletion 
    | PostDeleted
    | MakeIntoDraft of articleId:int 
    | DraftMade 
    | MakeDraftError of errorMsg:string
    | DeletePostError of string
    | EditPost of articleId:int 
    | DoNothing