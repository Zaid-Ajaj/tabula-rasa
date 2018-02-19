module Posts.Types
open Shared.ViewModels
open System

type Page = 
    | AllPosts
    | Post of slug:string 

type Msg = 
    | LoadLatestPosts
    | LoadingPostsFinished of list<BlogPostItem>
    | LoadPost of slug:string
    | LoadPostFinished of content:string
    | LoadingPostsError 
    | ReadPost of slug:string

type State = {
    IsLoadingPosts: bool
    IsLoadingSinglePost : bool
    PostContent : string option
    Posts: BlogPostItem list 
    Error: string option
}