module Posts.Types

type Post = string

type Msg = 
    | LoadLatestPosts
    | LoadingPostsFinished of Post[] option

type Model = {
    IsLoadingPosts: bool
    Posts: Post[]
    Error: string option
}