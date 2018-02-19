module Posts.Types
open Shared.ViewModels
open System

type Page = 
    | AllPosts
    | Post of slug:string 

type Msg =
    | LoadLatestPosts  
    | LoadLatestPostsFinished of list<BlogPostItem>
    | LoadLatestPostsError of exn
    | LoadSinglePost of slug:string
    | LoadSinglePostFinished of content:string
    | LoadSinglePostError of exn
    | NavigateToPost of slug:string
     
type State = {
    PostContent : Remote<string>
    LatestPosts: Remote<list<BlogPostItem>> 
}