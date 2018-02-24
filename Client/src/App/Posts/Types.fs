module Posts.Types
open Shared.ViewModels

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
     
type State = {
    Post : Remote<BlogPostItem>
    LatestPosts: Remote<list<BlogPostItem>>
}