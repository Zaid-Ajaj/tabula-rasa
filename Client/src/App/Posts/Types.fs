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
    | LoadSinglePostFinished of content:string
    | LoadSinglePostError of error:string
    | NavigateToPost of slug:string
     
type State = {
    PostContent : Remote<string>
    LatestPosts: Remote<list<BlogPostItem>> 
}