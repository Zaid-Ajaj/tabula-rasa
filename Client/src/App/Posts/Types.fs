module Posts.Types
open Shared.ViewModels

type Msg = 
    | LoadLatestPosts
    | LoadingPostsFinished of list<BlogPostItem>
    | LoadingPostsError 

type Model = {
    IsLoadingPosts: bool
    Posts: BlogPostItem list
    Error: string option
}