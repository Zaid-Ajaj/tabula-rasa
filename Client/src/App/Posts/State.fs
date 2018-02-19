module Posts.State

open Elmish
open Elmish.Browser.Navigation
open Posts.Types
open Fable.PowerPack
open Shared.ViewModels

let server = Server.createProxy()

let loadPostsCmd = 
    Cmd.ofAsync server.getPosts ()
        LoadingPostsFinished
        (fun ex -> LoadingPostsError)

let loadSinglePostCmd slug = 
    Cmd.ofAsync server.getPostBySlug slug
        (function | Some post -> LoadPostFinished post 
                  | None -> LoadingPostsError)
        (fun ex -> LoadingPostsError)

let update (state: State) (msg: Msg) = 
    match msg with
    | LoadLatestPosts ->
        let nextState = { state with IsLoadingPosts = true }
        nextState, loadPostsCmd
    | ReadPost slug ->
        state, Navigation.newUrl ("#posts/" + slug)
    | LoadPost slug ->
        let nextState = { state with IsLoadingSinglePost = true }
        nextState, loadSinglePostCmd slug
    | LoadPostFinished content ->
        let nextState = { state with PostContent = Some content; IsLoadingSinglePost = false }
        nextState, Cmd.none
    | LoadingPostsFinished posts ->
        let nextState = 
            { state with 
               Posts = posts
               IsLoadingPosts = false
               Error = None }
        nextState, Cmd.none
    | LoadingPostsError ->
        let nextState = 
            { state with 
                Posts = [] 
                IsLoadingPosts = false
                Error = Some "Error while loading latest posts" }
        nextState, Cmd.none

let init() =
    let initialModel =  
     {  Posts = []
        PostContent = None
        IsLoadingSinglePost = false
        IsLoadingPosts = false
        Error = None }

    initialModel, Cmd.ofMsg LoadLatestPosts