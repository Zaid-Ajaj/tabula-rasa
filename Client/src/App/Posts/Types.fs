module Posts.Types

open Shared

type Page = 
    | AllPosts
    | Post of slug:string 

type Msg =
    | LoadLatestPosts  
    | LoadLatestPostsFinished of list<BlogPostItem>
    | LoadLatestPostsError of error:string
    | LoadSinglePost of slug:string
    | LoadSinglePostFinished of BlogPostItem
    | LoadSinglePostError of error:string
    | NavigateToPost of slug:string
    | EditPost of postId:int
    | AskPermissionToDeletePost of postId:int
    | DeletePost of postId:int 
    | CancelPostDeletion
    | PostDeletedSuccessfully 
    | DeletePostError of error:string
    | DoNothing

type State = {
    Post : Remote<BlogPostItem>
    LatestPosts: Remote<list<BlogPostItem>>
    DeletingPost: Option<int>
}